using System.Net;
using System.Text;
using System.Text.Json;
using NLog;

namespace FemonReverse;

public static class Program
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    // ===== FIREBASE CONFIG (extraido del APK) =====
    private const string FirebaseApiKey = "AIzaSyADcEYKamrewxL8CDA8NmAuRZjp8eZ2XzY";
    private const string FirebaseProjectId = "femon-play";
    private const string FirebaseAppId = "1:539591373021:android:88e80ca11e7a6d934aeb34";

    private static readonly HttpClient Http = new(new HttpClientHandler
    {
        AutomaticDecompression = DecompressionMethods.All,
        ServerCertificateCustomValidationCallback = (_, _, _, _) => true
    });

    public static async Task Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;
        Logger.Info("=== Femon Play Reverse Engineering Tool ===");

        try
        {
            // 1. Obtener Remote Config (NO requiere autenticacion)
            Logger.Info("[1] Obteniendo Firebase Remote Config (sin auth)...");
            var remoteConfig = await RemoteConfig.Fetch();
            Logger.Info("    Claves obtenidas: {0}", remoteConfig.Count);
            foreach (var kv in remoteConfig)
                Logger.Info("      {0} = {1}", kv.Key, Truncate(kv.Value, 90));

            // 2. Extraer clave AES y URL del JSON
            var claveApp = remoteConfig.GetValueOrDefault("claveapp", "");
            var jsonUrl = remoteConfig.GetValueOrDefault("json3_url",
                remoteConfig.GetValueOrDefault("json_url", "https://app.femon.net/pirata/piratachanel.json"));

            Logger.Info("    claveapp = \"{0}\" ({1} chars)", claveApp, claveApp.Length);
            Logger.Info("    json_url = \"{0}\"", jsonUrl);

            // 3. Descargar y descifrar piratachanel.json (o cargar desde disco si ya existe)
            var outputPath = Path.Combine(AppContext.BaseDirectory, "canales_descifrados.json");
            List<CategoryItem> categories;

            if (File.Exists(outputPath))
            {
                Logger.Info("[2] Cargando JSON descifrado desde disco: {0}", outputPath);
                var existingJson = await File.ReadAllTextAsync(outputPath);
                categories = JsonSerializer.Deserialize<List<CategoryItem>>(existingJson) ?? [];
                Logger.Info("    Categorias (desde disco): {0}", categories.Count);
            }
            else
            {
                Logger.Info("[2] Descargando JSON de canales...");
                categories = await ChannelDecryptor.DownloadAndDecrypt(jsonUrl, claveApp);
                Logger.Info("    Categorias: {0}", categories.Count);
            }

            var totalChannels = categories.Sum(c => (c.Samples?.Count ?? 0) + (c.HiddenSamples?.Count ?? 0));
            Logger.Info("    Canales totales: {0}", totalChannels);

            // Mostrar primeros canales descifrados
            Logger.Info("    --- Primeros canales descifrados ---");
            var shown = 0;
            foreach (var cat in categories)
            {
                if (shown >= 20) break;
                foreach (var ch in cat.Samples ?? [])
                {
                    if (shown >= 20) break;
                    Logger.Info("    [{0}] {1}", cat.Name, ch.Name);
                    Logger.Info("      Type: {0}", ch.Type ?? "HLS");
                    Logger.Info("      URL: {0}", Truncate(ch.Url, 120));
                    if (!string.IsNullOrEmpty(ch.DrmLicenseUri))
                        Logger.Info("      DRM: {0}", Truncate(ch.DrmLicenseUri, 100));
                    if (ch.Headers?.Count > 0)
                        foreach (var h in ch.Headers)
                            Logger.Info("      H[{0}]: {1}", h.Key, Truncate(h.Value, 80));
                    shown++;
                }
            }

            // 4. Identificar canales Flow y obtener bearer token
            Logger.Info("[3] Buscando canales Flow que requieran bearer token...");
            var bearerJsonUrl = remoteConfig.GetValueOrDefault("flow_bearer_json_url", "");
            var cdnGenBase = remoteConfig.GetValueOrDefault("flow_cdn_gen_base_url", "");
            var refererDom = remoteConfig.GetValueOrDefault("flow_referer_domain", "");
            var refererDomPersonal = remoteConfig.GetValueOrDefault("flow_referer_domain_personal", "");
            var siteOrigin = remoteConfig.GetValueOrDefault("flow_site_origin", "");
            var siteOriginPersonal = remoteConfig.GetValueOrDefault("flow_site_origin_personal", "");
            var cdnGenBasePersonal = remoteConfig.GetValueOrDefault("flow_cdn_gen_base_url_personal", "");
            var bearerJsonUrlPersonal = remoteConfig.GetValueOrDefault("flow_bearer_json_url_personal", "");

            Logger.Info("    bearer_json_url = {0}", bearerJsonUrl);
            Logger.Info("    cdn_gen_base = {0}", cdnGenBase);
            Logger.Info("    referer_dom = {0}", refererDom);
            Logger.Info("    site_origin = {0}", siteOrigin);

            var flowChannels = FlowSigner.FindFlowChannels(categories, refererDom, refererDomPersonal);
            Logger.Info("    Canales Flow detectados: {0}", flowChannels.Count);

            if (flowChannels.Count > 0 && !string.IsNullOrEmpty(bearerJsonUrl))
            {
                Logger.Info("[4] Obteniendo bearer token y firmando URL...");

                // Obtener bearer token desde el JSON de configuracion
                var bearerToken = await FlowSigner.FetchBearerToken(bearerJsonUrl);
                Logger.Info("    Bearer token: {0}", Truncate(bearerToken, 80));

                if (!string.IsNullOrEmpty(bearerToken))
                {
                    var firstFlow = flowChannels.First();
                    Logger.Info("    Canal: {0}", firstFlow.Channel.Name);
                    Logger.Info("    URL original: {0}", firstFlow.Channel.Url);

                    var origin = firstFlow.IsPersonal ? siteOriginPersonal : siteOrigin;
                    var cdnBase = firstFlow.IsPersonal ? cdnGenBasePersonal : cdnGenBase;

                    var signedUrl = await FlowSigner.SignChannel(
                        firstFlow.Channel, bearerToken, cdnBase, origin);

                    if (signedUrl != null)
                        Logger.Info("    URL firmada: {0}", signedUrl);
                    else
                        Logger.Warn("No se pudo obtener URL firmada.");
                }
            }
            else
            {
                Logger.Info("    No hay canales Flow o falta configuracion de bearer.");
            }

            // 5. Exportar JSON descifrado
            var jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
            await File.WriteAllTextAsync(outputPath, JsonSerializer.Serialize(categories, jsonOptions));
            Logger.Info("[OK] JSON descifrado exportado a: {0}", outputPath);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Fatal error");
        }
    }

    private static string Truncate(string? s, int max) =>
        string.IsNullOrEmpty(s) ? "(vacio)" : s.Length <= max ? s : s[..max] + "...";
}
