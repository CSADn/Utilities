using FilarScraper.Models;
using Microsoft.Extensions.Logging;

namespace FilarScraper.Services;

public class ImageDownloaderService : IImageDownloaderService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ImageDownloaderService> _logger;

    // Track downloaded URLs across the session to avoid duplicates
    private readonly HashSet<string> _downloadedUrls = [];
    private readonly SemaphoreSlim _lock = new(1, 1);

    public ImageDownloaderService(HttpClient httpClient, ILogger<ImageDownloaderService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<bool> DownloadImageAsync(
        ProductImage image,
        string outputDirectory,
        CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            if (_downloadedUrls.Contains(image.Url))
            {
                _logger.LogDebug("Duplicate skipped: {Url}", image.Url);
                return false;
            }
            _downloadedUrls.Add(image.Url);
        }
        finally
        {
            _lock.Release();
        }

        var colorFolder = Path.Combine(
            outputDirectory,
            ProductScraperService.SanitizeName(image.ColorName));

        Directory.CreateDirectory(colorFolder);

        var localPath = Path.Combine(colorFolder, image.FileName);

        if (File.Exists(localPath))
        {
            _logger.LogDebug("File already exists, skipping: {Path}", localPath);
            image.Downloaded = false;
            image.LocalPath = localPath;
            return false;
        }

        try
        {
            _logger.LogInformation("Downloading: {Url}", image.Url);
            var bytes = await _httpClient.GetByteArrayAsync(image.Url, cancellationToken);
            await File.WriteAllBytesAsync(localPath, bytes, cancellationToken);
            image.Downloaded = true;
            image.LocalPath = localPath;
            _logger.LogInformation("Saved: {Path}", localPath);
            return true;
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Image not found (404): {Url}", image.Url);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to download: {Url}", image.Url);
            return false;
        }
    }

    public async Task DownloadAllAsync(
        IEnumerable<ProductVariant> variants,
        string outputDirectory,
        int maxParallel,
        CancellationToken cancellationToken = default)
    {
        var allImages = variants
            .SelectMany(v => v.Images)
            .ToList();

        _logger.LogInformation("Starting parallel download of {Count} images (max {Max} concurrent)",
            allImages.Count, maxParallel);

        var semaphore = new SemaphoreSlim(maxParallel, maxParallel);

        var tasks = allImages.Select(async image =>
        {
            await semaphore.WaitAsync(cancellationToken);
            try
            {
                return await DownloadImageAsync(image, outputDirectory, cancellationToken);
            }
            finally
            {
                semaphore.Release();
            }
        });

        await Task.WhenAll(tasks);
    }
}
