using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;
using NLog;

namespace FemonReverse;

/// <summary>
/// Handles bearer token acquisition and MPD URL signing for Flow CDN channels.
/// Replicates lambda$processSignedMpdIfNeeded$15 from PlayerActivity.
/// </summary>
public static class FlowSigner
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private static readonly HttpClient HttpNoRedirect = new(new HttpClientHandler
    {
        AllowAutoRedirect = false,
        ServerCertificateCustomValidationCallback = (_, _, _, _) => true
    });

    private static readonly HttpClient HttpWithRedirect = new(new HttpClientHandler
    {
        AllowAutoRedirect = true,
        ServerCertificateCustomValidationCallback = (_, _, _, _) => true
    });

    private const string DefaultUserAgent =
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/125.0.0.0 Safari/537.36";

    /// <summary>
    /// Identifies channels that require Flow CDN signing.
    /// A channel is "Flow" if its Referer header contains the Flow domain.
    /// </summary>
    public static List<FlowChannel> FindFlowChannels(
        List<CategoryItem> categories,
        string refererDom,
        string refererDomPersonal)
    {
        var result = new List<FlowChannel>();

        foreach (var cat in categories)
        {
            foreach (var ch in cat.Samples ?? [])
            {
                var flowType = ClassifyChannel(ch, refererDom, refererDomPersonal);
                if (flowType != null)
                    result.Add(flowType);
            }
            foreach (var ch in cat.HiddenSamples ?? [])
            {
                var flowType = ClassifyChannel(ch, refererDom, refererDomPersonal);
                if (flowType != null)
                    result.Add(flowType);
            }
        }

        return result;
    }

    private static FlowChannel? ClassifyChannel(ChannelItem ch, string refererDom, string refererDomPersonal)
    {
        if (string.IsNullOrEmpty(ch.Url)) return null;
        if (!ch.Url.EndsWith(".m3u8", StringComparison.OrdinalIgnoreCase) &&
            !ch.Url.Contains(".mpd", StringComparison.OrdinalIgnoreCase)) return null;

        var headers = ch.Headers;
        if (headers == null) return null;

        foreach (var kv in headers)
        {
            if (!"referer".Equals(kv.Key, StringComparison.OrdinalIgnoreCase)) continue;

            var refValue = kv.Value.ToLowerInvariant();

            if (!string.IsNullOrEmpty(refererDomPersonal) &&
                refValue.Contains(refererDomPersonal.ToLowerInvariant().Replace("https://", "").Replace("http://", "").TrimEnd('/')))
                return new FlowChannel { Channel = ch, IsPersonal = true };

            if (!string.IsNullOrEmpty(refererDom) &&
                refValue.Contains(refererDom.ToLowerInvariant().Replace("https://", "").Replace("http://", "").TrimEnd('/')))
                return new FlowChannel { Channel = ch, IsPersonal = false };
        }

        return null;
    }

    /// <summary>
    /// Fetches the bearer token from the bearer JSON URL.
    /// The Remote Config points to a JSON file that contains the token.
    /// </summary>
    public static async Task<string?> FetchBearerToken(string bearerJsonUrl)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, bearerJsonUrl);
            request.Headers.TryAddWithoutValidation("User-Agent", DefaultUserAgent);

            var response = await HttpWithRedirect.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                Logger.Warn("Bearer HTTP {0}: {1}", response.StatusCode, body[..Math.Min(200, body.Length)]);
                return null;
            }

            // Try to parse as JSON - check known field names
            try
            {
                using var doc = JsonDocument.Parse(body);

                // Priority order based on observed piratacodigo.json structure
                if (doc.RootElement.TryGetProperty("bearerToken", out var btProp))
                    return btProp.GetString();

                if (doc.RootElement.TryGetProperty("token", out var tokenProp))
                    return tokenProp.GetString();

                if (doc.RootElement.TryGetProperty("bearer", out var bearerProp))
                    return bearerProp.GetString();

                if (doc.RootElement.TryGetProperty("access_token", out var atProp))
                    return atProp.GetString();

                // Fallback: dump and try first string value
                Logger.Info("Bearer JSON structure: {0}", body[..Math.Min(300, body.Length)]);

                foreach (var prop in doc.RootElement.EnumerateObject())
                {
                    if (prop.Value.ValueKind == JsonValueKind.String)
                    {
                        var val = prop.Value.GetString();
                        if (!string.IsNullOrEmpty(val) && val.Length > 10)
                            return val;
                    }
                }
            }
            catch
            {
                // Not JSON - maybe the body itself is the token
                if (body.Length < 2000 && !body.Contains('<'))
                    return body.Trim().Trim('"');
            }

            return null;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Bearer token fetch failed");
            return null;
        }
    }

    /// <summary>
    /// Signs a Flow channel URL using the bearer token and CDN generator.
    /// Steps:
    /// 1. URL-encode the channel URL
    /// 2. Build CDN generator URL with the encoded path
    /// 3. Call CDN with bearer Authorization header
    /// 4. Extract signed URL from response
    /// 5. Follow redirects to get final playback URL
    /// </summary>
    public static async Task<string?> SignChannel(
        ChannelItem channel,
        string bearerToken,
        string cdnGenBase,
        string siteOrigin)
    {
        if (string.IsNullOrEmpty(bearerToken) || string.IsNullOrEmpty(cdnGenBase))
        {
            Logger.Warn("Missing bearer token or cdn_gen_base");
            return null;
        }

        try
        {
            // ===== STEP 1: Build CDN URL =====
            var originalUrl = channel.Url;

            // Extract path from the original URL for the CDN generator
            // The CDN expects the path URL-encoded (without host)
            var uri = new Uri(originalUrl);
            //var pathForCdn = uri.PathAndQuery; // e.g. /live/c3eds/.../index.mpd
            var encodedPath = Uri.EscapeDataString(uri.ToString());

            // Replace $encodedPath placeholder in the CDN template
            var cdnUrl = cdnGenBase.Replace("$encodedPath", encodedPath);
            if (cdnUrl == cdnGenBase) // No placeholder matched, try alternative patterns
                cdnUrl = BuildCdnUrl(cdnGenBase, encodedPath);

            Logger.Info("CDN URL: {0}", cdnUrl);

            // ===== STEP 2: Request signed token from CDN =====
            var cdnRequest = new HttpRequestMessage(HttpMethod.Get, cdnUrl);
            // Token ya incluye prefijo "Bearer " si viene del JSON
            var authHeader = bearerToken.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
                ? bearerToken
                : $"Bearer {bearerToken}";
            cdnRequest.Headers.TryAddWithoutValidation("Authorization", authHeader);
            cdnRequest.Headers.TryAddWithoutValidation("Origin", siteOrigin);
            cdnRequest.Headers.TryAddWithoutValidation("Referer", siteOrigin + "/");
            cdnRequest.Headers.TryAddWithoutValidation("User-Agent", DefaultUserAgent);

            var cdnResponse = await HttpWithRedirect.SendAsync(cdnRequest);
            var cdnBody = await cdnResponse.Content.ReadAsStringAsync();

            Logger.Info("CDN response ({0}): {1}", cdnResponse.StatusCode, cdnBody);

            // Extract signed URL or token from response
            string? signedToken = null;
            try
            {
                using var doc = JsonDocument.Parse(cdnBody);
                if (doc.RootElement.TryGetProperty("url", out var urlProp))
                    signedToken = urlProp.GetString();
                else if (doc.RootElement.TryGetProperty("token", out var tokProp))
                    signedToken = tokProp.GetString();
                else if (doc.RootElement.TryGetProperty("signedUrl", out var suProp))
                    signedToken = suProp.GetString();
            }
            catch
            {
                signedToken = cdnBody.Trim();
            }

            if (string.IsNullOrEmpty(signedToken))
            {
                Logger.Warn("No signed token in CDN response");
                return null;
            }

            // ===== STEP 3: Build final URL and follow redirects =====
            string finalUrl;
            if (signedToken.StartsWith("http"))
            {
                finalUrl = signedToken; // The CDN returned a full URL
            }
            else
            {
                // Append token to original URL
                var separator = originalUrl.Contains('?') ? "&" : "?";
                finalUrl = $"{originalUrl}{separator}cdntoken={signedToken}";
            }

            Logger.Info("Final URL: {0}", finalUrl);

            // Follow redirects to get the actual playback URL
            var finalRequest = new HttpRequestMessage(HttpMethod.Get, finalUrl);
            finalRequest.Headers.TryAddWithoutValidation("Origin", siteOrigin);
            finalRequest.Headers.TryAddWithoutValidation("Referer", siteOrigin + "/");
            finalRequest.Headers.TryAddWithoutValidation("User-Agent", DefaultUserAgent);

            var finalResponse = await HttpWithRedirect.SendAsync(finalRequest);
            var resolvedUrl = finalResponse.RequestMessage?.RequestUri?.ToString() ?? finalUrl;

            // Strip query params if needed
            var queryIdx = resolvedUrl.IndexOf('?');
            if (queryIdx >= 0)
                resolvedUrl = resolvedUrl[..queryIdx];

            Logger.Info("Resolved URL: {0}", resolvedUrl);
            return resolvedUrl;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Flow signer failed");
            return null;
        }
    }

    private static string BuildCdnUrl(string cdnGenBase, string encodedUrl)
    {
        if (cdnGenBase.Contains("{encoded_url}"))
            return cdnGenBase.Replace("{encoded_url}", encodedUrl);
        if (cdnGenBase.Contains("{url}"))
            return cdnGenBase.Replace("{url}", encodedUrl);
        if (cdnGenBase.EndsWith('/'))
            return cdnGenBase + encodedUrl;
        if (cdnGenBase.Contains('?'))
            return cdnGenBase + "&url=" + encodedUrl;
        return cdnGenBase + "?url=" + encodedUrl;
    }

    private static string Truncate(string? s, int max) =>
        string.IsNullOrEmpty(s) ? "(vacio)" : s.Length <= max ? s : s[..max] + "...";
}

public class FlowChannel
{
    public required ChannelItem Channel { get; set; }
    public bool IsPersonal { get; set; }
}
