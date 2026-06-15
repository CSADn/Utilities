using System.IO;
using System.Text.Json;

namespace TVPlayer.Services
{
    /// <summary>
    /// Persists user preferences to a JSON file in %AppData%\TVPlayer\prefs.json.
    /// </summary>
    public static class UserPreferences
    {
        private static readonly string FilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "TVPlayer", "prefs.json");

        private sealed class Prefs
        {
            public string LastPlaylistPath { get; set; } = string.Empty;
            public string LastSelectedGroup { get; set; } = "All";
            public float Volume { get; set; } = 100f;
        }

        private static Prefs _cache = Load();

        // ── Public API ────────────────────────────────────────────────────

        public static string LastPlaylistPath
        {
            get => _cache.LastPlaylistPath;
            set { _cache.LastPlaylistPath = value; Save(); }
        }

        public static string LastSelectedGroup
        {
            get => _cache.LastSelectedGroup;
            set { _cache.LastSelectedGroup = value; Save(); }
        }

        public static float Volume
        {
            get => _cache.Volume;
            set { _cache.Volume = value; Save(); }
        }

        // ── Persistence ───────────────────────────────────────────────────

        private static Prefs Load()
        {
            try
            {
                if (File.Exists(FilePath))
                {
                    var json = File.ReadAllText(FilePath);
                    return JsonSerializer.Deserialize<Prefs>(json) ?? new Prefs();
                }
            }
            catch { /* corrupt file – use defaults */ }

            return new Prefs();
        }

        private static void Save()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
                var json = JsonSerializer.Serialize(_cache,
                    new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(FilePath, json);
            }
            catch { /* non-critical – silently ignore */ }
        }
    }
}
