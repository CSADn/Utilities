using System.Windows;
using DashMPDPlayer.Interfaces;

namespace DashMPDPlayer.Services;

public class FullscreenManager : IFullscreenManager
{
    private FullscreenState _state;
    public bool IsFullscreen { get; private set; }

    public void Enter(Window window)
    {
        _state = new FullscreenState(window.WindowStyle, window.ResizeMode, window.WindowState);
        window.WindowStyle = WindowStyle.None;
        window.ResizeMode = ResizeMode.NoResize;
        window.WindowState = WindowState.Maximized;
        window.Topmost = true;
        IsFullscreen = true;
    }

    public void Exit(Window window)
    {
        window.WindowStyle = _state.WindowStyle;
        window.ResizeMode = _state.ResizeMode;
        window.WindowState = _state.WindowState;
        window.Topmost = false;
        IsFullscreen = false;
    }
}
