using System.Windows;
using System.Windows.Input;

namespace TVPlayer
{
    public partial class FullScreenWindow : Window
    {
        public FullScreenWindow()
        {
            InitializeComponent();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e.Key is Key.Escape or Key.F11)
            {
                Close(); // MainWindow listens to Closed and restores
                e.Handled = true;
            }
        }
    }
}
