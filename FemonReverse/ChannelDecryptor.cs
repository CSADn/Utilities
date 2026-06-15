using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using NLog;

namespace FemonReverse;

/// <summary>
/// Downloads and decrypts the channel JSON (piratachanel.json).
/// Replicates the double AES/ECB decryption from EpisodeItemVertical.
/// </summary>
public static class ChannelDecryptor
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private static readonly HttpClient Http = new();

    /// <summary>
    /// Downloads the JSON from the given URL and decrypts all encrypted fields.
    /// </summary>
    public static async Task<List<CategoryItem>> DownloadAndDecrypt(string jsonUrl, string aesKey)
    {
        // Download JSON
        var json = await Http.GetStringAsync(jsonUrl);

        // Parse categories
        var categories = JsonSerializer.Deserialize<List<CategoryItem>>(json) ?? [];

        // Decrypt all fields
        DecryptAllCategories(categories, aesKey);

        return categories;
    }

    /// <summary>
    /// Decrypts all encrypted properties in the category list.
    /// Mirrors decryptCategoryList() from EpisodeItemVertical.java
    /// </summary>
    public static void DecryptAllCategories(List<CategoryItem> categories, string aesKey)
    {
        if (string.IsNullOrEmpty(aesKey)) return;

        foreach (var category in categories)
        {
            if (category.Samples == null) continue;

            foreach (var channel in category.Samples)
            {
                // Double-decrypt URL
                channel.Url = DecryptDouble(channel.Url, aesKey);

                // Double-decrypt DRM license URI
                channel.DrmLicenseUri = DecryptDouble(channel.DrmLicenseUri, aesKey);

                // Double-decrypt all header values
                DecryptMap(channel.Headers, aesKey);
                DecryptMap(channel.Headers2, aesKey);
                DecryptMap(channel.HeadersM3u8, aesKey);
                DecryptMap(channel.HeadersUrl, aesKey);
            }

            // Also handle hidden_samples if present
            if (category.HiddenSamples == null) continue;
            foreach (var channel in category.HiddenSamples)
            {
                channel.Url = DecryptDouble(channel.Url, aesKey);
                channel.DrmLicenseUri = DecryptDouble(channel.DrmLicenseUri, aesKey);
                DecryptMap(channel.Headers, aesKey);
                DecryptMap(channel.Headers2, aesKey);
                DecryptMap(channel.HeadersM3u8, aesKey);
                DecryptMap(channel.HeadersUrl, aesKey);
            }
        }
    }

    /// <summary>
    /// Decrypts all values in a header map.
    /// </summary>
    private static void DecryptMap(Dictionary<string, string>? map, string key)
    {
        if (map == null || map.Count == 0) return;

        var keys = map.Keys.ToList();
        foreach (var k in keys)
        {
            map[k] = DecryptDouble(map[k], key);
        }
    }

    /// <summary>
    /// Double AES decryption: decrypt(decrypt(value, key), key).
    /// Mirrors decryptPotentiallyDoubleEncryptedString() from the app.
    /// </summary>
    public static string DecryptDouble(string? encrypted, string key)
    {
        if (string.IsNullOrEmpty(encrypted)) return encrypted ?? "";

        // First layer
        var firstPass = AesDecrypt(encrypted, key);
        // Second layer
        var secondPass = AesDecrypt(firstPass, key);

        return secondPass;
    }

    /// <summary>
    /// AES/ECB/PKCS5Padding decryption with Base64 input.
    /// Mirrors EncryptionUtil.decrypt().
    /// </summary>
    public static string AesDecrypt(string base64Cipher, string keyString)
    {
        if (string.IsNullOrEmpty(base64Cipher)) return base64Cipher ?? "";

        try
        {
            var keyBytes = BuildKeyBytes(keyString);
            var cipherBytes = Convert.FromBase64String(base64Cipher);

            using var aes = Aes.Create();
            aes.Key = keyBytes;
            aes.Mode = CipherMode.ECB;
            aes.Padding = PaddingMode.PKCS7; // PKCS5 == PKCS7 in .NET

            using var decryptor = aes.CreateDecryptor();
            var plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);

            return Encoding.UTF8.GetString(plainBytes);
        }
        catch (Exception ex)
        {
            //Logger.Warn(ex, "AES decrypt failed, returning original");
            return base64Cipher;
        }
    }

    /// <summary>
    /// Smart decryption that supports multiple key formats and up to 3 layers.
    /// Mirrors smartDecrypt() from PeliSeriesNetflixActivity.
    /// </summary>
    public static string SmartDecrypt(string encrypted, string key)
    {
        if (string.IsNullOrEmpty(encrypted)) return encrypted ?? "";

        var keyBytes = BuildKeyBytes(key);
        var current = encrypted.Trim();

        for (var layer = 0; layer < 3; layer++)
        {
            try
            {
                var decoded = Convert.FromBase64String(current);
                var asText = Encoding.UTF8.GetString(decoded).Trim();

                // Check if already a valid URL
                if (IsProbablyUrl(asText)) return asText;

                // If length is multiple of 16, try AES
                if (decoded.Length % 16 == 0)
                {
                    try
                    {
                        using var aes = Aes.Create();
                        aes.Key = keyBytes;
                        aes.Mode = CipherMode.ECB;
                        aes.Padding = PaddingMode.PKCS7;

                        using var decryptor = aes.CreateDecryptor();
                        var plain = decryptor.TransformFinalBlock(decoded, 0, decoded.Length);
                        current = Encoding.UTF8.GetString(plain).Trim();

                        if (IsProbablyUrl(current)) return current;
                    }
                    catch (Exception ex)
                    {
                        Logger.Debug(ex, "SmartDecrypt layer {0} AES failed", layer);
                        current = asText;
                    }
                }
                else
                {
                    current = asText;
                }
            }
            catch (Exception ex)
            {
                Logger.Debug(ex, "SmartDecrypt layer {0} Base64 decode failed", layer);
            }
        }

        if (!IsProbablyUrl(current))
            Logger.Debug("SmartDecrypt returned non-URL after 3 layers: {0}", Truncate(current, 100));
        return current;
    }

    /// <summary>
    /// Builds AES key bytes from string. Supports Base64, Hex, or raw UTF-8.
    /// Mirrors buildKeyBytes() from PeliSeriesNetflixActivity.
    /// </summary>
    public static byte[] BuildKeyBytes(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return new byte[16];

        var clean = key.Trim().Replace(" ", "").Replace("\t", "");

        // Try Base64 first
        try
        {
            var decoded = Convert.FromBase64String(clean);
            if (decoded.Length == 16) return decoded;
        }
        catch
        {
            Logger.Debug("Base64 decode failed for key, trying hex");
        }

        // Try Hex (32 hex chars = 16 bytes)
        if (clean.Length == 32 && System.Text.RegularExpressions.Regex.IsMatch(clean, "^[0-9A-Fa-f]{32}$"))
        {
            var bytes = new byte[16];
            for (var i = 0; i < 16; i++)
                bytes[i] = Convert.ToByte(clean.Substring(i * 2, 2), 16);
            return bytes;
        }

        // Raw UTF-8 (pad/truncate to 16 bytes)
        var raw = Encoding.UTF8.GetBytes(clean);
        var result = new byte[16];
        Array.Copy(raw, result, Math.Min(raw.Length, 16));
        return result;
    }

    private static bool IsProbablyUrl(string s) =>
        s.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
        s.StartsWith("https://", StringComparison.OrdinalIgnoreCase);

    private static string Truncate(string? s, int max) =>
        string.IsNullOrEmpty(s) ? "(vacio)" : s.Length <= max ? s : s[..max] + "...";
}

// ===== DATA MODELS =====

public class CategoryItem
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("samples")]
    public List<ChannelItem>? Samples { get; set; }

    [JsonPropertyName("hidden_samples")]
    public List<ChannelItem>? HiddenSamples { get; set; }
}

public class ChannelItem
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("url")]
    public string Url { get; set; } = "";

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("drm_license_uri")]
    public string? DrmLicenseUri { get; set; }

    [JsonPropertyName("icono")]
    public string? Icono { get; set; }

    [JsonPropertyName("globalIndex")]
    public int GlobalIndex { get; set; }

    [JsonPropertyName("headers")]
    public Dictionary<string, string>? Headers { get; set; }

    [JsonPropertyName("headers2")]
    public Dictionary<string, string>? Headers2 { get; set; }

    [JsonPropertyName("headersM3u8")]
    public Dictionary<string, string>? HeadersM3u8 { get; set; }

    [JsonPropertyName("headersUrl")]
    public Dictionary<string, string>? HeadersUrl { get; set; }
}
