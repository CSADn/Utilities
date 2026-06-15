using FilarScraper.Models;

namespace FilarScraper.Services;

public interface IImageDownloaderService
{
    Task<bool> DownloadImageAsync(
        ProductImage image,
        string outputDirectory,
        CancellationToken cancellationToken = default);

    Task DownloadAllAsync(
        IEnumerable<ProductVariant> variants,
        string outputDirectory,
        int maxParallel,
        CancellationToken cancellationToken = default);
}
