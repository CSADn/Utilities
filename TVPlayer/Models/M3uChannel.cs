using CommunityToolkit.Mvvm.ComponentModel;

namespace TVPlayer.Models
{
    /// <summary>
    /// Represents a single channel/entry in an M3U playlist.
    /// </summary>
    public partial class M3uChannel : ObservableObject
    {
        [ObservableProperty]
        private string _name = string.Empty;

        [ObservableProperty]
        private string _url = string.Empty;

        [ObservableProperty]
        private string _logoUrl = string.Empty;

        [ObservableProperty]
        private string _group = string.Empty;

        [ObservableProperty]
        private string _language = string.Empty;

        [ObservableProperty]
        private string _country = string.Empty;

        [ObservableProperty]
        private string _tvgId = string.Empty;

        [ObservableProperty]
        private string _tvgName = string.Empty;

        /// <summary>
        /// Extra EXTINF attributes not captured above.
        /// </summary>
        public Dictionary<string, string> ExtraAttributes { get; set; } = new();

        public override string ToString() => Name;
    }
}
