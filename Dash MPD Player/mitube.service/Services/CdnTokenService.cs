using System.Text.Json;

namespace mitube.service.Services;

public class CdnTokenService
{
    private readonly ILogger<CdnTokenService> _logger;

    private string? _cachedBearerToken;
    private DateTime _bearerTokenExpiry = DateTime.MinValue;
    private static readonly TimeSpan BearerCacheDuration = TimeSpan.FromHours(1);

    private static readonly HttpClient _client = new()
    {
        Timeout = TimeSpan.FromSeconds(15)
    };

    private const string UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/125.0.0.0 Safari/537.36";

    public CdnTokenService(ILogger<CdnTokenService> logger)
    {
        _logger = logger;
    }

    public async Task<string> GetBearerTokenAsync()
    {
        if (_cachedBearerToken != null && DateTime.UtcNow < _bearerTokenExpiry)
            return _cachedBearerToken;

        try
        {
            var response = await _client.GetStringAsync("https://app.femon.net/pirata/piratacodigo.json");
            using var doc = JsonDocument.Parse(response);
            var root = doc.RootElement;

            string? token = null;
            if (root.TryGetProperty("bearerToken", out var bt)) token = bt.GetString();
            else if (root.TryGetProperty("token", out var t)) token = t.GetString();
            else if (root.TryGetProperty("access_token", out var at)) token = at.GetString();

            if (token != null)
            {
                if (token.StartsWith("Bearer ")) token = token["Bearer ".Length..];
                _cachedBearerToken = token;
                _bearerTokenExpiry = DateTime.UtcNow.Add(BearerCacheDuration);
                _logger.LogDebug("Bearer token refreshed");
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to fetch bearer token");
            if (_cachedBearerToken != null)
                return _cachedBearerToken;
            throw;
        }

        return _cachedBearerToken ?? throw new InvalidOperationException("Could not obtain bearer token");
    }

    public async Task<string> RequestCdnTokenAsync(string mpdUrl, string bearerToken, Dictionary<string, string>? headers)
    {
        var encodedUrl = Uri.EscapeDataString(mpdUrl);
        var tokenUrl = $"https://cdn-token.app.flow.com.ar/cdntoken/v2/generator?path={encodedUrl}";

        using var request = new HttpRequestMessage(HttpMethod.Get, tokenUrl);
        request.Headers.TryAddWithoutValidation("Authorization", $"Bearer {bearerToken}");
        request.Headers.TryAddWithoutValidation("User-Agent", UserAgent);

        if (headers != null)
        {
            foreach (var (key, value) in headers)
            {
                if (!string.IsNullOrWhiteSpace(key) && !string.IsNullOrWhiteSpace(value) &&
                    !string.Equals(key, "Content-Type", StringComparison.OrdinalIgnoreCase))
                {
                    request.Headers.TryAddWithoutValidation(key, value);
                }
            }
        }

        _logger.LogInformation("RequestCdnTokenAsync: Requesting CDN token for {MpdUrl}", mpdUrl);
        _logger.LogInformation("RequestCdnTokenAsync: Bearer token: {BearerToken}", bearerToken);
        _logger.LogInformation("RequestCdnTokenAsync: Token URL: {TokenUrl}", tokenUrl);
        _logger.LogInformation("RequestCdnTokenAsync: User-Agent: {UserAgent}", UserAgent);
        _logger.LogInformation("RequestCdnTokenAsync: Header Authorization: {Authorization}", request.Headers.Authorization);
        _logger.LogInformation("RequestCdnTokenAsync: Header User-Agent: {User-Agent}", request.Headers.UserAgent);

        var response = await _client.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.GetProperty("token").GetString()
            ?? throw new InvalidOperationException("CDN token response missing 'token' field");
    }
}
