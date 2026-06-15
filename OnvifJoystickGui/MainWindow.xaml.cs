using LibVLCSharp.Shared;
using Microsoft.Extensions.Configuration;
using Microsoft.Win32;
using QuickNV.Onvif;
using QuickNV.Onvif.PTZ;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Color = System.Windows.Media.Color;
using MediaPlayer = LibVLCSharp.Shared.MediaPlayer;

namespace OnvifJoystickGui;

public partial class MainWindow : Window
{
    // 
    // https://github.com/uratmangun/onvif-v380-pro
    //

    private OnvifClient? _client;
    private PTZClient? _PTZClient;
    private string? _profileToken;
    private bool _isConnected = false;
    private bool _toggleUi = false;
    
    // VLC Media Player
    private LibVLC? _libVLC;
    private MediaPlayer? _mediaPlayer;
    private string? _streamUri;
    private bool _streamAutoStart;

    // Onvif Camera
    private string? _onvifHost;
    private int _onvifPort;

    // Panels Visible
    private bool _controlsVisible = true;
    private bool _outputLogVisible = true;

  
    public MainWindow()
    {
        InitializeComponent();

        LoadConfiguration();
        LoadRegistryParameters();

        this.KeyDown += MainWindow_KeyDown;
        this.KeyUp += MainWindow_KeyUp;
        this.Loaded += async (s, e) => { await MainWindow_LoadedAsync(s, e); };
        this.Closing += (s, e) => SaveParameters();
    }


    private async Task MainWindow_LoadedAsync(object sender, RoutedEventArgs e)
    {
        ToggleUI(_toggleUi);

        if (_streamAutoStart)
            await ConnectStream();
    }

    private void LoadConfiguration()
    {
        try
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("AppSettings.json", optional: false, reloadOnChange: false);

            IConfiguration config = builder.Build();

            _onvifHost = config.GetValue<string>("OnvifCamera:Host");
            _onvifPort = config.GetValue<int>("OnvifCamera:Port");

            _streamAutoStart = config.GetValue<bool>("StreamAutoStart");
            _controlsVisible = config.GetValue<bool>("ControlsVisible");
            _outputLogVisible = config.GetValue<bool>("OutputLogVisible");

            _toggleUi = _controlsVisible && _outputLogVisible;

            AddLog($"✓ Configuración cargada desde AppSettings.json");
            AddLog($"  Host: {_onvifHost}:{_onvifPort}");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al cargar appsettings.json:\n{ex.Message}\n\nAsegúrese de que el archivo existe en el directorio de la aplicación.",
                "Error de Configuración", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void LoadRegistryParameters()
    {
        try
        {
            using var registry = RegistryHelper.CurrentUser(@"Software\OnvifJoystickGui");

            // Intentar cargar los valores guardados
            var left = registry.GetValue<double>("WindowLeft", -1);
            var top = registry.GetValue<double>("WindowTop", -1);
            var width = registry.GetValue<double>("WindowWidth", -1);
            var height = registry.GetValue<double>("WindowHeight", -1);
            var streamAutoStart = registry.GetValue<bool>("StreamAutoStart", false);

            // Si no existen valores guardados, usar configuración por defecto
            if (left == -1 || top == -1 || width == -1 || height == -1)
            {
                // Usar valores del diseño (630x460)
                width = 630;
                height = 460;

                // Centrar en el escritorio
                var screenWidth = SystemParameters.PrimaryScreenWidth;
                var screenHeight = SystemParameters.PrimaryScreenHeight;
                left = (screenWidth - width) / 2;
                top = (screenHeight - height) / 2;

                // Guardar la configuración por defecto
                registry.SetValue("WindowLeft", left, RegistryValueKind.String);
                registry.SetValue("WindowTop", top, RegistryValueKind.String);
                registry.SetValue("WindowWidth", width, RegistryValueKind.String);
                registry.SetValue("WindowHeight", height, RegistryValueKind.String);
                registry.SetValue("StreamAutoStart", false, RegistryValueKind.String);
                registry.SetValue("ToggleUI", false, RegistryValueKind.String);
            }

            // Aplicar los valores a la ventana
            this.Left = left;
            this.Top = top;
            this.Width = width;
            this.Height = height;
        }
        catch (Exception ex)
        {
            AddLog($"⚠ Error al cargar posición de ventana: {ex.Message}");
        }
    }

    private void SaveParameters()
    {
        using var registry = RegistryHelper.CurrentUser(@"Software\OnvifJoystickGui");

        registry.SetValue("WindowLeft", this.Left, RegistryValueKind.String);
        registry.SetValue("WindowTop", this.Top, RegistryValueKind.String);
        registry.SetValue("WindowWidth", this.Width, RegistryValueKind.String);
        registry.SetValue("WindowHeight", this.Height, RegistryValueKind.String);
        registry.SetValue("ToggleUI", _toggleUi, RegistryValueKind.String);
        registry.SetValue("StreamAutoStart", _streamAutoStart, RegistryValueKind.String);
    }


    private async void BtnConnect_Click(object sender, RoutedEventArgs e)
    {
        await ConnectStream();
    }

    private async void BtnUp_MouseDown(object sender, MouseButtonEventArgs e)
    {
        await MoveUp();
    }

    private async void BtnDown_MouseDown(object sender, MouseButtonEventArgs e)
    {
        await MoveDown();
    }

    private async void BtnLeft_MouseDown(object sender, MouseButtonEventArgs e)
    {
        await MoveLeft();
    }

    private async void BtnRight_MouseDown(object sender, MouseButtonEventArgs e)
    {
        await MoveRight();
    }

    private async void BtnZoomIn_MouseDown(object sender, MouseButtonEventArgs e)
    {
        await ZoomIn();
    }

    private async void BtnZoomOut_MouseDown(object sender, MouseButtonEventArgs e)
    {
        await ZoomOut();
    }

    private async void Btn_MouseUp(object sender, MouseEventArgs e)
    {
        await StopMovement();
    }

    private async void MainWindow_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            Disconnect();
            Close();
            return;
        }

        if (e.Key == Key.D1)
        {
            ToggleUI(!_toggleUi);
            return;
        }

        if (!_isConnected || e.IsRepeat)
            return;

        switch (e.Key)
        {
            case Key.W:
                await MoveUp();
                break;
            case Key.S:
                await MoveDown();
                break;
            case Key.A:
                await MoveLeft();
                break;
            case Key.D:
                await MoveRight();
                break;
            case Key.Add:
            case Key.OemPlus:
                await ZoomIn();
                break;
            case Key.Subtract:
            case Key.OemMinus:
                await ZoomOut();
                break;
        }
    }

    private async void MainWindow_KeyUp(object sender, KeyEventArgs e)
    {
        if (!_isConnected)
            return;

        switch (e.Key)
        {
            case Key.W:
            case Key.S:
            case Key.A:
            case Key.D:
            case Key.Add:
            case Key.OemPlus:
            case Key.Subtract:
            case Key.OemMinus:
                await StopMovement();
                break;
        }
    }


    private void VideoOverlay_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState == MouseButtonState.Pressed)
        {
            this.DragMove();
        }
    }


    private async Task MoveUp()
    {
        AddLog("↑ Arriba");
        await ContinuousMove(0f, 0.5f, 0f);
    }

    private async Task MoveDown()
    {
        AddLog("↓ Abajo");
        await ContinuousMove(0f, -0.5f, 0f);
    }

    private async Task MoveLeft()
    {
        AddLog("← Izquierda");
        await ContinuousMove(-0.5f, 0f, 0f);
    }

    private async Task MoveRight()
    {
        AddLog("→ Derecha");
        await ContinuousMove(0.5f, 0f, 0f);
    }

    private async Task ZoomIn()
    {
        AddLog("🔍+ Zoom In");
        await ContinuousMove(0f, 0f, 0.5f);
    }

    private async Task ZoomOut()
    {
        AddLog("🔍- Zoom Out");
        await ContinuousMove(0f, 0f, -0.5f);
    }


    private async Task ContinuousMove(float panSpeed, float tiltSpeed, float zoomSpeed)
    {
        if (_PTZClient == null || string.IsNullOrEmpty(_profileToken))
            return;

        try
        {
            await _PTZClient.ContinuousMoveAsync(
                ProfileToken: _profileToken,
                Velocity: new PTZSpeed
                {
                    PanTilt = new Vector2D
                    {
                        x = panSpeed,
                        y = tiltSpeed
                    },
                    Zoom = new Vector1D
                    {
                        x = zoomSpeed
                    }
                },
                Timeout: "PT1S"
            );
        }
        catch (Exception ex)
        {
            AddLog($"✗ Error en movimiento: {ex.Message}");
        }
    }

    private async Task StopMovement()
    {
        if (_PTZClient == null || string.IsNullOrEmpty(_profileToken))
            return;

        try
        {
            await _PTZClient.StopAsync(
                ProfileToken: _profileToken,
                PanTilt: true,
                Zoom: true
            );
        }
        catch (Exception ex)
        {
            AddLog($"✗ Error al detener: {ex.Message}");
        }
    }


    private void Disconnect()
    {
        StopStream();

        _mediaPlayer?.Dispose();
        _mediaPlayer = null;

        _libVLC?.Dispose();
        _libVLC = null;

        _PTZClient?.Close();
        _PTZClient = null;

        _client?.Dispose();
        _client = null;

        _profileToken = null;
        _streamUri = null;
        _isConnected = false;

        UpdateConnectionUI(false);
        AddLog("Desconectado");
    }

    private void UpdateConnectionUI(bool connected)
    {
        btnUp.IsEnabled = connected;
        btnDown.IsEnabled = connected;
        btnLeft.IsEnabled = connected;
        btnRight.IsEnabled = connected;

        if (connected)
        {
            btnConnect.Content = "Desconectar";
            btnConnect.Background = new SolidColorBrush(Color.FromRgb(244, 67, 54));
        }
        else
        {
            btnConnect.Content = "Conectar";
            btnConnect.Background = new SolidColorBrush(Color.FromRgb(76, 175, 80));
            txtStreamStatus.Text = "Video Stream - Desconectado";
            txtStreamStatus.Foreground = new SolidColorBrush(Color.FromRgb(117, 117, 117));
        }
    }

    private void ToggleUI(bool flag = true)
    {
        _toggleUi = flag;

        Height = flag
            ? Height + PTZControls.ActualHeight + OutputLogWindow.ActualHeight
            : Height;

        PTZControls.Visibility = flag
            ? Visibility.Visible
            : Visibility.Collapsed;

        OutputLogWindow.Visibility = flag
            ? Visibility.Visible
            : Visibility.Collapsed;
    }

    private void AddLog(string message)
    {
        Dispatcher.Invoke(() =>
        {
            var timestamp = DateTime.Now.ToString("HH:mm:ss");
            txtLog.Text += $"\n[{timestamp}] {message}";

            // Auto-scroll al final
            if (txtLog.Parent is ScrollViewer scrollViewer)
            {
                scrollViewer.ScrollToEnd();
            }
        });
    }


    private async Task ConnectStream()
    {
        if (_isConnected)
        {
            Disconnect();
            return;
        }

        try
        {
            btnConnect.IsEnabled = false;
            AddLog("Conectando a la cámara...");

            _client = new OnvifClient(new OnvifClientOptions()
            {
                Scheme = "http",
                Host = _onvifHost,
                Port = _onvifPort
            });

            await _client.ConnectAsync();

            AddLog($"✓ Conectado: {_client.DeviceInformation?.Manufacturer} {_client.DeviceInformation?.Model}");

            _PTZClient = new PTZClient(_client);

            var configurations = await _PTZClient.GetConfigurationsAsync();
            if (configurations == null || configurations.PTZConfiguration.Length == 0)
            {
                throw new Exception("La cámara no soporta PTZ");
            }

            var mediaClient = new QuickNV.Onvif.Media.MediaClient(_client);
            var profiles = await mediaClient.GetProfilesAsync();

            if (profiles == null || profiles.Profiles.Length == 0)
            {
                throw new Exception("No se encontraron perfiles de media");
            }

            _profileToken = profiles.Profiles[0].token;
            AddLog($"Perfil PTZ: {profiles.Profiles[0].Name}");

            try
            {
                _streamUri = await mediaClient.QuickOnvif_GetStreamUriAsync(_profileToken, true);

                if (_streamUri != null)
                {
                    AddLog($"Stream URI obtenido: {_streamUri}");
                    StartStream();
                }
                else
                {
                    AddLog("⚠ No se pudo obtener URI del stream");
                }
            }
            catch (Exception ex)
            {
                AddLog($"⚠ Error al obtener stream URI: {ex.Message}");
            }

            _isConnected = true;
            UpdateConnectionUI(true);
            AddLog("✓ Sistema PTZ listo. Use los botones o teclas W, A, S, D");
        }
        catch (Exception ex)
        {
            AddLog($"✗ Error: {ex.Message}");
            MessageBox.Show($"Error al conectar:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            Disconnect();
        }
        finally
        {
            btnConnect.IsEnabled = true;
        }
    }

    private void StartStream()
    {
        if (string.IsNullOrEmpty(_streamUri))
        {
            AddLog("✗ No hay URI de stream disponible");
            return;
        }

        try
        {
            _libVLC = new LibVLC();
            _mediaPlayer = new MediaPlayer(_libVLC);

            videoView.MediaPlayer = _mediaPlayer;

            AddLog("▶ Iniciando stream...");

            var media = new LibVLCSharp.Shared.Media(_libVLC, _streamUri, FromType.FromLocation);

            // Opciones para optimizar RTSP
            media.AddOption(":network-caching=300");
            media.AddOption(":rtsp-tcp");
            media.AddOption(":rtsp-frame-buffer-size=500000");

            _mediaPlayer.Play(media);
            _mediaPlayer.Volume = 0;

            overlayNoVideo.Visibility = Visibility.Collapsed;
            videoView.Visibility = Visibility.Visible;

            txtStreamStatus.Text = "Video Stream - Reproduciendo";
            txtStreamStatus.Foreground = new SolidColorBrush(Color.FromRgb(76, 175, 80));

            AddLog("✓ Stream iniciado correctamente");
        }
        catch (Exception ex)
        {
            AddLog($"✗ Error al iniciar stream: {ex.Message}");
            MessageBox.Show($"Error al iniciar el stream:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void StopStream()
    {
        try
        {
            _mediaPlayer?.Stop();

            videoView.Visibility = Visibility.Collapsed;
            overlayNoVideo.Visibility = Visibility.Visible;

            txtStreamStatus.Text = "Video Stream - Detenido";
            txtStreamStatus.Foreground = new SolidColorBrush(Color.FromRgb(117, 117, 117));

            AddLog("⏹ Stream detenido");
        }
        catch (Exception ex)
        {
            AddLog($"✗ Error al detener stream: {ex.Message}");
        }
    }


    protected override void OnClosed(EventArgs e)
    {
        Disconnect();
        base.OnClosed(e);
    }
}
