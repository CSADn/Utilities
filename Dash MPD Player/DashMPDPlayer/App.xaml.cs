using System.Windows;
using NLog;

namespace DashMPDPlayer;

public partial class App : Application
{
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    protected override void OnStartup(StartupEventArgs e)
    {
        _logger.Info("=== DASH MPD Player iniciando ===");
        _logger.Info("Args: {Args}", string.Join(" ", e.Args));
        _logger.Info("BaseDirectory: {Dir}", AppDomain.CurrentDomain.BaseDirectory);

        base.OnStartup(e);

        _logger.Info("Aplicación iniciada correctamente");
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _logger.Info("Aplicación finalizando. Exit code: {ExitCode}", e.ApplicationExitCode);
        _logger.Info("=== DASH MPD Player finalizado ===");

        LogManager.Shutdown();
        base.OnExit(e);
    }

    private void App_OnDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
    {
        _logger.Fatal(e.Exception, "Excepción no controlada en el dispatcher UI");
        e.Handled = true;
    }
}
