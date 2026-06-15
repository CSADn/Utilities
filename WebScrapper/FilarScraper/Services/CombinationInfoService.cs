using System.Net.Http.Json;
using System.Text.Json;
using FilarScraper.Configuration;
using FilarScraper.Models;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FilarScraper.Services;

public class CombinationInfoService : ICombinationInfoService
{
    private const string Endpoint = "/website_sale/get_combination_info";

    private readonly HttpClient _httpClient;
    private readonly ILogger<CombinationInfoService> _logger;
    private readonly ScraperOptions _options;

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    public CombinationInfoService(
        HttpClient httpClient,
        ILogger<CombinationInfoService> logger,
        IOptions<ScraperOptions> options)
    {
        _httpClient = httpClient;
        _logger = logger;
        _options = options.Value;
    }

    public async Task<(int ProductId, List<ProductImage> Images)> GetVariantImagesAsync(
        int productTemplateId,
        int attributeValueId,
        string colorName,
        CancellationToken cancellationToken = default)
    {
        var request = new JsonRpcRequest
        {
            Params = new CombinationInfoParams
            {
                ProductTemplateId = productTemplateId,
                ProductId = 0,
                Combination = [attributeValueId],
                AddQty = 1
            }
        };

        _logger.LogDebug(
            "Fetching combination info: templateId={T} attrValueId={A} color={C}",
            productTemplateId, attributeValueId, colorName);

        var httpResponse = await _httpClient.PostAsJsonAsync(Endpoint, request, cancellationToken);
        httpResponse.EnsureSuccessStatusCode();

        var rpc = await httpResponse.Content
            .ReadFromJsonAsync<JsonRpcResponse<CombinationInfoResult>>(cancellationToken: cancellationToken);

        if (rpc?.Error is not null)
        {
            _logger.LogWarning(
                "JSON-RPC error for {Color}: {Msg}", colorName, rpc.Error.Message);
            return (0, []);
        }

        if (rpc?.Result is null)
        {
            _logger.LogWarning("Empty result for {Color}", colorName);
            return (0, []);
        }

        // Odoo returns product_id=false (deserialized as 0) when the variant
        // is not available (is_combination_possible=false, out of stock, etc.)
        if (rpc.Result.ProductId == 0)
        {
            _logger.LogInformation(
                "Skipping {Color}: variant not available (is_combination_possible={V})",
                colorName, rpc.Result.IsCombinationPossible);
            return (0, []);
        }

        var images = ParseCarouselImages(rpc.Result.Carousel, colorName);
        return (rpc.Result.ProductId, images);
    }

    /// <summary>
    /// Parses the carousel HTML fragment returned by Odoo and extracts
    /// the 1024px (src) and 1920px (data-zoom-image) URLs from the
    /// carousel-item imgs — ignoring the smaller indicator thumbnails.
    /// </summary>
    private List<ProductImage> ParseCarouselImages(string carouselHtml, string colorName)
    {
        if (string.IsNullOrWhiteSpace(carouselHtml)) return [];

        var doc = new HtmlDocument();
        doc.LoadHtml(carouselHtml);

        // Only images inside carousel-item divs (not the indicators)
        var imgNodes = doc.DocumentNode.SelectNodes(
            "//div[contains(@class,'carousel-item')]//img");

        if (imgNodes is null) return [];

        var baseUrl = _options.BaseUrl.TrimEnd('/');
        var safeName = ProductScraperService.SanitizeName(colorName);
        var images = new List<ProductImage>();
        var index = 0;

        foreach (var img in imgNodes)
        {
            // src → image_1024
            var src = img.GetAttributeValue("src", "");
            if (!string.IsNullOrEmpty(src))
            {
                // Strip query string for the filename; keep it for the URL
                var fileNameBase = ExtractFileNameBase(src, safeName, index, "1024");
                images.Add(new ProductImage
                {
                    Url = NormalizeUrl(baseUrl, src),
                    FileName = fileNameBase,
                    ColorName = colorName
                });
            }

            // data-zoom-image → image_1920
            var zoomSrc = img.GetAttributeValue("data-zoom-image", "");
            if (!string.IsNullOrEmpty(zoomSrc))
            {
                var fileNameBase = ExtractFileNameBase(zoomSrc, safeName, index, "1920");
                images.Add(new ProductImage
                {
                    Url = NormalizeUrl(baseUrl, zoomSrc),
                    FileName = fileNameBase,
                    ColorName = colorName
                });
            }

            index++;
        }

        return images;
    }

    private static string NormalizeUrl(string baseUrl, string src)
    {
        // src may be relative (/web/image/...) or already absolute
        if (src.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            return src;
        return baseUrl + src;
    }

    private static string ExtractFileNameBase(string src, string safeName, int index, string size)
    {
        // Try to get the original filename from the URL path segment after the size token
        // Pattern: /web/image/{model}/{id}/image_1024/SomeName.webp?unique=xxx
        var path = src.Contains('?') ? src[..src.IndexOf('?')] : src;
        var lastSegment = path.Split('/').LastOrDefault() ?? "";

        // If it looks like a real filename (has extension), use it
        if (!string.IsNullOrEmpty(lastSegment) && lastSegment.Contains('.'))
        {
            var ext = Path.GetExtension(lastSegment);
            var stem = Path.GetFileNameWithoutExtension(lastSegment);
            return $"{safeName}_{size}_{index}_{stem}{ext}";
        }

        // Fallback: use color + size + index
        return $"{safeName}_{size}_{index}.webp";
    }
}
