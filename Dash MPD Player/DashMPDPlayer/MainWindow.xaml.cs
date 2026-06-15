using System.IO;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using DashMPDPlayer.Interfaces;
using DashMPDPlayer.Models;
using DashMPDPlayer.Services;
using Microsoft.Web.WebView2.Core;
using NLog;

namespace DashMPDPlayer;

    public partial class MainWindow : Window
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        private const double PanelWidth = 300;

    private static readonly string ConsoleBridgeScript = @"
(function() {
    var nativePostMessage = window.chrome && window.chrome.webview && window.chrome.webview.postMessage;
    if (!nativePostMessage) return;
    var consoleErrorOrig = console.error;
    console.error = function() {
        var msg = Array.prototype.slice.call(arguments).join(' ');
        nativePostMessage(JSON.stringify({ action: 'console', level: 'error', message: msg }));
        consoleErrorOrig.apply(console, arguments);
    };
    var consoleWarnOrig = console.warn;
    console.warn = function() {
        var msg = Array.prototype.slice.call(arguments).join(' ');
        nativePostMessage(JSON.stringify({ action: 'console', level: 'warn', message: msg }));
        consoleWarnOrig.apply(console, arguments);
    };
    var consoleLogOrig = console.log;
    console.log = function() {
        var msg = Array.prototype.slice.call(arguments).join(' ');
        nativePostMessage(JSON.stringify({ action: 'console', level: 'log', message: msg }));
        consoleLogOrig.apply(console, arguments);
    };
})();
";

    private readonly IChannelService _channelService = new ChannelService();
    private readonly IProxyService _proxyService = new ProxyService();
    private readonly ISearchService _searchService = new SearchService();
    private readonly IFullscreenManager _fullscreenManager = new FullscreenManager();
    private List<ChannelGroup> _channelGroups = new();
    private readonly Dictionary<string, BitmapSource> _iconCache = new(StringComparer.OrdinalIgnoreCase);
    private bool _webViewReady;
    private bool _channelsLoaded;
    private bool _isPlaying;
    private bool _isChannelLoaded;

    public MainWindow()
    {
        _logger.Info("Inicializando MainWindow...");

        var webPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Web", "player.html");
        string playerHtml;
        try
        {
            playerHtml = File.ReadAllText(webPath);
            _logger.Debug("Player HTML leído ({Length} chars)", playerHtml.Length);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error leyendo player.html desde {Path}", webPath);
            playerHtml = "<html><body><h2>Error: player.html no encontrado</h2></body></html>";
        }

        _proxyService.SetPlayerHtml(playerHtml);
        _proxyService.OnLog += msg => Dispatcher.Invoke(() => StatusTextBlock.Text = msg);
        InitializeComponent();

        try
        {
            using var stream = typeof(MainWindow).Assembly
                .GetManifestResourceStream("DashMPDPlayer.app.ico");
            if (stream != null)
            {
                using var ms = new MemoryStream();
                stream.CopyTo(ms);
                ms.Position = 0;
                Icon = BitmapFrame.Create(ms, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
            }
        }
        catch (Exception ex)
        {
            _logger.Warn(ex, "No se pudo cargar el icono embebido");
        }

        SourceInitialized += OnSourceInitialized;
        Loaded += OnLoaded;

        _logger.Info("MainWindow inicializada");
    }

    private void OnSourceInitialized(object? sender, EventArgs e)
    {
        var hwnd = new System.Windows.Interop.WindowInteropHelper(this).Handle;
        if (hwnd != IntPtr.Zero)
        {
            var useDarkMode = 1;
            DwmSetWindowAttribute(hwnd, 20, ref useDarkMode, sizeof(int));
            _logger.Info("Dark mode aplicado a la barra de título");
        }
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        _logger.Info("Window Loaded — iniciando proxy y WebView");
        _proxyService.Start();
        await InitWebViewAsync();
        TryLoadDefaultJson();
    }

    private async Task InitWebViewAsync()
    {
        _logger.Info("Inicializando WebView2...");
        try
        {
            var opts = new CoreWebView2EnvironmentOptions
            {
                AdditionalBrowserArguments = "--enable-encrypted-media --autoplay-policy=no-user-gesture-required"
            };
            var env = await CoreWebView2Environment.CreateAsync(null, null, opts);
            await PlayerWebView.EnsureCoreWebView2Async(env);
            _logger.Info("WebView2 inicializado correctamente");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error inicializando WebView2");
            StatusTextBlock.Text = $"Error inicializando WebView2: {ex.Message}";
        }
    }

    private void OnWebViewInit(object sender, CoreWebView2InitializationCompletedEventArgs e)
    {
        if (!e.IsSuccess)
        {
            _logger.Error("WebView2 initialization failed: {Message}", e.InitializationException?.Message);
            StatusTextBlock.Text = $"Error WebView2: {e.InitializationException?.Message}";
            return;
        }

        _webViewReady = true;

        var settings = PlayerWebView.CoreWebView2.Settings;
        settings.AreDefaultScriptDialogsEnabled = true;
        settings.IsScriptEnabled = true;
        settings.IsWebMessageEnabled = true;
        settings.AreHostObjectsAllowed = true;
        settings.IsPasswordAutosaveEnabled = false;
        settings.IsGeneralAutofillEnabled = false;

        PlayerWebView.CoreWebView2.ContainsFullScreenElementChanged += OnFullScreenElementChanged;
        PlayerWebView.CoreWebView2.WebMessageReceived += OnWebMessageReceived;
        _ = PlayerWebView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(ConsoleBridgeScript);

        var playerUrl = $"http://localhost:{_proxyService.Port}/player";
        _logger.Info("Navegando a {Url} para habilitar EME (origen HTTP)", playerUrl);
        PlayerWebView.CoreWebView2.Navigate(playerUrl);

        _logger.Info("WebView2 listo, player.html cargado");
    }

    private void OnWebMessageReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs args)
    {
        var json = args.TryGetWebMessageAsString();
        if (string.IsNullOrEmpty(json)) return;

        _logger.Warn("WebView2 message: {Message}", json);
        try
        {
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("action", out var actionProp))
            {
                var action = actionProp.GetString();
                if (action == "loaded")
                {
                    _logger.Info("WebView lista - canal cargado");
                    _isChannelLoaded = true;
                    _isPlaying = true;
                    UpdateTopmostState();
                }
                else if (action == "playState" &&
                    doc.RootElement.TryGetProperty("playing", out var playingProp))
                {
                    _isPlaying = playingProp.GetBoolean();
                    UpdateTopmostState();
                }
                else if (action == "loadError" || action == "error")
                {
                    _isPlaying = false;
                    UpdateTopmostState();
                }
            }
        }
        catch { }
    }

    private void OnFullScreenElementChanged(object? sender, object e)
    {
        var cwv = PlayerWebView.CoreWebView2;
        if (cwv?.ContainsFullScreenElement == true)
        {
            _logger.Info("Entrando a pantalla completa");
            LeftColumn.MinWidth = 0;
            LeftColumn.Width = new GridLength(0);
            GridSplitter.Visibility = Visibility.Collapsed;
            MenuItemTogglePanel.Visibility = Visibility.Collapsed;
            Menu.Visibility = Visibility.Collapsed;
            StatusBar.Visibility = Visibility.Collapsed;
            _fullscreenManager.Enter(this);
        }
        else
        {
            _logger.Info("Saliendo de pantalla completa");
            _fullscreenManager.Exit(this);
            Menu.Visibility = Visibility.Visible;
            MenuItemTogglePanel.Visibility = Visibility.Visible;
            GridSplitter.Visibility = Visibility.Visible;
            ApplyPanelWidth();
            StatusBar.Visibility = Visibility.Visible;
        }
    }

    private void TryLoadDefaultJson()
    {
        var path = _channelService.FindDefaultJson();
        if (path != null)
        {
            _logger.Info("JSON de canales encontrado: {Path}", path);
            LoadJsonFile(path);
            StatusTextBlock.Text = $"Cargado: {Path.GetFileName(path)}";
        }
        else
        {
            _logger.Warn("No se encontró archivo JSON de canales");
            StatusTextBlock.Text = "No se encontró archivo de canales. Use Archivo > Abrir JSON";
            OnOpenJson(this, new RoutedEventArgs());
        }
    }

    private void LoadJsonFile(string path)
    {
        _logger.Info("Cargando JSON de canales: {Path}", path);
        try
        {
            _channelGroups = _channelService.LoadFromFile(path);
            _channelsLoaded = true;
            RebindTreeView(_channelGroups);
            UpdateChannelCount(_channelGroups);
            StatusTextBlock.Text = $"Cargado: {Path.GetFileName(path)}";
            _logger.Info("JSON cargado: {Groups} grupos, {Channels} canales", _channelGroups.Count, _channelGroups.Sum(g => g.Samples.Count));
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error al cargar JSON: {Path}", path);
            MessageBox.Show($"Error al cargar {path}:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void OnOpenJson(object sender, RoutedEventArgs e)
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Title = "Seleccionar archivo JSON de canales",
            Filter = "Archivos JSON (*.json)|*.json|Todos los archivos (*.*)|*.*",
            DefaultExt = ".json"
        };

        if (dialog.ShowDialog() == true)
        {
            _logger.Info("Usuario seleccionó JSON: {File}", dialog.FileName);
            LoadJsonFile(dialog.FileName);
        }
    }

    private void OnReload(object sender, RoutedEventArgs e)
    {
        _logger.Info("Recargando JSON de canales...");
        TryLoadDefaultJson();
    }

    private void OnExit(object sender, RoutedEventArgs e)
    {
        _logger.Info("Cerrando aplicación por solicitud del usuario");
        Close();
    }

    private void ApplyPanelWidth()
    {
        LeftColumn.Width = MenuItemTogglePanel.IsChecked ? new GridLength(PanelWidth) : new GridLength(0);
    }

    private void OnTogglePanel(object sender, RoutedEventArgs e)
    {
        ApplyPanelWidth();
        _logger.Debug("Panel toggled: {State}", MenuItemTogglePanel.IsChecked ? "visible" : "oculto");
    }

    private void RebindTreeView(List<ChannelGroup> groups)
    {
        ChannelTreeView.ItemsSource = null;
        ChannelTreeView.ItemsSource = groups;
    }

    private void CollapseAllCategories()
    {
        foreach (var group in ChannelTreeView.Items)
        {
            if (ChannelTreeView.ItemContainerGenerator.ContainerFromItem(group) is TreeViewItem item)
            {
                item.IsExpanded = false;
            }
        }
    }

    private void OnChannelTreeViewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Divide)
        {
            CollapseAllCategories();
            e.Handled = true;
        }
    }

    private void UpdateChannelCount(IEnumerable<ChannelGroup> groups)
    {
        var groupList = groups as List<ChannelGroup> ?? groups.ToList();
        ChannelCountText.Text = $"{groupList.Count} grupos, {groupList.Sum(g => g.Samples.Count)} canales";
    }

    private void OnAbout(object sender, RoutedEventArgs e)
    {
        MessageBox.Show(
            "IPTV DASH Player v1.0\n\nReproductor de canales IPTV con soporte DASH + DRM (Clearkey/Widevine).\n\nBasado en .NET 10 WPF + WebView2 + Shaka Player.",
            "Acerca de", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private async void OnChannelSelected(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (e.NewValue is Channel channel)
        {
            _logger.Info("Canal seleccionado: {ChannelName} ({Type})", channel.Name, channel.Type);
            await PlayChannelAsync(channel);
        }
    }

    private void UpdateTopmostState()
    {
        Topmost = MenuItemAlwaysOnTop.IsChecked && _isChannelLoaded && _isPlaying;
    }

    private void OnAlwaysOnTopClick(object sender, RoutedEventArgs e)
    {
        UpdateTopmostState();
        _logger.Debug("Siempre visible: {State}, cargado: {Loaded}, reproduciendo: {Playing}",
            MenuItemAlwaysOnTop.IsChecked, _isChannelLoaded, _isPlaying);
    }

    private async Task PlayChannelAsync(Channel channel)
    {
        if (!_webViewReady)
        {
            _logger.Warn("WebView no está listo, no se puede reproducir {Channel}", channel.Name);
            StatusTextBlock.Text = "WebView no está listo";
            return;
        }

        _isChannelLoaded = false;
        _isPlaying = false;
        UpdateTopmostState();

        _logger.Info("Reproduciendo canal: {Name}, url: {Url}, type: {Type}", channel.Name, channel.Url, channel.Type);
        StatusTextBlock.Text = $"Conectando: {channel.Name}...";
        ChannelStatusBlock.Text = channel.Name;
        WelcomeOverlay.Visibility = Visibility.Collapsed;

        _proxyService.SetChannelConfig(channel.Url, channel.Headers);

        var manifestUrl = $"{_proxyService.ProxyBaseUrl}/manifest?url={Uri.EscapeDataString(channel.Url)}";

        var (keyId, key) = ("", "");
        var clearkey = channel.ParseClearkeyLicense();
        if (clearkey.HasValue)
        {
            keyId = clearkey.Value.keyId;
            key = clearkey.Value.key;
            _logger.Debug("Clearkey configurado: keyId={KeyId}", keyId[..Math.Min(16, keyId.Length)] + "...");
        }

        var drmLicenseUri = channel.DrmLicenseUri;
        var channelType = channel.Type;

        if (string.IsNullOrEmpty(drmLicenseUri) && channel.Url?.EndsWith(".mpd", StringComparison.OrdinalIgnoreCase) == true)
        {
            var detected = await _proxyService.DetectDrmAsync(channel.Url, channel.Headers);
            if (detected != null)
            {
                drmLicenseUri = detected.LicenseUrl;
                if (detected.IsWidevine)
                    channelType = "WIDEVINE";
                else if (detected.IsPlayReady)
                    channelType = "WIDEVINE";
                _logger.Info("License URL auto-detectada desde MPD: {Url}", drmLicenseUri);
            }
        }

        var proxyBase = _proxyService.ProxyBaseUrl;
        var licenseProxyUrl = string.IsNullOrEmpty(drmLicenseUri)
            ? ""
            : $"{proxyBase}/license?url={Uri.EscapeDataString(drmLicenseUri)}";

        var channelData = new
        {
            action = "loadChannel",
            channel = new
            {
                name = channel.Name,
                url = manifestUrl,
                manifestUrl,
                type = channelType,
                drmLicenseUri = drmLicenseUri ?? "",
                licenseProxyUrl,
                keyId,
                key,
                headers = channel.Headers ?? new Dictionary<string, string>(),
                icono = channel.Icono
            }
        };

        var json = JsonSerializer.Serialize(channelData);
        PlayerWebView.CoreWebView2.PostWebMessageAsJson(json);
        _logger.Info("Mensaje loadChannel enviado a WebView para: {Name}", channel.Name);
    }

    private void OnChannelIconLoaded(object sender, RoutedEventArgs e)
    {
        if (sender is not Image img || img.Source != null) return;

        var iconUrl = (img.DataContext as Channel)?.Icono;
        if (string.IsNullOrEmpty(iconUrl)) return;

        try
        {
            if (!_iconCache.TryGetValue(iconUrl, out var cached))
            {
                cached = new BitmapImage(new Uri(iconUrl));
                _iconCache[iconUrl] = cached;
            }
            img.Source = cached;
        }
        catch (Exception ex)
        {
            _logger.Debug(ex, "Error cargando icono de canal");
        }
    }

    private void SearchBox_GotFocus(object sender, RoutedEventArgs e)
    {
        if (SearchBox.Text == "Buscar canales...")
            SearchBox.Text = "";
    }

    private void SearchBox_LostFocus(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(SearchBox.Text))
            SearchBox.Text = "Buscar canales...";
    }

    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        if (!_channelsLoaded) return;

        var raw = SearchBox.Text?.Trim() ?? "";
        if (string.IsNullOrWhiteSpace(raw) || raw == "Buscar canales...")
        {
            RebindTreeView(_channelGroups);
            UpdateChannelCount(_channelGroups);
            return;
        }

        var filtered = _searchService.Filter(_channelGroups, raw);
        RebindTreeView(filtered);
        UpdateChannelCount(filtered);
    }

    protected override void OnClosed(EventArgs e)
    {
        _logger.Info("Cerrando aplicación...");
        _proxyService.Dispose();
        _logger.Info("Aplicación cerrada");
        base.OnClosed(e);
    }
}
