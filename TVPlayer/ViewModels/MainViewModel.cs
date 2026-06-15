using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using TVPlayer.Models;
using TVPlayer.Services;

namespace TVPlayer.ViewModels
{
    /// <summary>
    /// Main window ViewModel – manages the playlist list, search, group filter and
    /// delegates video playback to <see cref="PlayerViewModel"/>.
    /// </summary>
    public partial class MainViewModel : ObservableObject
    {
        // ----------------------------------------------------------------
        //  Child VM
        // ----------------------------------------------------------------
        public PlayerViewModel Player { get; } = new();

        // ----------------------------------------------------------------
        //  Playlists & channels
        // ----------------------------------------------------------------
        public ObservableCollection<M3uPlaylist> Playlists { get; } = new();

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(FilteredChannels))]
        [NotifyPropertyChangedFor(nameof(Groups))]
        private M3uPlaylist? _selectedPlaylist;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(FilteredChannels))]
        private M3uChannel? _selectedChannel;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(FilteredChannels))]
        private string _searchText = string.Empty;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(FilteredChannels))]
        private string _selectedGroup = "All";

        [ObservableProperty] private bool _isLoadingPlaylist;
        [ObservableProperty] private string _loadError = string.Empty;
        [ObservableProperty] private string _urlInput = string.Empty;

        // ----------------------------------------------------------------
        //  Derived / computed
        // ----------------------------------------------------------------
        public IEnumerable<M3uChannel> FilteredChannels
        {
            get
            {
                if (SelectedPlaylist is null) return Enumerable.Empty<M3uChannel>();

                IEnumerable<M3uChannel> src = SelectedPlaylist.Channels;

                if (!string.IsNullOrWhiteSpace(SelectedGroup) && SelectedGroup != "All")
                    src = src.Where(c => c.Group == SelectedGroup);

                if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    var q = SearchText.Trim().ToLowerInvariant();
                    src = src.Where(c =>
                        c.Name.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                        c.Group.Contains(q, StringComparison.OrdinalIgnoreCase));
                }

                return src;
            }
        }

        public IEnumerable<string> Groups
        {
            get
            {
                if (SelectedPlaylist is null) return ["All"];
                return new[] { "All" }.Concat(SelectedPlaylist.Groups);
            }
        }

        // ----------------------------------------------------------------
        //  Commands – Playlist management
        // ----------------------------------------------------------------

        [RelayCommand]
        private async Task OpenFileAsync()
        {
            var dlg = new OpenFileDialog
            {
                Title  = "Open M3U / M3U8 Playlist",
                Filter = "Playlist files (*.m3u;*.m3u8)|*.m3u;*.m3u8|All files (*.*)|*.*"
            };

            if (dlg.ShowDialog() != true) return;

            await LoadPlaylistFromFileAsync(dlg.FileName);
        }

        [RelayCommand]
        private async Task OpenUrlAsync()
        {
            var url = UrlInput.Trim();
            if (string.IsNullOrWhiteSpace(url)) return;

            // If it looks like a media stream (not a playlist), play it directly
            if (!url.Contains(".m3u", StringComparison.OrdinalIgnoreCase) &&
                !url.Contains(".m3u8", StringComparison.OrdinalIgnoreCase) &&
                !url.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
            {
                Player.OpenUrl(url);
                return;
            }

            await LoadPlaylistFromUrlAsync(url);
        }

        [RelayCommand]
        private void RemovePlaylist(M3uPlaylist? playlist)
        {
            if (playlist is null) return;
            Playlists.Remove(playlist);
            if (SelectedPlaylist == playlist)
                SelectedPlaylist = Playlists.FirstOrDefault();
        }

        [RelayCommand]
        private void PlayChannel(M3uChannel? channel)
        {
            if (channel is null) return;
            SelectedChannel = channel;
            Player.OpenChannel(channel);
        }

        // ----------------------------------------------------------------
        //  Loaders
        // ----------------------------------------------------------------
        private async Task LoadPlaylistFromFileAsync(string path)
        {
            IsLoadingPlaylist = true;
            LoadError = string.Empty;
            try
            {
                var playlist = await M3uParser.ParseFileAsync(path);
                Playlists.Add(playlist);
                SelectedPlaylist = playlist;

                // Persist so it can be restored on next launch
                UserPreferences.LastPlaylistPath = path;
            }
            catch (Exception ex)
            {
                LoadError = $"Failed to load file: {ex.Message}";
                MessageBox.Show(LoadError, "Load Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            finally { IsLoadingPlaylist = false; }
        }

        private async Task LoadPlaylistFromUrlAsync(string url)
        {
            IsLoadingPlaylist = true;
            LoadError = string.Empty;
            try
            {
                var playlist = await M3uParser.ParseUrlAsync(url);
                Playlists.Add(playlist);
                SelectedPlaylist = playlist;
                UrlInput = string.Empty;
            }
            catch (Exception ex)
            {
                LoadError = $"Failed to load URL: {ex.Message}";
                MessageBox.Show(LoadError, "Load Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            finally { IsLoadingPlaylist = false; }
        }

        // ----------------------------------------------------------------
        //  Startup restore
        // ----------------------------------------------------------------
        /// <summary>
        /// Loads the last playlist file opened, if it still exists on disk.
        /// Called once after the main window is rendered.
        /// </summary>
        public async Task LoadLastPlaylistAsync()
        {
            var path = UserPreferences.LastPlaylistPath;
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
                return;

            await LoadPlaylistFromFileAsync(path);

            // Restore the saved group filter if it still exists in the playlist
            var savedGroup = UserPreferences.LastSelectedGroup;
            if (Groups.Contains(savedGroup))
                SelectedGroup = savedGroup;
        }

        // ----------------------------------------------------------------
        //  Property change reactions
        // ----------------------------------------------------------------
        partial void OnSelectedChannelChanged(M3uChannel? value)
        {
            if (value is not null)
                Player.OpenChannel(value);
        }

        partial void OnSelectedGroupChanged(string value)
        {
            UserPreferences.LastSelectedGroup = value;
        }
    }
}
