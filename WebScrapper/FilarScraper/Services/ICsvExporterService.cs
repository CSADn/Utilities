using FilarScraper.Models;

namespace FilarScraper.Services;

public interface ICsvExporterService
{
    Task ExportAsync(ScraperResult result, string filePath, CancellationToken cancellationToken = default);
}
