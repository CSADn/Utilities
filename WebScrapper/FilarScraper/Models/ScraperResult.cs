namespace FilarScraper.Models;

public record ScraperResult
{
    public string ProductName { get; init; } = string.Empty;
    public string SourceUrl { get; init; } = string.Empty;
    public List<ProductVariant> Variants { get; init; } = [];
    public int TotalImagesDownloaded { get; set; }
    public int TotalImagesDuplicated { get; set; }
    public string OutputDirectory { get; init; } = string.Empty;
    public string CsvPath { get; init; } = string.Empty;
}
