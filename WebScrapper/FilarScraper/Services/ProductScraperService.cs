using System.Text.Json;
using FilarScraper.Configuration;
using FilarScraper.Models;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FilarScraper.Services;

public class ProductScraperService : IProductScraperService
{
    private readonly HttpClient _httpClient;
    private readonly ICombinationInfoService _combinationInfoService;
    private readonly ILogger<ProductScraperService> _logger;
    private readonly ScraperOptions _options;

    public ProductScraperService(
        HttpClient httpClient,
        ICombinationInfoService combinationInfoService,
        ILogger<ProductScraperService> logger,
        IOptions<ScraperOptions> options)
    {
        _httpClient = httpClient;
        _combinationInfoService = combinationInfoService;
        _logger = logger;
        _options = options.Value;
    }

    public async Task<ScraperResult> ScrapeAsync(string url, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching product page: {Url}", url);

        var html = await _httpClient.GetStringAsync(url, cancellationToken);

        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var productName = ExtractProductName(doc);
        _logger.LogInformation("Product found: {Name}", productName);

        var productTemplateId = ExtractProductTemplateId(doc);
        _logger.LogInformation("Product template ID: {Id}", productTemplateId);

        var colorSwatches = ExtractColorSwatches(doc);
        _logger.LogInformation("Color swatches detected: {Count}", colorSwatches.Count);

        // Resolve images for each variant via the combination info API (parallel)
        var semaphore = new SemaphoreSlim(_options.MaxParallelDownloads, _options.MaxParallelDownloads);
        var variantTasks = colorSwatches.Select(async swatch =>
        {
            await semaphore.WaitAsync(cancellationToken);
            try
            {
                var (productId, images) = await _combinationInfoService.GetVariantImagesAsync(
                    productTemplateId,
                    swatch.AttributeValueId,
                    swatch.ColorName,
                    cancellationToken);

                _logger.LogInformation(
                    "  [{Hex}] {Color} → productId={PId}, images={N}",
                    swatch.HexColor, swatch.ColorName, productId, images.Count);

                return new ProductVariant
                {
                    ProductId = productId,
                    AttributeValueId = swatch.AttributeValueId,
                    ColorName = swatch.ColorName,
                    HexColor = swatch.HexColor,
                    ProductName = productName,
                    Price = swatch.Price,
                    Images = images
                };
            }
            finally
            {
                semaphore.Release();
            }
        });

        var variants = (await Task.WhenAll(variantTasks)).ToList();

        return new ScraperResult
        {
            ProductName = productName,
            SourceUrl = url,
            Variants = variants,
            OutputDirectory = _options.OutputDirectory
        };
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private static string ExtractProductName(HtmlDocument doc)
    {
        var h1 = doc.DocumentNode.SelectSingleNode("//h1[@itemprop='name']");
        return h1?.InnerText.Trim() ?? "Unknown Product";
    }

    private static int ExtractProductTemplateId(HtmlDocument doc)
    {
        var input = doc.DocumentNode.SelectSingleNode(
            "//input[@name='product_template_id']");
        if (input is null) return 0;
        return int.TryParse(input.GetAttributeValue("value", ""), out var id) ? id : 0;
    }

    /// <summary>
    /// Extracts raw color swatch data from the HTML — without resolving images yet.
    /// </summary>
    private static List<ColorSwatch> ExtractColorSwatches(HtmlDocument doc)
    {
        var swatches = new List<ColorSwatch>();
        var seen = new HashSet<int>();

        // Current price from the active variant
        var priceNode = doc.DocumentNode.SelectSingleNode(
            "//span[@itemprop='price']");
        decimal.TryParse(
            priceNode?.InnerText.Trim(),
            System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.InvariantCulture,
            out var basePrice);

        var colorInputs = doc.DocumentNode.SelectNodes(
            "//input[contains(@class,'js_variant_change') and @data-attribute_name='PLA Color']");

        if (colorInputs is null) return swatches;

        foreach (var input in colorInputs)
        {
            var valueIdStr = input.GetAttributeValue("data-value_id", "");
            if (!int.TryParse(valueIdStr, out var valueId) || !seen.Add(valueId))
                continue;

            var colorName = input.GetAttributeValue("data-value_name", "");
            if (string.IsNullOrEmpty(colorName)) continue;

            var attrValueIdStr = input.GetAttributeValue("data-attribute-value-id", "");
            if (!int.TryParse(attrValueIdStr, out var attrValueId))
                attrValueId = valueId; // fallback

            var label = input.ParentNode;
            var hexColor = ExtractHexFromLabel(label);

            swatches.Add(new ColorSwatch(attrValueId, colorName, hexColor, basePrice));
        }

        return swatches;
    }

    private static string ExtractHexFromLabel(HtmlNode? label)
    {
        if (label is null) return "#000000";
        var style = label.GetAttributeValue("style", "");
        var idx = style.IndexOf("background:", StringComparison.OrdinalIgnoreCase);
        if (idx < 0) return "#000000";
        var value = style[(idx + "background:".Length)..].Trim().TrimEnd(';').Trim();
        return value.StartsWith('#') ? value : $"#{value}";
    }

    public static string SanitizeName(string name)
    {
        var invalid = Path.GetInvalidFileNameChars();
        return string.Concat(name.Select(c => invalid.Contains(c) ? '_' : c))
                     .Replace(' ', '_');
    }

    // Internal DTO — not exposed outside
    private record ColorSwatch(int AttributeValueId, string ColorName, string HexColor, decimal Price);
}
