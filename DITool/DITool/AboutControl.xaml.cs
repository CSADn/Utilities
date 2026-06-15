using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DITool
{
    public delegate void FadeInCompletedHandler(object sender);
    public delegate void FadeOutCompletedHandler(object sender);

    public partial class AboutControl : UserControl
    {
        public event FadeInCompletedHandler OnFadeInCompleted;
        public event FadeOutCompletedHandler OnFadeOutCompleted;


        public AboutControl()
        {
            InitializeComponent();

            Loaded += AboutControl_Loaded;
        }


        private void AboutControl_Loaded(object sender, RoutedEventArgs e)
        {
            BuildControls();
            BindControlEvents();
        }

        private void BuildControls()
        {
            SetupVersion();
        }

        private void BindControlEvents()
        {
            dpOverlay.PreviewMouseUp += (s, e) =>
            {
                var isTutorial = (e.Source as Label)?.Name.Equals("lbTutorial") ?? false;

                if (isTutorial)
                {
                    LaunchBrowser();
                    return;
                }

                AboutFadeOut();
            };
        }


        private void SetupVersion()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var version = assembly.GetName().Version;
            var versionText = $"v{version.Major}.{version.Minor}";

            lbTitle.Content = lbTitle.Content.ToString()
                .Replace("{v}", versionText);
        }

        private void LaunchBrowser()
        {
            var url = "https://www.elotrolado.net/hilo_tutorial-clonar-figuras-de-disney-infinity_2311609";
            System.Diagnostics.Process.Start(url);
        }


        public void AboutFadeIn()
        {
            dpOverlay.Opacity = 0;
            dpOverlay.Visibility = Visibility.Visible;

            var animation = new DoubleAnimation
            {
                From = 0.0,
                To = 1.0,
                FillBehavior = FillBehavior.HoldEnd,
                BeginTime = TimeSpan.FromMilliseconds(0),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn },
                Duration = new Duration(TimeSpan.FromMilliseconds(500))
            };
            var storyboard = new Storyboard();

            storyboard.Children.Add(animation);
            Storyboard.SetTarget(animation, dpOverlay);
            Storyboard.SetTargetProperty(animation, new PropertyPath(OpacityProperty));
            storyboard.Completed += (s, e) => OnFadeInCompleted?.Invoke(s);
            storyboard.Begin();
        }

        public void AboutFadeOut()
        {
            dpOverlay.Visibility = Visibility.Visible;
            dpOverlay.Opacity = 1.0;

            var animation = new DoubleAnimation
            {
                From = 1.0,
                To = 0.0,
                FillBehavior = FillBehavior.HoldEnd,
                BeginTime = TimeSpan.FromMilliseconds(0),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut },
                Duration = new Duration(TimeSpan.FromMilliseconds(500))
            };
            var storyboard = new Storyboard();

            storyboard.Children.Add(animation);
            Storyboard.SetTarget(animation, dpOverlay);
            Storyboard.SetTargetProperty(animation, new PropertyPath(OpacityProperty));
            storyboard.Completed += (s, e) =>
            {
                dpOverlay.Visibility = Visibility.Hidden;
                OnFadeOutCompleted?.Invoke(s);
            };
            storyboard.Begin();
        }
    }
}
