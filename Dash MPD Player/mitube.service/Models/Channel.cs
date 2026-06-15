using System.Text.Json.Serialization;

namespace mitube.service.Models;

public class Channel
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("url")]
    public string Url { get; set; } = "";

    [JsonPropertyName("type")]
    public string Type { get; set; } = "";

    [JsonPropertyName("drm_license_uri")]
    public string DrmLicenseUri { get; set; } = "";

    [JsonPropertyName("icono")]
    public string Icono { get; set; } = "";

    [JsonPropertyName("headers")]
    public Dictionary<string, string>? Headers { get; set; }

    public (string keyId, string key)? ParseClearkeyLicense()
    {
        if (string.IsNullOrEmpty(DrmLicenseUri)) return null;
        var uri = new Uri(DrmLicenseUri);
        var query = uri.Query.TrimStart('?').Split('&')
            .Select(p => p.Split('=', 2))
            .Where(p => p.Length == 2)
            .ToDictionary(p => Uri.UnescapeDataString(p[0]), p => Uri.UnescapeDataString(p[1]), StringComparer.OrdinalIgnoreCase);
        query.TryGetValue("keyid", out var keyId);
        query.TryGetValue("key", out var key);
        if (!string.IsNullOrEmpty(keyId) && !string.IsNullOrEmpty(key))
            return (keyId, key);
        return null;
    }
}
