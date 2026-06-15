using System.Windows;

namespace DashMPDPlayer.Interfaces;

public readonly record struct FullscreenState(
    WindowStyle WindowStyle,
    ResizeMode ResizeMode,
    WindowState WindowState);

public interface IFullscreenManager
{
    bool IsFullscreen { get; }
    void Enter(Window window);
    void Exit(Window window);
}
