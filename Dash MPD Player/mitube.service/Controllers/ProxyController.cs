using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using mitube.service.Services;

namespace mitube.service.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProxyController : ControllerBase
{
    private readonly ILogger<ProxyController> _logger;
    // Manifest client: don't follow redirects — Shaka's requestFilter re-routes the
    // redirect URL through the proxy, ensuring sip (source IP) consistency.
    private static readonly HttpClient _httpClientManifest = new HttpClient(new HttpClientHandler
    {
        AllowAutoRedirect = false
    })
    {
        Timeout = TimeSpan.FromSeconds(30),
        MaxResponseContentBufferSize = 50 * 1024 * 1024 // 50 MB
    };
    private static readonly HttpClient _httpClientSegment = new HttpClient(new HttpClientHandler
    {
        AllowAutoRedirect = false
    })
    {
        Timeout = TimeSpan.FromSeconds(120),
        MaxResponseContentBufferSize = 100 * 1024 * 1024 // 100 MB
    };

    public ProxyController(ILogger<ProxyController> logger)
    {
        _logger = logger;
    }

    [AllowAnonymous]
    [AcceptVerbs("GET", "POST")]
    [Route("fetch")]
    public async Task Fetch(
        [FromQuery] string url,
        [FromHeader(Name = "X-Proxy-Origin")] string? origin,
        [FromHeader(Name = "X-Proxy-Referer")] string? referer,
        [FromHeader(Name = "X-Proxy-User-Agent")] string? userAgent)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            HttpContext.Response.StatusCode = 400;
            await HttpContext.Response.WriteAsJsonAsync(new { error = "url query param is required" });
            return;
        }

        _logger.LogDebug("Proxy [{Method}] {Url}", HttpContext.Request.Method, url);

        // Log received X-Proxy-* headers for debugging header forwarding
        _logger.LogDebug("Proxy X-Proxy-Origin: {Origin}, X-Proxy-Referer: {Referer}, X-Proxy-User-Agent: {UserAgent}",
            origin ?? "(null)", referer ?? "(null)", userAgent ?? "(null)");

        try
        {
            var method = HttpContext.Request.Method.Equals("POST", StringComparison.OrdinalIgnoreCase)
                ? HttpMethod.Post
                : HttpMethod.Get;

            using var msg = new HttpRequestMessage(method, url);

            // Forward custom proxy headers as real HTTP headers.
            // If client doesn't provide them, use defaults for known Flow CDNs.
            var isFlowCdn = url.Contains("chromecast.cvattv.com.ar") || url.Contains("cdn-token.app.flow.com.ar") || url.Contains("edge-live");
            if (!string.IsNullOrWhiteSpace(origin))
                msg.Headers.TryAddWithoutValidation("Origin", origin);
            else if (isFlowCdn)
                msg.Headers.TryAddWithoutValidation("Origin", "https://portal.app.flow.com.py");
            if (!string.IsNullOrWhiteSpace(referer))
                msg.Headers.TryAddWithoutValidation("Referer", referer);
            else if (isFlowCdn)
                msg.Headers.TryAddWithoutValidation("Referer", "https://portal.app.flow.com.py/");
            if (!string.IsNullOrWhiteSpace(userAgent))
                msg.Headers.TryAddWithoutValidation("User-Agent", userAgent);
            else if (isFlowCdn)
                msg.Headers.TryAddWithoutValidation("User-Agent",
                    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/145.0.0.0 Safari/537.36");

            // Forward Range header (byte-range requests from Shaka)
            var rangeHeader = HttpContext.Request.Headers.Range.ToString();
            if (!string.IsNullOrWhiteSpace(rangeHeader))
                msg.Headers.TryAddWithoutValidation("Range", rangeHeader);

            // Forward POST body (Widevine license challenge)
            if (method == HttpMethod.Post)
            {
                using var memStream = new MemoryStream();
                await HttpContext.Request.Body.CopyToAsync(memStream);
                memStream.Position = 0;
                msg.Content = new StreamContent(memStream);
                // Forward Content-Type from incoming request
                var contentType = HttpContext.Request.ContentType;
                if (!string.IsNullOrWhiteSpace(contentType))
                    msg.Content.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse(contentType);
            }

            // Log the upstream headers we're about to send (first request only, avoid spam for segments)
            var isManifest = !url.Contains("/segment/") && !url.Contains(".mp4") && !url.Contains(".m4s") && !url.Contains(".ts");
            if (isManifest)
            {
                var originHdr = msg.Headers.TryGetValues("Origin", out var ov) ? string.Join(",", ov) : "(not set)";
                var refererHdr = msg.Headers.TryGetValues("Referer", out var rv) ? string.Join(",", rv) : "(not set)";
                var uaHdr = msg.Headers.TryGetValues("User-Agent", out var uv) ? string.Join(",", uv) : "(not set)";
                _logger.LogInformation("Proxy upstream headers: Origin={Origin}, Referer={Referer}, User-Agent={UserAgent}",
                    originHdr, refererHdr, uaHdr);
            }

            // Choose client based on URL path: segments get 120s timeout
            var isSegment = url.Contains("/segment/") || url.Contains(".mp4") || url.Contains(".m4s") || url.Contains(".ts");
            var client = isSegment ? _httpClientSegment : _httpClientManifest;

            using var response = await client.SendAsync(msg, HttpCompletionOption.ResponseHeadersRead);

            _logger.LogDebug("Proxy response [{Status}] for {Url}", (int)response.StatusCode, url);

            // Forward response headers
            foreach (var header in response.Headers)
            {
                HttpContext.Response.Headers[header.Key] = header.Value.ToArray();
            }
            foreach (var header in response.Content.Headers)
            {
                HttpContext.Response.Headers[header.Key] = header.Value.ToArray();
            }

            HttpContext.Response.StatusCode = (int)response.StatusCode;

            if (response.IsSuccessStatusCode)
            {
                // DASH manifests: inject <BaseURL> so Shaka resolves relative segment
                // URLs against the CDN origin, not against the proxy URL.
                var contentType = response.Content.Headers.ContentType?.MediaType ?? "";
                if (contentType.Contains("dash+xml") || contentType.Contains("mpd"))
                {
                    var body = await response.Content.ReadAsStringAsync();
                    var baseUrl = ExtractCdnBaseUrl(url);
                    if (!string.IsNullOrEmpty(baseUrl))
                    {
                        var mpdStart = body.IndexOf("<MPD", StringComparison.OrdinalIgnoreCase);
                        if (mpdStart >= 0)
                        {
                            var mpdTagEnd = body.IndexOf('>', mpdStart);
                            if (mpdTagEnd > mpdStart)
                            {
                                body = body.Insert(mpdTagEnd + 1, "\n<BaseURL>" + baseUrl + "</BaseURL>");
                            }
                        }
                    }
                    HttpContext.Response.ContentLength = Encoding.UTF8.GetByteCount(body);
                    await HttpContext.Response.WriteAsync(body, Encoding.UTF8);
                }
                else
                {
                    var stream = await response.Content.ReadAsStreamAsync();
                    await stream.CopyToAsync(HttpContext.Response.Body);
                }
            }
            else
            {
                var statusCode = (int)response.StatusCode;
                // Log redirect responses (3xx) with Location header for diagnosis
                if (statusCode is >= 300 and < 400)
                {
                    var location = response.Headers.Location?.ToString() ?? "(none)";
                    _logger.LogWarning("Proxy redirect [{Status}] for {Url} -> {Location}",
                        statusCode, url, location);
                }
                else if (statusCode == 403)
                {
                    // Log 403 response body to diagnose CDN rejection reason
                    var errorBody = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Proxy upstream error 403 for {Url} - Body: {Body}",
                        url, errorBody.Length > 500 ? errorBody[..500] : errorBody);
                }
                else
                {
                    var errorBody = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Proxy upstream error {Status} for {Url}: {Body}",
                        statusCode, url, errorBody.Length > 200 ? errorBody[..200] : errorBody);
                }
                await response.Content.CopyToAsync(HttpContext.Response.Body);
            }
        }
        catch (TaskCanceledException)
        {
            _logger.LogWarning("Proxy timeout for {Url}", url);
            HttpContext.Response.StatusCode = 504;
            await HttpContext.Response.WriteAsJsonAsync(new { error = "Upstream request timed out" });
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Proxy error for {Url}", url);
            HttpContext.Response.StatusCode = 502;
            await HttpContext.Response.WriteAsJsonAsync(new { error = $"Upstream request failed: {ex.Message}" });
        }
    }

    [Authorize]
    [HttpGet("cdn-token")]
    public async Task<IActionResult> GetCdnToken(
        [FromQuery] string url,
        [FromHeader(Name = "X-Proxy-Origin")] string? origin,
        [FromHeader(Name = "X-Proxy-Referer")] string? referer,
        [FromHeader(Name = "X-Proxy-User-Agent")] string? userAgent,
        [FromServices] CdnTokenService cdnTokenService)
    {
        if (string.IsNullOrWhiteSpace(url))
            return BadRequest(new { error = "url query param is required" });

        try
        {
            // Build channel headers dictionary from client-provided proxy headers
            var channelHeaders = new Dictionary<string, string>();
            if (!string.IsNullOrWhiteSpace(origin)) channelHeaders["Origin"] = origin;
            if (!string.IsNullOrWhiteSpace(referer)) channelHeaders["Referer"] = referer;
            if (!string.IsNullOrWhiteSpace(userAgent)) channelHeaders["User-Agent"] = userAgent;

            _logger.LogInformation("GetCdnToken: received channel headers - Origin={Origin}, Referer={Referer}, User-Agent={UserAgent}",
                origin ?? "(null)", referer ?? "(null)", userAgent ?? "(null)");

            // URLs with embedded tok_ JWT don't need CDN token generation
            if (url.Contains("/tok_"))
            {
                _logger.LogInformation("GetCdnToken: URL has embedded token, skipping");
                return Ok(new { cdnToken = (string?)null, bearerToken = (string?)null, message = "embedded" });
            }

            var bearerToken = await cdnTokenService.GetBearerTokenAsync();
            var cdnToken = await cdnTokenService.RequestCdnTokenAsync(
                url, bearerToken, channelHeaders.Count > 0 ? channelHeaders : null);
            return Ok(new { cdnToken, bearerToken });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get CDN token for {Url}", url);
            return StatusCode(502, new { error = $"Failed to obtain CDN token: {ex.Message}" });
        }
    }

    /// Extracts the CDN base URL (scheme + host + path up to last "/")
    /// from the original upstream URL so we can inject it as <BaseURL>
    /// in DASH manifests.
    private static string ExtractCdnBaseUrl(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            return "";
        var path = uri.AbsolutePath;
        var lastSlash = path.LastIndexOf('/');
        if (lastSlash > 0)
            return $"{uri.Scheme}://{uri.Host}{path[..(lastSlash + 1)]}";
        return $"{uri.Scheme}://{uri.Host}/";
    }
}
