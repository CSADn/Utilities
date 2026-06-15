namespace FilarScraper.Configuration;

public class ScraperOptions
{
    public string BaseUrl { get; set; } = "https://filar.com.ar";
    public string OutputDirectory { get; set; } = "output";
    public int MaxParallelDownloads { get; set; } = 4;
    public int RetryCount { get; set; } = 3;
    public int RetryDelaySeconds { get; set; } = 2;
    public int TimeoutSeconds { get; set; } = 30;
    public string UserAgent { get; set; } =
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 Chrome/120.0.0.0 Safari/537.36";
}
