using TVPlayer.Models;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;

namespace TVPlayer.Services
{
    /// <summary>
    /// Parses M3U / M3U8 playlist files and returns a structured <see cref="M3uPlaylist"/>.
    /// Supports: #EXTM3U, #EXTINF, tvg-id, tvg-name, tvg-logo, group-title, and arbitrary attributes.
    /// </summary>
    public static class M3uParser
    {
        // Matches key="value" or key=value attribute pairs on a #EXTINF line
        private static readonly Regex AttributeRegex =
            new(@"([\w-]+)=""([^""]*)""|(?:([\w-]+)=(\S+))", RegexOptions.Compiled);

        /// <summary>
        /// Loads a playlist from a local file path.
        /// </summary>
        public static async Task<M3uPlaylist> ParseFileAsync(string filePath)
        {
            var content = await File.ReadAllTextAsync(filePath);
            var playlist = Parse(content);
            playlist.Name = Path.GetFileNameWithoutExtension(filePath);
            playlist.SourceUrl = filePath;
            playlist.IsRemote = false;
            return playlist;
        }

        /// <summary>
        /// Downloads and parses a playlist from a remote URL.
        /// </summary>
        public static async Task<M3uPlaylist> ParseUrlAsync(string url)
        {
            using var http = new HttpClient();
            http.DefaultRequestHeaders.UserAgent.ParseAdd("TVPlayer/1.0");
            http.Timeout = TimeSpan.FromSeconds(30);

            var content = await http.GetStringAsync(url);
            var playlist = Parse(content);
            playlist.Name = ExtractPlaylistName(url);
            playlist.SourceUrl = url;
            playlist.IsRemote = true;
            return playlist;
        }

        /// <summary>
        /// Core parsing logic – works on raw M3U text content.
        /// </summary>
        public static M3uPlaylist Parse(string content)
        {
            var playlist = new M3uPlaylist();
            var lines = content.Split('\n', StringSplitOptions.None);

            M3uChannel? pending = null;

            foreach (var rawLine in lines)
            {
                var line = rawLine.TrimEnd('\r').Trim();

                if (string.IsNullOrWhiteSpace(line))
                    continue;

                if (line.StartsWith("#EXTM3U", StringComparison.OrdinalIgnoreCase))
                {
                    // Optional: parse playlist-level attributes here
                    continue;
                }

                if (line.StartsWith("#EXTINF:", StringComparison.OrdinalIgnoreCase))
                {
                    pending = ParseExtInf(line);
                    continue;
                }

                // Skip other directives unless they look like a URL / file path
                if (line.StartsWith('#'))
                    continue;

                // This should be a media URL
                var channel = pending ?? new M3uChannel();
                channel.Url = line;

                // If no name was parsed, derive one from the URL
                if (string.IsNullOrWhiteSpace(channel.Name))
                    channel.Name = ExtractNameFromUrl(line);

                playlist.Channels.Add(channel);
                pending = null;
            }

            return playlist;
        }

        // -----------------------------------------------------------------
        //  Helpers
        // -----------------------------------------------------------------

        private static M3uChannel ParseExtInf(string line)
        {
            var channel = new M3uChannel();

            // The format is: #EXTINF:<duration> [attributes...],<display-name>
            var commaIndex = line.LastIndexOf(',');
            if (commaIndex >= 0)
                channel.Name = line[(commaIndex + 1)..].Trim();

            foreach (Match m in AttributeRegex.Matches(line))
            {
                var key   = (m.Groups[1].Success ? m.Groups[1].Value : m.Groups[3].Value).ToLowerInvariant();
                var value = m.Groups[2].Success ? m.Groups[2].Value : m.Groups[4].Value;

                switch (key)
                {
                    case "tvg-id":           channel.TvgId    = value; break;
                    case "tvg-name":         channel.TvgName  = value; break;
                    case "tvg-logo":         channel.LogoUrl  = value; break;
                    case "group-title":      channel.Group    = value; break;
                    case "tvg-language":     channel.Language = value; break;
                    case "tvg-country":      channel.Country  = value; break;
                    default:
                        channel.ExtraAttributes[key] = value;
                        break;
                }
            }

            return channel;
        }

        private static string ExtractPlaylistName(string url)
        {
            try
            {
                var uri = new Uri(url);
                var seg = uri.Segments.LastOrDefault()?.TrimEnd('/') ?? string.Empty;
                return string.IsNullOrWhiteSpace(seg) ? uri.Host : Path.GetFileNameWithoutExtension(seg);
            }
            catch
            {
                return "Remote Playlist";
            }
        }

        private static string ExtractNameFromUrl(string url)
        {
            try
            {
                var uri = new Uri(url);
                var seg = uri.Segments.LastOrDefault()?.TrimEnd('/') ?? string.Empty;
                return string.IsNullOrWhiteSpace(seg) ? url : Path.GetFileNameWithoutExtension(seg);
            }
            catch
            {
                return url;
            }
        }
    }
}
