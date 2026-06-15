using FilarScraper.Configuration;
using FilarScraper.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Extensions.Http;

namespace FilarScraper.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFilarScraper(
        this IServiceCollection services,
        Action<ScraperOptions>? configure = null)
    {
        // Options
        var optionsBuilder = services.AddOptions<ScraperOptions>();
        if (configure is not null)
            optionsBuilder.Configure(configure);

        // Retry policy (Polly v7 via Polly.Extensions.Http)
        static IAsyncPolicy<HttpResponseMessage> BuildRetryPolicy(IServiceProvider sp)
        {
            var options = sp.GetRequiredService<IOptions<ScraperOptions>>().Value;
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(
                    options.RetryCount,
                    attempt => TimeSpan.FromSeconds(Math.Pow(options.RetryDelaySeconds, attempt)),
                    onRetry: (outcome, delay, attempt, _) =>
                    {
                        var logger = sp.GetService<ILogger<ProductScraperService>>();
                        logger?.LogWarning(
                            "Retry {Attempt} after {Delay:0.0}s — {Reason}",
                            attempt, delay.TotalSeconds,
                            outcome.Exception?.Message ?? outcome.Result?.StatusCode.ToString());
                    });
        }

        // HttpClient for scraping (follows redirects, browser UA)
        services
            .AddHttpClient<IProductScraperService, ProductScraperService>((sp, client) =>
            {
                var opts = sp.GetRequiredService<IOptions<ScraperOptions>>().Value;
                client.BaseAddress = new Uri(opts.BaseUrl);
                client.Timeout = TimeSpan.FromSeconds(opts.TimeoutSeconds);
                client.DefaultRequestHeaders.UserAgent.ParseAdd(opts.UserAgent);
                client.DefaultRequestHeaders.Accept.ParseAdd("text/html,application/xhtml+xml");
            })
            .AddPolicyHandler((sp, _) => BuildRetryPolicy(sp));

        // HttpClient for combination info API (JSON-RPC)
        services
            .AddHttpClient<ICombinationInfoService, CombinationInfoService>((sp, client) =>
            {
                var opts = sp.GetRequiredService<IOptions<ScraperOptions>>().Value;
                client.BaseAddress = new Uri(opts.BaseUrl);
                client.Timeout = TimeSpan.FromSeconds(opts.TimeoutSeconds);
                client.DefaultRequestHeaders.UserAgent.ParseAdd(opts.UserAgent);
                client.DefaultRequestHeaders.Accept.ParseAdd("application/json");
                client.DefaultRequestHeaders.Referrer = new Uri($"{opts.BaseUrl}/shop");
            })
            .AddPolicyHandler((sp, _) => BuildRetryPolicy(sp));

        // HttpClient for image downloading
        services
            .AddHttpClient<IImageDownloaderService, ImageDownloaderService>((sp, client) =>
            {
                var opts = sp.GetRequiredService<IOptions<ScraperOptions>>().Value;
                client.BaseAddress = new Uri(opts.BaseUrl);
                client.Timeout = TimeSpan.FromSeconds(opts.TimeoutSeconds * 2); // images can be larger
                client.DefaultRequestHeaders.UserAgent.ParseAdd(opts.UserAgent);
            })
            .AddPolicyHandler((sp, _) => BuildRetryPolicy(sp));

        // CSV exporter (no HTTP)
        services.AddTransient<ICsvExporterService, CsvExporterService>();

        return services;
    }
}
