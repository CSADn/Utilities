using FilarScraper.Configuration;
using FilarScraper.Infrastructure;
using FilarScraper.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// ─── Configuration ────────────────────────────────────────────────────────────
const string TargetUrl = "https://filar.com.ar/shop/filamento-pla-x-1kg-14";
const string OutputBase = "output";

// ─── DI Setup ─────────────────────────────────────────────────────────────────
var services = new ServiceCollection();

services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.SetMinimumLevel(LogLevel.Information);
});

services.AddFilarScraper(opts =>
{
    opts.BaseUrl = "https://filar.com.ar";
    opts.OutputDirectory = OutputBase;
    opts.MaxParallelDownloads = 5;
    opts.RetryCount = 3;
    opts.RetryDelaySeconds = 2;
    opts.TimeoutSeconds = 30;
});

var provider = services.BuildServiceProvider();
var logger = provider.GetRequiredService<ILogger<Program>>();

// ─── Run ───────────────────────────────────────────────────────────────────────
logger.LogInformation("=== FilAr Scraper — .NET 10 ===");
logger.LogInformation("Target: {Url}", TargetUrl);

using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    logger.LogWarning("Cancellation requested…");
    cts.Cancel();
};

try
{
    // 1. Scrape product page
    var scraper = provider.GetRequiredService<IProductScraperService>();
    var result = await scraper.ScrapeAsync(TargetUrl, cts.Token);

    logger.LogInformation("Scraped {Count} color variants for \"{Product}\"",
        result.Variants.Count, result.ProductName);

    // 2. Print variant summary
    foreach (var v in result.Variants)
        logger.LogInformation("  [{Hex}] {Color} (attrValueId={Id})",
            v.HexColor, v.ColorName, v.AttributeValueId);

    // 3. Create output directory
    Directory.CreateDirectory(OutputBase);

    // 4. Download images in parallel
    var downloader = provider.GetRequiredService<IImageDownloaderService>();
    await downloader.DownloadAllAsync(
        result.Variants,
        OutputBase,
        maxParallel: 5,
        cts.Token);

    // Tally results
    result.TotalImagesDownloaded = result.Variants
        .SelectMany(v => v.Images)
        .Count(i => i.Downloaded);

    result.TotalImagesDuplicated = result.Variants
        .SelectMany(v => v.Images)
        .Count(i => !i.Downloaded && !string.IsNullOrEmpty(i.LocalPath));

    // 5. Export CSV
    var csvPath = Path.Combine(OutputBase, "products.csv");
    var exporter = provider.GetRequiredService<ICsvExporterService>();
    await exporter.ExportAsync(result with { CsvPath = csvPath }, csvPath, cts.Token);

    // ─── Summary ────────────────────────────────────────────────────────────
    logger.LogInformation("=== DONE ===");
    logger.LogInformation("  Variants  : {V}", result.Variants.Count);
    logger.LogInformation("  Downloaded: {D}", result.TotalImagesDownloaded);
    logger.LogInformation("  Skipped   : {S}", result.TotalImagesDuplicated);
    logger.LogInformation("  CSV       : {P}", Path.GetFullPath(csvPath));
    logger.LogInformation("  Output dir: {P}", Path.GetFullPath(OutputBase));
}
catch (OperationCanceledException)
{
    logger.LogWarning("Operation was cancelled.");
}
catch (Exception ex)
{
    logger.LogError(ex, "Fatal error during scraping.");
    return 1;
}

return 0;
