using System.Windows;
using LibVLCSharp.Shared;

namespace TVPlayer;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        // Initialize LibVLC core before any window is created
        Core.Initialize();
        base.OnStartup(e);
    }
}
