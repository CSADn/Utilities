using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LibVLCSharp.Shared;
using TVPlayer.Models;
using TVPlayer.Services;
using System.Windows;

namespace TVPlayer.ViewModels
{
    /// <summary>
    /// ViewModel for the video player. Wraps LibVLC MediaPlayer and exposes
    /// bindable properties for all playback controls.
    /// </summary>
    public partial class PlayerViewModel : ObservableObject, IDisposable
    {
        // ----------------------------------------------------------------
        //  LibVLC core objects (must live on the full app lifetime)
        // ----------------------------------------------------------------
        private readonly LibVLC _libVlc;
        public MediaPlayer MediaPlayer { get; }

        // ----------------------------------------------------------------
        //  Observable state
        // ----------------------------------------------------------------
        [ObservableProperty] private bool _isPlaying;
        [ObservableProperty] private bool _isPaused;
        [ObservableProperty] private bool _isStopped = true;
        [ObservableProperty] private bool _isMuted;
        [ObservableProperty] private bool _isBuffering;
        [ObservableProperty] private bool _isFullScreen;
        [ObservableProperty] private float _volume = UserPreferences.Volume;
        [ObservableProperty] private float _position;           // 0.0 – 1.0
        [ObservableProperty] private long  _duration;           // milliseconds
        [ObservableProperty] private long  _time;               // milliseconds
        [ObservableProperty] private string _timeDisplay = "00:00:00";
        [ObservableProperty] private string _durationDisplay = "00:00:00";
        [ObservableProperty] private string _statusMessage = "Ready";
        [ObservableProperty] private string _currentChannelName = string.Empty;
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(CopyUrlToClipboardCommand))]
        private string _currentChannelUrl = string.Empty;
        [ObservableProperty] private bool _seekBarDragging;
        [ObservableProperty] private string _copyUrlFeedback = string.Empty;

        // ----------------------------------------------------------------
        //  Constructor
        // ----------------------------------------------------------------
        public PlayerViewModel()
        {
            Core.Initialize();
            _libVlc = new LibVLC(enableDebugLogs: false,
                "--network-caching=3000",
                "--live-caching=3000",
                "--disc-caching=300",
                "--file-caching=300",
                "--no-video-title-show");

            MediaPlayer = new MediaPlayer(_libVlc);

            // Wire LibVLC events → UI thread
            MediaPlayer.Playing   += (_, _) => Dispatch(() => { IsPlaying = true; IsPaused = false; IsStopped = false; StatusMessage = "Playing"; });
            MediaPlayer.Paused    += (_, _) => Dispatch(() => { IsPlaying = false; IsPaused = true; StatusMessage = "Paused"; });
            MediaPlayer.Stopped   += (_, _) => Dispatch(() => { IsPlaying = false; IsPaused = false; IsStopped = true; Position = 0; Time = 0; StatusMessage = "Stopped"; CurrentChannelName = string.Empty; CurrentChannelUrl = string.Empty; });
            MediaPlayer.Buffering += (_, e) => Dispatch(() => { IsBuffering = e.Cache < 100; StatusMessage = IsBuffering ? $"Buffering {e.Cache:0}%…" : "Playing"; });
            MediaPlayer.EndReached+= (_, _) => Dispatch(() => { IsPlaying = false; IsStopped = true; StatusMessage = "Ended"; });
            MediaPlayer.EncounteredError += (_, _) => Dispatch(() => { StatusMessage = "Error – stream unavailable"; IsStopped = true; IsPlaying = false; });

            MediaPlayer.TimeChanged += (_, e) =>
                Dispatch(() =>
                {
                    Time = e.Time;
                    TimeDisplay = FormatMs(e.Time);
                    if (!SeekBarDragging && Duration > 0)
                        Position = (float)e.Time / Duration;
                });

            MediaPlayer.LengthChanged += (_, e) =>
                Dispatch(() =>
                {
                    Duration = e.Length;
                    DurationDisplay = FormatMs(e.Length);
                });

            MediaPlayer.Muted   += (_, _) => Dispatch(() => IsMuted = true);
            MediaPlayer.Unmuted += (_, _) => Dispatch(() => IsMuted = false);
        }

        // ----------------------------------------------------------------
        //  Commands
        // ----------------------------------------------------------------

        [RelayCommand]
        public void Play()
        {
            if (IsPaused)
                MediaPlayer.Play();
        }

        [RelayCommand]
        public void Pause()
        {
            if (IsPlaying)
                MediaPlayer.Pause();
        }

        [RelayCommand]
        public void Stop()
        {
            MediaPlayer.Stop();
        }

        [RelayCommand]
        public void ToggleMute()
        {
            MediaPlayer.ToggleMute();
        }

        [RelayCommand]
        public void ToggleFullScreen()
        {
            IsFullScreen = !IsFullScreen;
        }

        [RelayCommand]
        public void SeekForward()
        {
            if (Duration > 0)
                MediaPlayer.Time = Math.Min(MediaPlayer.Time + 10_000, Duration);
        }

        [RelayCommand]
        public void SeekBackward()
        {
            MediaPlayer.Time = Math.Max(MediaPlayer.Time - 10_000, 0);
        }

        [RelayCommand(CanExecute = nameof(CanCopyUrl))]
        public async Task CopyUrlToClipboard()
        {
            Clipboard.SetText(CurrentChannelUrl);
            CopyUrlFeedback = "✔ Copied!";
            await Task.Delay(2000);
            CopyUrlFeedback = string.Empty;
        }

        private bool CanCopyUrl() => !string.IsNullOrWhiteSpace(CurrentChannelUrl);

        /// <summary>
        /// Opens a channel URL for playback.
        /// </summary>
        public void OpenChannel(M3uChannel channel)
        {
            if (string.IsNullOrWhiteSpace(channel.Url)) return;

            CurrentChannelName = channel.Name;
            CurrentChannelUrl  = channel.Url;
            StatusMessage = $"Loading: {channel.Name}";

            var media = new Media(_libVlc, new Uri(channel.Url));
            // Improve HLS / TS streaming
            media.AddOption(":network-caching=3000");
            media.AddOption(":clock-jitter=0");
            media.AddOption(":clock-synchro=0");

            MediaPlayer.Play(media);
        }

        /// <summary>
        /// Opens any URL or file path directly.
        /// </summary>
        public void OpenUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url)) return;

            CurrentChannelName = url;
            CurrentChannelUrl  = url;
            StatusMessage = $"Loading…";

            Media media;
            if (Uri.IsWellFormedUriString(url, UriKind.Absolute))
                media = new Media(_libVlc, new Uri(url));
            else
                media = new Media(_libVlc, url, FromType.FromPath);

            media.AddOption(":network-caching=3000");
            MediaPlayer.Play(media);
        }

        /// <summary>
        /// Called while the seek-bar thumb is being dragged.
        /// </summary>
        public void Seek(float positionRatio)
        {
            if (Duration > 0)
                MediaPlayer.Time = (long)(positionRatio * Duration);
        }

        // ----------------------------------------------------------------
        //  Volume property handler
        // ----------------------------------------------------------------
        partial void OnVolumeChanged(float value)
        {
            MediaPlayer.Volume = (int)Math.Clamp(value, 0, 200);
            UserPreferences.Volume = value;
        }

        // ----------------------------------------------------------------
        //  Helpers
        // ----------------------------------------------------------------
        private static void Dispatch(Action a) =>
            Application.Current?.Dispatcher.Invoke(a);

        private static string FormatMs(long ms)
        {
            var ts = TimeSpan.FromMilliseconds(ms);
            return ts.TotalHours >= 1
                ? ts.ToString(@"h\:mm\:ss")
                : ts.ToString(@"mm\:ss");
        }

        // ----------------------------------------------------------------
        //  IDisposable
        // ----------------------------------------------------------------
        private bool _disposed;
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            MediaPlayer.Stop();
            MediaPlayer.Dispose();
            _libVlc.Dispose();
        }
    }
}
