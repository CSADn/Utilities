using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using NLog;

namespace DashMPDPlayer.Services;

public class TokenService
{
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    private readonly HttpClient _httpClient;
    private string? _bearerToken;
    private DateTime? _bearerTokenExpiry;

    public TokenService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public void InvalidateToken()
    {
        _bearerToken = null;
        _bearerTokenExpiry = null;
    }

    public async Task<string?> GetBearerTokenAsync()
    {
        if (_bearerToken != null && _bearerTokenExpiry.HasValue && DateTime.UtcNow < _bearerTokenExpiry.Value)
            return _bearerToken;

        try
        {
            _logger.Info("Obteniendo bearer token de piratacodigo.json...");
            var response = await _httpClient.GetAsync("https://app.femon.net/pirata/piratacodigo.json");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("bearerToken", out var tokenProp))
                _bearerToken = tokenProp.GetString();
            else if (doc.RootElement.TryGetProperty("token", out var tokenProp2))
                _bearerToken = tokenProp2.GetString();
            else if (doc.RootElement.TryGetProperty("access_token", out var accessProp))
                _bearerToken = accessProp.GetString();

            if (_bearerToken != null)
            {
                if (_bearerToken.StartsWith("Bearer "))
                    _bearerToken = _bearerToken["Bearer ".Length..];
                _bearerTokenExpiry = DateTime.UtcNow.AddHours(1);
                _logger.Info("Bearer token obtenido exitosamente");
            }
            else
            {
                _logger.Warn("No se encontró token en la respuesta de piratacodigo.json");
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error obteniendo bearer token");
        }

        return _bearerToken;
    }

    public async Task<string> RequestCdnTokenAsync(string mpdUrl, string? bearerToken, Dictionary<string, string> headers)
    {
        var tokenUrl = $"https://cdn-token.app.flow.com.ar/cdntoken/v2/generator?path={Uri.EscapeDataString(mpdUrl)}";
        _logger.Info("Solicitando token CDN para: {MpdUrl}", mpdUrl);

        var tokenRequest = new HttpRequestMessage(HttpMethod.Get, tokenUrl);
        if (bearerToken != null)
            tokenRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
        AddHeaders(tokenRequest, headers);

        var tokenResponse = await _httpClient.SendAsync(tokenRequest);
        tokenResponse.EnsureSuccessStatusCode();
        var tokenJson = await tokenResponse.Content.ReadAsStringAsync();

        var tokenDoc = JsonDocument.Parse(tokenJson);
        var cdnToken = tokenDoc.RootElement.GetProperty("token").GetString()
            ?? throw new InvalidOperationException("No se encontró 'token' en la respuesta del CDN");

        _logger.Debug("Token CDN obtenido: {Length} chars", cdnToken.Length);
        return cdnToken;
    }

    private static void AddHeaders(HttpRequestMessage request, Dictionary<string, string> headers)
    {
        foreach (var h in headers)
        {
            if (string.IsNullOrEmpty(h.Key) || string.IsNullOrEmpty(h.Value)) continue;
            if (h.Key.Equals("Content-Type", StringComparison.OrdinalIgnoreCase)) continue;
            try
            {
                request.Headers.TryAddWithoutValidation(h.Key, h.Value);
            }
            catch (Exception ex)
            {
                _logger.Warn(ex, "Error agregando header {Header}", h.Key);
            }
        }
    }
}
