using System.Text;
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

    // Separate client for public IP discovery (no default headers)
    private static readonly HttpClient _ipClient = new()
    {
        Timeout = TimeSpan.FromSeconds(10)
    };

    private string? _cachedPublicIp;
    private DateTime _publicIpExpiry = DateTime.MinValue;

    private const string UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/125.0.0.0 Safari/537.36";

    public CdnTokenService(ILogger<CdnTokenService> logger)
    {
        _logger = logger;
    }

    public async Task<string> GetPublicIpAsync()
    {
        if (_cachedPublicIp != null && DateTime.UtcNow < _publicIpExpiry)
            return _cachedPublicIp;

        try
        {
            var ip = await _ipClient.GetStringAsync("https://api.ipify.org");
            _cachedPublicIp = ip.Trim();
            _publicIpExpiry = DateTime.UtcNow.AddMinutes(5);
            return _cachedPublicIp;
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Failed to discover public IP");
            return "unknown";
        }
    }

    /// Base64url-decode the payload of a JWT and return the JSON string.
    private static string DecodeJwtPayload(string jwt)
    {
        try
        {
            var parts = jwt.Split('.');
            if (parts.Length < 2) return "";
            var payload = parts[1];
            var padded = (payload.Length % 4) switch
            {
                2 => payload + "==",
                3 => payload + "=",
                _ => payload
            };
            var bytes = Convert.FromBase64String(padded);
            return Encoding.UTF8.GetString(bytes);
        }
        catch { return ""; }
    }

    /// Extract a specific claim from a JWT payload JSON.
    private static string? GetJwtClaim(string jwt, string claimName)
    {
        var json = DecodeJwtPayload(jwt);
        if (string.IsNullOrEmpty(json)) return null;
        using var doc = JsonDocument.Parse(json);
        if (doc.RootElement.TryGetProperty(claimName, out var prop))
            return prop.GetString();
        return null;
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
        // Log server's public IP for sip mismatch diagnosis
        var publicIp = await GetPublicIpAsync();
        _logger.LogInformation("RequestCdnTokenAsync: Server public IP: {PublicIp}", publicIp);

        // URLs with embedded tok_ JWT in the path don't need a CDN token
        if (mpdUrl.Contains("/tok_"))
        {
            _logger.LogWarning("RequestCdnTokenAsync called with embedded token URL, this should not happen");
            return "";
        }

        var encodedUrl = Uri.EscapeDataString(mpdUrl);
        var tokenUrl = $"https://cdn-token.app.flow.com.ar/cdntoken/v2/generator?path={encodedUrl}";

        using var request = new HttpRequestMessage(HttpMethod.Get, tokenUrl);
        request.Headers.TryAddWithoutValidation("Authorization", $"Bearer {bearerToken}");

        // Use channel-specific headers if provided, otherwise fall back to defaults
        if (headers != null && headers.TryGetValue("User-Agent", out var ua) && !string.IsNullOrWhiteSpace(ua))
            request.Headers.TryAddWithoutValidation("User-Agent", ua);
        else
            request.Headers.TryAddWithoutValidation("User-Agent", UserAgent);

        if (headers != null && headers.TryGetValue("Origin", out var origin) && !string.IsNullOrWhiteSpace(origin))
            request.Headers.TryAddWithoutValidation("Origin", origin);
        else
            request.Headers.TryAddWithoutValidation("Origin", "https://portal.app.flow.com.py");

        if (headers != null && headers.TryGetValue("Referer", out var referer) && !string.IsNullOrWhiteSpace(referer))
            request.Headers.TryAddWithoutValidation("Referer", referer);
        else
            request.Headers.TryAddWithoutValidation("Referer", "https://portal.app.flow.com.py");

        // Forward any remaining channel-specific headers (e.g. custom ones)
        if (headers != null)
        {
            foreach (var (key, value) in headers)
            {
                if (!string.IsNullOrWhiteSpace(key) && !string.IsNullOrWhiteSpace(value) &&
                    !string.Equals(key, "Content-Type", StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(key, "Origin", StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(key, "Referer", StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(key, "User-Agent", StringComparison.OrdinalIgnoreCase))
                {
                    request.Headers.TryAddWithoutValidation(key, value);
                }
            }
        }

        _logger.LogInformation("RequestCdnTokenAsync: Requesting CDN token for {MpdUrl}", mpdUrl);
        _logger.LogInformation("RequestCdnTokenAsync: Bearer token: {BearerToken}", bearerToken);
        _logger.LogInformation("RequestCdnTokenAsync: Token URL: {TokenUrl}", tokenUrl);

        var response = await _client.SendAsync(request);

        try
        {
            response.EnsureSuccessStatusCode();
        }
        catch (Exception)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Failed to fetch CDN token. Status: {StatusCode}, Response: {ResponseContent}", response.StatusCode, responseContent);
            throw;
        }

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var cdnToken = doc.RootElement.GetProperty("token").GetString()
            ?? throw new InvalidOperationException("CDN token response missing 'token' field");

        // Decode JWT sip and log for comparison with server's public IP
        var tokenSip = GetJwtClaim(cdnToken, "sip");
        _logger.LogInformation("RequestCdnTokenAsync: CDN token sip: {Sip}, Server public IP: {PublicIp}",
            tokenSip ?? "(not found)", publicIp);

        return cdnToken;
    }
}
