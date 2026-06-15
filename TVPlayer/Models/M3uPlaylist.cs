using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace TVPlayer.Models
{
    /// <summary>
    /// Represents a loaded M3U/M3U8 playlist.
    /// </summary>
    public partial class M3uPlaylist : ObservableObject
    {
        [ObservableProperty]
        private string _name = "Playlist";

        [ObservableProperty]
        private string _sourceUrl = string.Empty;

        [ObservableProperty]
        private bool _isRemote;

        public ObservableCollection<M3uChannel> Channels { get; set; } = new();

        /// <summary>
        /// Returns all distinct group names found in this playlist.
        /// </summary>
        public IEnumerable<string> Groups =>
            Channels.Select(c => c.Group).Distinct().OrderBy(g => g);
    }
}
