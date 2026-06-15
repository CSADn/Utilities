using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using NLog;

using DashMPDPlayer.Interfaces;
using DashMPDPlayer.Models;

namespace DashMPDPlayer.Services;

public class ProxyService : IProxyService
{
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    private static readonly TimeSpan SessionTtl = TimeSpan.FromMinutes(5);
    private const int ManifestTimeoutSec = 30;
    private const int SegmentTimeoutSec = 120;

    private readonly HttpListener _listener;
    private readonly HttpClient _httpClient;
    private readonly HttpClient _httpClientSegments;
    private readonly TokenService _tokenService;
    private readonly MpdRewriter _mpdRewriter;
    private readonly int _port;
    private readonly object _configLock = new();
    private Dictionary<string, string>? _currentHeaders;
    private string? _currentMpdUrl;
    private string _playerHtml = "";
    private readonly ConcurrentDictionary<string, SessionEntry> _sessionEntries = new(StringComparer.Ordinal);
    private int _sessionCounter;

    private record SessionEntry(string BaseUrl, DateTime Created);

    public int Port => _port;
    public string ProxyBaseUrl => $"http://localhost:{_port}";

    public event Action<string>? OnLog;

    public ProxyService()
    {
        _port = GetAvailablePort();
        _listener = new HttpListener();
        _listener.Prefixes.Add($"http://localhost:{_port}/");
        _listener.Start();

        var handler = new SocketsHttpHandler
        {
            PooledConnectionLifetime = TimeSpan.FromSeconds(30),
            AllowAutoRedirect = true,
            UseCookies = true,
            CookieContainer = new CookieContainer(),
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
        };
        _httpClient = new HttpClient(handler);
        _httpClient.Timeout = TimeSpan.FromSeconds(ManifestTimeoutSec);

        var segmentHandler = new SocketsHttpHandler
        {
            PooledConnectionLifetime = TimeSpan.FromSeconds(60),
            AllowAutoRedirect = false,
            UseCookies = true,
            CookieContainer = new CookieContainer(),
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
        };
        _httpClientSegments = new HttpClient(segmentHandler);
        _httpClientSegments.Timeout = TimeSpan.FromSeconds(SegmentTimeoutSec);

        _tokenService = new TokenService(_httpClient);
        _mpdRewriter = new MpdRewriter(ProxyBaseUrl);

        _logger.Info("ProxyService inicializado, puerto asignado: {Port}", _port);
    }

    private void CleanupStaleSessions()
    {
        if (_sessionEntries.Count < 50) return;
        var cutoff = DateTime.UtcNow - SessionTtl;
        var stale = _sessionEntries.Where(kvp => kvp.Value.Created < cutoff).Select(kvp => kvp.Key).ToList();
        foreach (var key in stale)
            _sessionEntries.TryRemove(key, out _);
    }

    private (string? mpdUrl, Dictionary<string, string>? headers) GetConfigSnapshot()
    {
        lock (_configLock)
        {
            return (_currentMpdUrl, _currentHeaders != null ? new Dictionary<string, string>(_currentHeaders) : null);
        }
    }

    public void SetPlayerHtml(string html)
    {
        _playerHtml = html;
        _logger.Debug("Player HTML configurado ({Length} chars)", html.Length);
    }

    public void SetChannelConfig(string mpdUrl, Dictionary<string, string>? headers)
    {
        lock (_configLock)
        {
            _currentMpdUrl = mpdUrl;
            _currentHeaders = headers;
        }
        _logger.Info("Config de canal actualizada: {Url}, headers: {HasHeaders}", mpdUrl, headers != null);
    }

    public void Start()
    {
        _logger.Info("Proxy HTTP listo en puerto {Port}", _port);
        Log($"Proxy iniciado en puerto {_port}");
        _ = Task.Run(async () =>
        {
            try
            {
                await HandleRequestsAsync();
            }
            catch (Exception ex)
            {
                _logger.Fatal(ex, "Error fatal en el loop de escucha HTTP");
            }
        });
    }

    private void Log(string msg)
    {
        _logger.Info(msg);
        OnLog?.Invoke(msg);
    }

    public void Stop()
    {
        try
        {
            _listener.Stop();
            _logger.Info("Proxy HTTP detenido");
        }
        catch (Exception ex)
        {
            _logger.Warn(ex, "Error al detener proxy");
        }
    }

    private async Task HandleRequestsAsync()
    {
        _logger.Debug("Iniciando loop de escucha HTTP");
        while (_listener.IsListening)
        {
            try
            {
                var ctx = await _listener.GetContextAsync();
                _ = Task.Run(() => ProcessRequestAsync(ctx));
            }
            catch (HttpListenerException ex)
            {
                _logger.Debug(ex, "Listener detenido (HttpListenerException)");
                break;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error en listener HTTP");
            }
        }
        _logger.Debug("Loop de escucha HTTP terminado");
    }

    private async Task ProcessRequestAsync(HttpListenerContext ctx)
    {
        try
        {
            var path = ctx.Request.Url!.AbsolutePath;
            var query = ctx.Request.Url.Query;
            var isHead = string.Equals(ctx.Request.HttpMethod, "HEAD", StringComparison.OrdinalIgnoreCase);

            if (string.Equals(ctx.Request.HttpMethod, "OPTIONS", StringComparison.OrdinalIgnoreCase))
            {
                ctx.Response.StatusCode = 204;
                ctx.Response.Headers.Add("Access-Control-Allow-Origin", "*");
                ctx.Response.Headers.Add("Access-Control-Allow-Methods", "GET, HEAD, POST, OPTIONS");
                ctx.Response.Headers.Add("Access-Control-Allow-Headers", "*");
                ctx.Response.Headers.Add("Access-Control-Max-Age", "86400");
                ctx.Response.Close();
                return;
            }

            _logger.Debug("=> {Method} {Path}{Query}", ctx.Request.HttpMethod, path, query);

            if (path == "/player" || path == "/player.html")
                await ServePlayerAsync(ctx);
            else if (path == "/manifest")
                await HandleManifestAsync(ctx, query, isHead);
            else if (path.StartsWith("/segment/"))
                await HandleSegmentAsync(ctx, query);
            else if (path == "/license")
                await HandleLicenseAsync(ctx, query);
            else if (path == "/fetch")
                await HandleFetchAsync(ctx, query, isHead);
            else
            {
                ctx.Response.StatusCode = 404;
                await WriteTextAsync(ctx.Response, "Not found");
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error procesando request {Path}", ctx.Request.Url?.AbsolutePath);
            try
            {
                ctx.Response.StatusCode = 500;
                ctx.Response.StatusDescription = "Internal Server Error";
                var errBytes = Encoding.UTF8.GetBytes($"Error: {ex.Message}");
                await ctx.Response.OutputStream.WriteAsync(errBytes);
                ctx.Response.OutputStream.Close();
            }
            catch (Exception ex2)
            {
                _logger.Warn(ex2, "Error al enviar respuesta de error");
            }
        }
    }

    private async Task ServePlayerAsync(HttpListenerContext ctx)
    {
        ctx.Response.ContentType = "text/html; charset=utf-8";
        ctx.Response.Headers.Add("Access-Control-Allow-Origin", "*");
        var bytes = Encoding.UTF8.GetBytes(_playerHtml);
        ctx.Response.ContentLength64 = bytes.Length;
        await ctx.Response.OutputStream.WriteAsync(bytes);
        ctx.Response.OutputStream.Close();
        _logger.Debug("Player.html servido ({Length} bytes)", bytes.Length);
    }

    private async Task HandleManifestAsync(HttpListenerContext ctx, string query, bool isHead = false)
    {
        var parsed = ParseQueryString(query);
        var mpdUrl = parsed.GetValueOrDefault("url");
        if (string.IsNullOrEmpty(mpdUrl))
        {
            ctx.Response.StatusCode = 400;
            await WriteTextAsync(ctx.Response, "Missing url parameter");
            _logger.Warn("HandleManifest: url parameter missing");
            return;
        }

        var (_, currentHeaders) = GetConfigSnapshot();

        var (mpdContent, baseUrl) = currentHeaders != null
            ? await FetchSignedManifestAsync(mpdUrl, currentHeaders)
            : await FetchDirectManifestAsync(mpdUrl);

        CleanupStaleSessions();
        var token = Interlocked.Increment(ref _sessionCounter).ToString("x");
        _sessionEntries[token] = new SessionEntry(baseUrl, DateTime.UtcNow);
        _logger.Debug("Session token: {Token} -> {BaseUrl}", token, baseUrl);

        mpdContent = _mpdRewriter.Rewrite(mpdContent, token);

        if (mpdContent.TrimStart().StartsWith("#EXTM3U"))
        {
            mpdContent = RewriteHlsPlaylist(mpdContent, mpdUrl);
        }

        if (mpdContent.Length > 0)
        {
            var preview = mpdContent.Length > 500 ? mpdContent[..500] + "..." : mpdContent;
            _logger.Debug("MPD reescrito (inicio): {Preview}", preview.Replace("\r\n", " ").Replace("\n", " "));
        }

        ctx.Response.ContentType = mpdContent.TrimStart().StartsWith("#EXTM3U")
            ? "application/vnd.apple.mpegURL; charset=utf-8"
            : "application/dash+xml; charset=utf-8";
        ctx.Response.Headers.Add("Access-Control-Allow-Origin", "*");
        ctx.Response.Headers.Add("Access-Control-Allow-Headers", "*");
        ctx.Response.ContentLength64 = Encoding.UTF8.GetByteCount(mpdContent);
        if (!isHead)
        {
            await ctx.Response.OutputStream.WriteAsync(Encoding.UTF8.GetBytes(mpdContent));
            ctx.Response.OutputStream.Close();
            _logger.Info("MPD devuelto ({Length} chars)", mpdContent.Length);
            Log($"MPD devuelto ({mpdContent.Length} chars)");
        }
        else
        {
            ctx.Response.Close();
            _logger.Info("HEAD MPD verificado ({Length} chars)", mpdContent.Length);
        }
    }

    private async Task HandleFetchAsync(HttpListenerContext ctx, string query, bool isHead)
    {
        var parsed = ParseQueryString(query);
        var url = parsed.GetValueOrDefault("url");
        if (string.IsNullOrEmpty(url))
        {
            ctx.Response.StatusCode = 400;
            await WriteTextAsync(ctx.Response, "Missing url parameter");
            _logger.Warn("HandleFetch: url parameter missing");
            return;
        }

        _logger.Debug("Fetch solicitado: {Url}", url);

        var request = new HttpRequestMessage(HttpMethod.Get, url);
        var (_, currentHeaders) = GetConfigSnapshot();
        AddHeadersToRequest(request, currentHeaders);

        HttpResponseMessage response;
        try
        {
            response = await _httpClientSegments.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();
        }
        catch (HttpRequestException ex) when (ex.StatusCode.HasValue)
        {
            ctx.Response.StatusCode = (int)ex.StatusCode.Value;
            ctx.Response.StatusDescription = ex.StatusCode.Value.ToString();
            await WriteTextAsync(ctx.Response, $"Upstream error: {(int)ex.StatusCode.Value}");
            _logger.Warn("Fetch {Url} retornó {StatusCode}", url, (int)ex.StatusCode.Value);
            return;
        }

        var contentBytes = await response.Content.ReadAsByteArrayAsync();
        var upstreamCt = response.Content.Headers.ContentType?.MediaType ?? "(none)";
        var contentType = DetectFetchContentType(url, contentBytes);
        _logger.Debug("Fetch upstream Content-Type: {UpstreamCt}, local: {LocalCt}, size: {Size} for {Url}",
            upstreamCt, contentType, contentBytes.Length, url);

        ctx.Response.ContentType = contentType;
        ctx.Response.Headers.Add("Access-Control-Allow-Origin", "*");
        ctx.Response.Headers.Add("Access-Control-Allow-Headers", "*");

        if (contentType.StartsWith("application/vnd.apple.mpegURL", StringComparison.OrdinalIgnoreCase) ||
            contentType.StartsWith("application/x-mpegURL", StringComparison.OrdinalIgnoreCase))
        {
            var text = Encoding.UTF8.GetString(contentBytes);
            var preview = text.Length > 500 ? text[..500] + "..." : text;
            _logger.Debug("HLS playlist ({Len} chars): {Preview}", text.Length,
                preview.Replace("\r\n", " ").Replace("\n", " "));
            text = RewriteHlsPlaylist(text, url, false);
            contentBytes = Encoding.UTF8.GetBytes(text);
        }

        ctx.Response.ContentLength64 = contentBytes.Length;
        if (!isHead)
        {
            await ctx.Response.OutputStream.WriteAsync(contentBytes);
            ctx.Response.OutputStream.Close();
            _logger.Debug("Fetch servido ({Length} bytes): {Url}", contentBytes.Length, url);
        }
        else
        {
            ctx.Response.Close();
            _logger.Debug("Fetch HEAD verificado: {Url}", url);
        }
    }

    private static string DetectFetchContentType(string url, byte[] content)
    {
        var path = url.Contains('?') ? url[..url.IndexOf('?')] : url;
        var ext = System.IO.Path.GetExtension(path)?.ToLowerInvariant();

        return ext switch
        {
            ".m3u8" => "application/vnd.apple.mpegURL; charset=utf-8",
            ".ts" => "video/mp2t",
            ".mp4" => "video/mp4",
            ".m4s" => "application/octet-stream",
            ".aac" => "audio/aac",
            ".m4a" => "audio/mp4",
            ".vtt" => "text/vtt; charset=utf-8",
            ".key" => "application/octet-stream",
            _ => "application/octet-stream"
        };
    }

    private string RewriteHlsPlaylist(string content, string playlistUrl, bool rewriteMediaUrls = true)
    {
        var baseDir = playlistUrl[..(playlistUrl.LastIndexOf('/') + 1)];
        var lines = content.Replace("\r\n", "\n").Split('\n');
        var result = new StringBuilder();

        foreach (var line in lines)
        {
            var rewritten = RewriteHlsLine(line.TrimEnd('\r'), baseDir, rewriteMediaUrls);
            result.AppendLine(rewritten);
        }

        return result.ToString();
    }

    private string RewriteHlsLine(string line, string baseDir, bool rewriteMediaUrls)
    {
        var trimmed = line.Trim();

        if (string.IsNullOrEmpty(trimmed))
            return line;

        if (rewriteMediaUrls && (trimmed.StartsWith("http://") || trimmed.StartsWith("https://")))
        {
            return $"{ProxyBaseUrl}/fetch?url={Uri.EscapeDataString(trimmed)}";
        }

        if (trimmed.Contains("URI=\""))
        {
            return RewriteUriAttribute(line, '"', baseDir);
        }

        if (trimmed.Contains("URI='"))
        {
            return RewriteUriAttribute(line, '\'', baseDir);
        }

        if (rewriteMediaUrls && !trimmed.StartsWith("#"))
        {
            try
            {
                var absolute = new Uri(new Uri(baseDir), trimmed);
                return $"{ProxyBaseUrl}/fetch?url={Uri.EscapeDataString(absolute.ToString())}";
            }
            catch
            {
                return line;
            }
        }

        return line;
    }

    private string RewriteUriAttribute(string line, char quote, string baseDir)
    {
        var attrPattern = $"URI={quote}";
        var startIdx = line.IndexOf(attrPattern, StringComparison.Ordinal);
        if (startIdx < 0) return line;

        startIdx += attrPattern.Length;
        var endIdx = line.IndexOf(quote, startIdx);
        if (endIdx < 0) return line;

        var originalUri = line[startIdx..endIdx];

        if (originalUri.Contains("/fetch?url="))
            return line;

        string absoluteUri;
        if (originalUri.StartsWith("http://") || originalUri.StartsWith("https://"))
        {
            absoluteUri = originalUri;
        }
        else
        {
            try
            {
                absoluteUri = new Uri(new Uri(baseDir), originalUri).ToString();
            }
            catch
            {
                return line;
            }
        }

        var proxied = $"{ProxyBaseUrl}/fetch?url={Uri.EscapeDataString(absoluteUri)}";
        return line[..startIdx] + proxied + line[endIdx..];
    }

    private async Task<(string mpdContent, string baseUrl)> FetchSignedManifestAsync(string mpdUrl, Dictionary<string, string> headers)
    {
        var bearerToken = await _tokenService.GetBearerTokenAsync();
        var cdnToken = await _tokenService.RequestCdnTokenAsync(mpdUrl, bearerToken, headers);

        var separator = mpdUrl.Contains('?') ? "&" : "?";
        var signedMpdUrl = mpdUrl + separator + "cdntoken=" + cdnToken;
        _logger.Info("URL firmada construida");

        var mpdResponse = await SendManifestRequestAsync(signedMpdUrl, headers);
        var mpdContent = await mpdResponse.Content.ReadAsStringAsync();

        var baseUrl = mpdResponse.RequestMessage?.RequestUri?.ToString() ?? mpdUrl;
        var slashIdx = baseUrl.LastIndexOf('/');
        baseUrl = slashIdx >= 0 ? baseUrl[..(slashIdx + 1)] : baseUrl;
        _logger.Info("Base URL para segmentos (resuelta): {BaseUrl}", baseUrl);

        return (mpdContent, baseUrl);
    }

    private async Task<(string mpdContent, string baseUrl)> FetchDirectManifestAsync(string mpdUrl)
    {
        _logger.Info("Obteniendo MPD directamente (sin firma): {MpdUrl}", mpdUrl);
        var response = await _httpClient.GetAsync(mpdUrl);
        response.EnsureSuccessStatusCode();
        var mpdContent = await response.Content.ReadAsStringAsync();

        var slashIdx = mpdUrl.LastIndexOf('/');
        var baseUrl = slashIdx >= 0 ? mpdUrl[..(slashIdx + 1)] : mpdUrl;
        return (mpdContent, baseUrl);
    }

    private async Task<HttpResponseMessage> SendManifestRequestAsync(string signedMpdUrl, Dictionary<string, string> headers)
    {
        var mpdRequest = new HttpRequestMessage(HttpMethod.Get, signedMpdUrl);
        AddHeadersToRequest(mpdRequest, headers);

        var mpdResponse = await _httpClient.SendAsync(mpdRequest);

        if ((int)mpdResponse.StatusCode == 401)
        {
            _logger.Warn("MPD request obtuvo 401, renovando bearer token y reintentando...");
            _tokenService.InvalidateToken();
            var newToken = await _tokenService.GetBearerTokenAsync();

            if (newToken != null)
            {
                var retryRequest = new HttpRequestMessage(HttpMethod.Get, signedMpdUrl);
                AddHeadersToRequest(retryRequest, headers);
                retryRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", newToken);
                mpdResponse = await _httpClient.SendAsync(retryRequest);
            }
        }

        mpdResponse.EnsureSuccessStatusCode();
        return mpdResponse;
    }

    private async Task HandleSegmentAsync(HttpListenerContext ctx, string query)
    {
        var path = ctx.Request.Url!.AbsolutePath;

        var prefix = "/segment/";
        if (!path.StartsWith(prefix))
        {
            ctx.Response.StatusCode = 400;
            await WriteTextAsync(ctx.Response, "Invalid segment path");
            _logger.Warn("HandleSegment: invalid path {Path}", path);
            return;
        }

        var rest = path.AsSpan(prefix.Length);
        var slashIdx = rest.IndexOf('/');
        if (slashIdx < 0)
        {
            ctx.Response.StatusCode = 400;
            await WriteTextAsync(ctx.Response, "Invalid segment format");
            _logger.Warn("HandleSegment: invalid format {Path}", path);
            return;
        }

        var token = rest[..slashIdx].ToString();
        var filePath = rest[(slashIdx + 1)..].ToString();

        if (!_sessionEntries.TryGetValue(token, out var entry))
        {
            ctx.Response.StatusCode = 400;
            await WriteTextAsync(ctx.Response, "Invalid session token");
            _logger.Warn("HandleSegment: token no encontrado {Token}", token);
            return;
        }

        var segmentUrl = entry.BaseUrl.TrimEnd('/') + "/" + filePath.TrimStart('/');

        _logger.Debug("Segmento solicitado: {SegmentUrl}", segmentUrl);

        var request = new HttpRequestMessage(HttpMethod.Get, segmentUrl);
        var (_, currentHeaders) = GetConfigSnapshot();
        AddHeadersToRequest(request, currentHeaders);

        HttpResponseMessage response;
        try
        {
            response = await _httpClientSegments.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();
        }
        catch (HttpRequestException ex) when (ex.StatusCode.HasValue)
        {
            ctx.Response.StatusCode = (int)ex.StatusCode.Value;
            ctx.Response.StatusDescription = ex.StatusCode.Value.ToString();
            await WriteTextAsync(ctx.Response, $"Upstream error: {(int)ex.StatusCode.Value}");
            _logger.Warn("Segmento {Segment} retornó {StatusCode}", segmentUrl, (int)ex.StatusCode.Value);
            return;
        }

        ctx.Response.ContentType = response.Content.Headers.ContentType?.MediaType ?? "application/octet-stream";
        ctx.Response.Headers.Add("Access-Control-Allow-Origin", "*");
        ctx.Response.Headers.Add("Access-Control-Allow-Headers", "*");

        var contentLength = response.Content.Headers.ContentLength;
        if (contentLength.HasValue)
            ctx.Response.ContentLength64 = contentLength.Value;

        await response.Content.CopyToAsync(ctx.Response.OutputStream);
        ctx.Response.OutputStream.Close();
        _logger.Debug("Segmento servido ({Length} bytes): {Segment}", contentLength ?? 0, segmentUrl);
    }

    private async Task HandleLicenseAsync(HttpListenerContext ctx, string query)
    {
        var parsed = ParseQueryString(query);
        var licenseUrl = parsed.GetValueOrDefault("url");
        if (string.IsNullOrEmpty(licenseUrl))
        {
            ctx.Response.StatusCode = 400;
            await WriteTextAsync(ctx.Response, "Missing url parameter");
            _logger.Warn("HandleLicense: url parameter missing");
            return;
        }

        try
        {
            using var ms = new MemoryStream();
            await ctx.Request.InputStream.CopyToAsync(ms);
            var challengeBytes = ms.ToArray();

            _logger.Debug("License request para: {Url} ({Length} bytes)", licenseUrl, challengeBytes.Length);

            var request = new HttpRequestMessage(HttpMethod.Post, licenseUrl)
            {
                Content = new ByteArrayContent(challengeBytes)
            };
            request.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");

            var (_, currentHeaders) = GetConfigSnapshot();
            AddHeadersToRequest(request, currentHeaders);

            request.Headers.Referrer = new Uri("https://www.amazon.com/");
            request.Headers.Add("Origin", "https://www.amazon.com");
            request.Headers.Add("X-Requested-With", "XMLHttpRequest");

            var response = await _httpClient.SendAsync(request);
            var licenseBytes = await response.Content.ReadAsByteArrayAsync();

            ctx.Response.StatusCode = (int)response.StatusCode;
            ctx.Response.ContentType = response.Content.Headers.ContentType?.MediaType ?? "application/octet-stream";
            ctx.Response.ContentLength64 = licenseBytes.Length;
            ctx.Response.Headers.Add("Access-Control-Allow-Origin", "*");
            ctx.Response.Headers.Add("Access-Control-Allow-Headers", "*");

            await ctx.Response.OutputStream.WriteAsync(licenseBytes);
            ctx.Response.OutputStream.Close();

            _logger.Debug("License respuesta: {StatusCode}, {Length} bytes", (int)response.StatusCode, licenseBytes.Length);
        }
        catch (HttpRequestException ex) when (ex.HttpRequestError == HttpRequestError.NameResolutionError)
        {
            _logger.Warn(ex, "DNS no resuelve para servidor de licencia: {Url}", licenseUrl);
            ctx.Response.StatusCode = 502;
            await WriteTextAsync(ctx.Response, "License server DNS resolution failed");
        }
        catch (Exception ex)
        {
            _logger.Warn(ex, "Error en proxy de licencia para {Url}", licenseUrl);
            ctx.Response.StatusCode = 502;
            await WriteTextAsync(ctx.Response, $"Proxy license error: {ex.Message}");
        }
    }

    private static void AddHeadersToRequest(HttpRequestMessage request, Dictionary<string, string>? headers)
    {
        if (headers == null) return;
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

    private static async Task WriteTextAsync(HttpListenerResponse response, string text)
    {
        var bytes = Encoding.UTF8.GetBytes(text);
        response.ContentLength64 = bytes.Length;
        await response.OutputStream.WriteAsync(bytes);
        response.OutputStream.Close();
    }

    public async Task<DrmInfo?> DetectDrmAsync(string mpdUrl, Dictionary<string, string>? headers)
    {
        _logger.Info("Detectando DRM en MPD: {Url}", mpdUrl);
        try
        {
            var mpdResponse = headers != null
                ? await SendManifestRequestAsync(mpdUrl, headers)
                : await _httpClient.GetAsync(mpdUrl);

            mpdResponse.EnsureSuccessStatusCode();
            var mpdContent = await mpdResponse.Content.ReadAsStringAsync();

            var drm = _mpdRewriter.DetectDrm(mpdContent);
            if (drm != null)
            {
                _logger.Info("DRM auto-detectado: {SystemId}, license: {Url}", drm.SystemId, drm.LicenseUrl);
            }
            else
            {
                _logger.Debug("No se detectó DRM en el MPD");
            }

            return drm;
        }
        catch (Exception ex)
        {
            _logger.Warn(ex, "Error detectando DRM en MPD: {Url}", mpdUrl);
            return null;
        }
    }

    private static int GetAvailablePort()
    {
        using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        socket.Bind(new IPEndPoint(IPAddress.Loopback, 0));
        var port = ((IPEndPoint)socket.LocalEndPoint!).Port;
        _logger.Debug("Puerto disponible asignado: {Port}", port);
        return port;
    }

    private static Dictionary<string, string> ParseQueryString(string query)
    {
        var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrEmpty(query)) return dict;
        var trimmed = query.TrimStart('?');
        foreach (var pair in trimmed.Split('&'))
        {
            if (string.IsNullOrEmpty(pair)) continue;
            var parts = pair.Split('=', 2);
            var key = Uri.UnescapeDataString(parts[0]);
            var value = parts.Length > 1 ? Uri.UnescapeDataString(parts[1]) : "";
            dict[key] = value;
        }
        return dict;
    }

    public void Dispose()
    {
        Stop();
        _httpClient.Dispose();
        _httpClientSegments.Dispose();
        _listener.Close();
        _logger.Info("ProxyService disposed");
    }
}
