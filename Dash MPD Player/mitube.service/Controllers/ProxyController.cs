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
    // Separate clients for manifest/segments (same strategy as original WPF)
    private static readonly HttpClient _httpClientManifest = new HttpClient
    {
        Timeout = TimeSpan.FromSeconds(30),
        MaxResponseContentBufferSize = 50 * 1024 * 1024 // 50 MB
    };
    private static readonly HttpClient _httpClientSegment = new HttpClient
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

        try
        {
            var method = HttpContext.Request.Method.Equals("POST", StringComparison.OrdinalIgnoreCase)
                ? HttpMethod.Post
                : HttpMethod.Get;

            using var msg = new HttpRequestMessage(method, url);

            // Forward custom proxy headers as real HTTP headers
            if (!string.IsNullOrWhiteSpace(origin))
                msg.Headers.TryAddWithoutValidation("Origin", origin);
            if (!string.IsNullOrWhiteSpace(referer))
                msg.Headers.TryAddWithoutValidation("Referer", referer);
            if (!string.IsNullOrWhiteSpace(userAgent))
                msg.Headers.TryAddWithoutValidation("User-Agent", userAgent);

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
                var errorBody = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Proxy upstream error {Status} for {Url}: {Body}",
                    (int)response.StatusCode, url, errorBody.Length > 200 ? errorBody[..200] : errorBody);
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
        [FromServices] CdnTokenService cdnTokenService)
    {
        if (string.IsNullOrWhiteSpace(url))
            return BadRequest(new { error = "url query param is required" });

        try
        {
            var bearerToken = await cdnTokenService.GetBearerTokenAsync();
            var cdnToken = await cdnTokenService.RequestCdnTokenAsync(url, bearerToken, null);
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
