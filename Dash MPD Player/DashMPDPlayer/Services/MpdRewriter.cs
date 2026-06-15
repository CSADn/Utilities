using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using DashMPDPlayer.Models;
using NLog;

namespace DashMPDPlayer.Services;

public class MpdRewriter
{
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    private readonly string _proxyBaseUrl;

    private static readonly Regex MediaAttrRegex = new(
        @"(media|initialization)\s*=\s*""([^""]+)""",
        RegexOptions.Compiled);

    private static readonly Regex BaseUrlRegex = new(
        @"<BaseURL>([^<]+)</BaseURL>",
        RegexOptions.Compiled);

    private static readonly Dictionary<string, string> DrmSystemUuid = new(StringComparer.OrdinalIgnoreCase)
    {
        ["urn:uuid:edef8ba9-79d6-4ace-a3c8-27dcd51d21ed"] = "edef8ba9-79d6-4ace-a3c8-27dcd51d21ed",
        ["urn:uuid:9a04f079-9840-4286-ab92-e65be0885f95"] = "9a04f079-9840-4286-ab92-e65be0885f95",
        ["urn:uuid:e2719d58-a985-b3c9-781a-b030af78d30e"] = "e2719d58-a985-b3c9-781a-b030af78d30e",
    };

    public MpdRewriter(string proxyBaseUrl)
    {
        _proxyBaseUrl = proxyBaseUrl;
    }

    public string Rewrite(string mpdXml, string token)
    {
        try
        {
            return RewriteWithXml(mpdXml, token);
        }
        catch (Exception ex)
        {
            _logger.Warn(ex, "Error parseando MPD XML, usando rewrite regex como fallback");
            return RewriteWithRegex(mpdXml, token);
        }
    }

    public DrmInfo? DetectDrm(string mpdXml)
    {
        try
        {
            var doc = XDocument.Parse(mpdXml);
            XNamespace ns = "urn:mpeg:dash:schema:mpd:2011";

            foreach (var cp in doc.Descendants(ns + "ContentProtection"))
            {
                var schemeAttr = cp.Attribute("schemeIdUri")?.Value;
                if (schemeAttr == null || !DrmSystemUuid.TryGetValue(schemeAttr, out var systemId))
                    continue;

                var licenseUrl = ExtractLicenseUrl(cp);
                if (licenseUrl != null)
                {
                    _logger.Info("DRM detectado: {SystemId}, license URL: {Url}", systemId, licenseUrl);
                    return new DrmInfo(systemId, licenseUrl);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.Warn(ex, "Error detectando DRM en MPD");
        }

        return null;
    }

    private static string? ExtractLicenseUrl(XElement contentProtection)
    {
        XNamespace nsDashIf = "urn:dashif:org:mpd:2014";
        XNamespace nsMs = "urn:microsoft:playready";

        var laurl = contentProtection.Element(nsDashIf + "laurl")?.Value;
        if (!string.IsNullOrEmpty(laurl))
            return laurl;

        var msLaurl = contentProtection.Element(nsMs + "laurl")?.Value;
        if (!string.IsNullOrEmpty(msLaurl))
            return msLaurl;

        var msPro = contentProtection.Element(nsMs + "pro")?.Value;
        // Also try without namespace (some MPDs inline the element differently)
        if (string.IsNullOrEmpty(msPro))
            msPro = contentProtection.Element("pro")?.Value;

        if (!string.IsNullOrEmpty(msPro))
        {
            var url = ExtractLicenseUrlFromPlayReadyObject(msPro);
            if (url != null)
                return url;
        }

        return null;
    }

    private static string? ExtractLicenseUrlFromPlayReadyObject(string base64Pro)
    {
        try
        {
            var bytes = Convert.FromBase64String(base64Pro);
            if (bytes.Length < 10)
                return null;

            var recordCount = BitConverter.ToUInt16(bytes, 4);
            var offset = 6;
            for (int i = 0; i < recordCount; i++)
            {
                if (offset + 4 > bytes.Length)
                    break;

                var type = BitConverter.ToUInt16(bytes, offset);
                var length = BitConverter.ToUInt16(bytes, offset + 2);
                offset += 4;

                if (type != 1 || offset + length > bytes.Length)
                {
                    offset += length;
                    continue;
                }

                var xml = Encoding.Unicode.GetString(bytes, offset, length);

                var laUrlStart = xml.IndexOf("<LA_URL>", StringComparison.OrdinalIgnoreCase);
                if (laUrlStart < 0)
                    break;
                laUrlStart += 8;

                var laUrlEnd = xml.IndexOf("</LA_URL>", laUrlStart, StringComparison.OrdinalIgnoreCase);
                if (laUrlEnd < 0)
                    break;

                return xml[laUrlStart..laUrlEnd];
            }
        }
        catch (Exception ex)
        {
            _logger.Warn(ex, "Error parseando PlayReady Object para LA_URL");
        }

        return null;
    }

    private string RewriteWithXml(string mpdXml, string token)
    {
        var doc = XDocument.Parse(mpdXml);
        XNamespace ns = "urn:mpeg:dash:schema:mpd:2011";

        foreach (var segTemplate in doc.Descendants(ns + "SegmentTemplate"))
        {
            var media = segTemplate.Attribute("media");
            if (media != null)
            {
                var path = GetFileNameFromUrl(media.Value);
                media.Value = $"{_proxyBaseUrl}/segment/{token}/{path}";
            }

            var init = segTemplate.Attribute("initialization");
            if (init != null)
            {
                var path = GetFileNameFromUrl(init.Value);
                init.Value = $"{_proxyBaseUrl}/segment/{token}/{path}";
            }
        }

        foreach (var baseUrlEl in doc.Descendants(ns + "BaseURL").ToList())
        {
            var path = GetFileNameFromUrl(baseUrlEl.Value);
            baseUrlEl.Value = $"{_proxyBaseUrl}/segment/{token}/{path}";
        }

        return doc.ToString();
    }

    private string RewriteWithRegex(string mpdXml, string token)
    {
        var result = MediaAttrRegex.Replace(mpdXml, match =>
        {
            var attr = match.Groups[1].Value;
            var path = GetFileNameFromUrl(match.Groups[2].Value);
            return $@"{attr}=""{_proxyBaseUrl}/segment/{token}/{path}""";
        });

        result = BaseUrlRegex.Replace(result, match =>
        {
            var path = GetFileNameFromUrl(match.Groups[1].Value);
            return $"<BaseURL>{_proxyBaseUrl}/segment/{token}/{path}</BaseURL>";
        });

        return result;
    }

    private static string GetFileNameFromUrl(string url)
    {
        if (!url.StartsWith("http")) return url;
        var trimmed = url.TrimEnd('/');
        var lastSlash = trimmed.LastIndexOf('/');
        return lastSlash >= 0 ? trimmed[(lastSlash + 1)..] : trimmed;
    }
}
