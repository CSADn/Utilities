using LibVLCSharp.WPF;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using TVPlayer.ViewModels;

namespace TVPlayer
{
    public partial class MainWindow : Window
    {
        private MainViewModel VM => (MainViewModel)DataContext;

        // Single VideoView shared between normal and fullscreen modes
        private readonly VideoView _videoView;

        // Fullscreen container window
        private FullScreenWindow? _fsWindow;

        public MainWindow()
        {
            InitializeComponent();

            _videoView = new VideoView
            {
                Background = System.Windows.Media.Brushes.Black,
                MediaPlayer = VM.Player.MediaPlayer
            };

            VideoContainer.Content = _videoView;

            // Restore last playlist once the window is visible
            ContentRendered += async (_, _) => await VM.LoadLastPlaylistAsync();
        }

        // ── Closing ───────────────────────────────────────────────────────

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ExitFullScreen();
            VM.Player.Dispose();
        }

        // ── Seek bar ──────────────────────────────────────────────────────

        private void SeekSlider_DragStarted(object sender, DragStartedEventArgs e)
        {
            VM.Player.SeekBarDragging = true;
        }

        private void SeekSlider_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            VM.Player.SeekBarDragging = false;
            VM.Player.Seek((float)SeekSlider.Value);
        }

        // ── Fullscreen ────────────────────────────────────────────────────

        private void FullScreenButton_Click(object sender, RoutedEventArgs e) =>
            ToggleFullScreen();

        private void ToggleFullScreen()
        {
            if (VM.Player.IsFullScreen) ExitFullScreen();
            else EnterFullScreen();
        }

        private void EnterFullScreen()
        {
            // 1. Detach VideoView from this window's container
            VideoContainer.Content = null;

            // 2. Create the borderless fullscreen window
            _fsWindow = new FullScreenWindow();

            // 3. Attach the same VideoView instance to the new window
            _fsWindow.VideoContainer.Content = _videoView;

            // 4. When the fullscreen window is closed by any means, restore
            _fsWindow.Closed += (_, _) => RestoreFromFullScreen();

            _fsWindow.Show();
            VM.Player.IsFullScreen = true;
        }

        private void ExitFullScreen()
        {
            _fsWindow?.Close(); // triggers RestoreFromFullScreen via Closed event
        }

        private void RestoreFromFullScreen()
        {
            if (_fsWindow is null) return;

            // Detach from fullscreen window before it is garbage-collected
            _fsWindow.VideoContainer.Content = null;
            _fsWindow = null;

            // Return VideoView to the normal container
            VideoContainer.Content = _videoView;

            VM.Player.IsFullScreen = false;
        }

        // ── Keyboard ──────────────────────────────────────────────────────

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (IsTextInputFocused()) return;

            switch (e.Key)
            {
                case Key.F11:
                    ToggleFullScreen();
                    e.Handled = true;
                    break;

                case Key.Escape:
                    if (VM.Player.IsFullScreen) ExitFullScreen();
                    e.Handled = true;
                    break;

                case Key.Space:
                    if (VM.Player.IsPlaying)       VM.Player.PauseCommand.Execute(null);
                    else if (VM.Player.IsPaused)   VM.Player.PlayCommand.Execute(null);
                    e.Handled = true;
                    break;

                case Key.M:
                    VM.Player.ToggleMuteCommand.Execute(null);
                    e.Handled = true;
                    break;

                case Key.Left:
                    VM.Player.SeekBackwardCommand.Execute(null);
                    e.Handled = true;
                    break;

                case Key.Right:
                    VM.Player.SeekForwardCommand.Execute(null);
                    e.Handled = true;
                    break;
            }
        }

        private static bool IsTextInputFocused()
        {
            var focused = Keyboard.FocusedElement;
            return focused is TextBox or RichTextBox
                || (focused is ComboBox cb && cb.IsEditable);
        }
    }
}
