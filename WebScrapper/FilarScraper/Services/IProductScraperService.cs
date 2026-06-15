using FilarScraper.Models;

namespace FilarScraper.Services;

public interface IProductScraperService
{
    Task<ScraperResult> ScrapeAsync(string url, CancellationToken cancellationToken = default);
}
