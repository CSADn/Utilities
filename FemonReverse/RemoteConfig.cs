using System.Text;
using System.Text.Json;
using NLog;

namespace FemonReverse;

/// <summary>
/// Fetches Firebase Remote Config values using the REST API.
/// NO authentication required - only API Key + App ID.
/// </summary>
public static class RemoteConfig
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private const string ProjectId = "femon-play";
    private const string ApiKey = "AIzaSyADcEYKamrewxL8CDA8NmAuRZjp8eZ2XzY";
    private const string AppId = "1:539591373021:android:88e80ca11e7a6d934aeb34";
    private const string PackageName = "com.example.myapplication";

    private static readonly HttpClient Http = new();

    /// <summary>
    /// Fetches all Remote Config parameters without authentication.
    /// The Firebase project allows unauthenticated access to Remote Config.
    /// </summary>
    public static async Task<Dictionary<string, string>> Fetch()
    {
        var url = $"https://firebaseremoteconfig.googleapis.com/v1/projects/{ProjectId}/namespaces/firebase:fetch?key={ApiKey}";

        var payload = new
        {
            appId = AppId,
            appInstanceId = Guid.NewGuid().ToString("N"),
            appInstanceIdToken = "",
            appVersion = "7.0",
            countryCode = "PY",
            languageCode = "es",
            platformVersion = "28",
            sdkVersion = "21.6.4",
            packageName = PackageName,
            analyticsUserProperties = new { }
        };

        var json = JsonSerializer.Serialize(payload);
        var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };

        request.Headers.TryAddWithoutValidation("X-Goog-Api-Key", ApiKey);
        request.Headers.TryAddWithoutValidation("X-Android-Package", PackageName);
        request.Headers.TryAddWithoutValidation("X-Firebase-GMPID", AppId);
        request.Headers.TryAddWithoutValidation("User-Agent", "Firebase/5/21.0.0/28/Android");
        request.Headers.TryAddWithoutValidation("X-Firebase-AppCheck", "null");

        var response = await Http.SendAsync(request);
        var body = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new Exception($"Remote Config fetch failed ({response.StatusCode}): {body[..Math.Min(300, body.Length)]}");

        using var doc = JsonDocument.Parse(body);
        var result = new Dictionary<string, string>();

        if (doc.RootElement.TryGetProperty("entries", out var entries))
        {
            foreach (var prop in entries.EnumerateObject())
                result[prop.Name] = prop.Value.GetString() ?? "";
        }

        return result;
    }
}
