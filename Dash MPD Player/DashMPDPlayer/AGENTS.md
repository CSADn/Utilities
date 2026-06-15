# DASH MPD Player — Knowledge Base

## Descripción General

Reproductor IPTV para canales DASH en vivo con DRM (Clearkey/Widevine). Construido en .NET 10 WPF con WebView2 + Shaka Player.

---

## Stack Tecnológico

| Componente    | Tecnología                         | Versión     |
|---------------|------------------------------------|-------------|
| Runtime       | .NET 10 WPF                        | net10.0-windows |
| UI            | WPF (XAML) + WebView2              | —           |
| WebView2      | Microsoft.Web.WebView2             | 1.0.3967.48 |
| DASH Player   | Shaka Player (JS, CDN)             | 4.x         |
| HLS Player    | hls.js (JS, CDN)                   | latest      |
| Serialización | System.Text.Json (built-in)        | —           |
| HTTP Server   | HttpListener (built-in)            | —           |
| HTTP Client   | HttpClient + SocketsHttpHandler    | —           |
| Logging       | NLog                               | 6.x         |

---

## Estructura del Proyecto

```
DashMPDPlayer/
├── DashMPDPlayer.csproj
├── AGENTS.md                       ← This file
├── app.ico                        ← Icono de la aplicación (television.png convertido)
├── nlog.config                    # Configuración de logging (NLog)
├── App.xaml / App.xaml.cs
├── MainWindow.xaml / MainWindow.xaml.cs
├── Interfaces/
│   ├── IChannelService.cs          # Contrato carga de canales
│   ├── IFullscreenManager.cs       # Contrato fullscreen
│   ├── IProxyService.cs            # Contrato proxy local
│   └── ISearchService.cs           # Contrato búsqueda
├── Models/
│   ├── Channel.cs                  # Modelo de canal (limpiado)
│   └── ChannelGroup.cs             # Modelo de grupo de canales (limpiado)
├── Services/
│   ├── ChannelService.cs           # Carga JSON de canales
│   ├── FullscreenManager.cs        # Fullscreen (WindowStyle, ResizeMode, etc.)
│   ├── MpdRewriter.cs              # Rewrite XML/Regex de URLs en MPD
│   ├── ProxyService.cs             # Proxy HTTP local (~300 líneas)
│   ├── SearchService.cs            # Filtro accent-insensitive en TreeView
│   └── TokenService.cs             # Bearer token + CDN token management
└── Web/
    └── player.html                 # Shaka Player embebido + bridge WebView2
```

---

## Arquitectura: Proxy Local + Shaka Player

### Flujo de Reproducción

```
Usuario selecciona canal en TreeView
         │
         ▼
MainWindow.xaml.cs — OnChannelSelected()
  → ProxyService.SetChannelConfig(mpdUrl, headers)
  → PostWebMessageAsJson({ action: "loadChannel", channel: {...} })
         │
         ▼
WebView2 (player.html)
  → window.chrome.webview.addEventListener('message', ...)
  → loadChannel(channel)
    → Configura DRM (Clearkey: keyId+key, o Widevine: licenseUri)
    → player.load(manifestUrl)  ← apunta al proxy local
         │
         ▼
Shaka Player solicita MPD a:
  http://localhost:{port}/manifest?url=<original_mpd_url>
         │
         ▼
ProxyService.HandleManifestAsync()
  │
  ├─ Si el canal tiene headers (Flow, requiere firma):
  │   1. TokenService.GetBearerTokenAsync() → GET https://app.femon.net/pirata/piratacodigo.json
  │   2. TokenService.RequestCdnTokenAsync() → GET https://cdn-token.app.flow.com.ar/cdntoken/v2/generator...
  │   3. Recibe {"token":"eyJ..."}, extrae el token JWT
  │   4. Construye URL firmada: mpdUrl?cdntoken=<token>
  │   5. GET a URL firmada (sigue redirect → edge CDN)
  │   6. Obtiene MPD con URLs de segmentos tokenizadas
  │   7. MpdRewriter.Rewrite() — reescribe URLs (XML XDocument + fallback Regex) → proxy
  │
  └─ Si el canal no tiene headers (no-Flow):
      1. GET directo a mpdUrl
      2. MpdRewriter.Rewrite() — reescribe URLs → proxy
         │
         ▼
Shaka parsea MPD, solicita segmentos a:
  http://localhost:{port}/segment/{token}/{filename}
         │
         ▼
ProxyService.HandleSegmentAsync()
  → Busca SessionEntry.BaseUrl por token
  → Construye URL CDN completa
  → Agrega headers (Origin, Referer, User-Agent)
  → Forwardea a CDN con _httpClientSegments (timeout 120s)
  → Si CDN devuelve 4xx/5xx, propaga el status code original al cliente
  → Retorna segmento a Shaka

Shaka solicita licencia a:
  http://localhost:{port}/license?url=<license_server>
         │
         ▼
ProxyService.HandleLicenseAsync()
  → Lee body binario (Widevine challenge)
  → Construye POST request al license server con headers (Origin, Referer, etc.)
  → Forwardea con _httpClient (timeout 30s)
  → Retorna respuesta (licencia) a Shaka
```

### Proxy Local — Endpoints HTTP

| Ruta      | Método | Función                                  |
|-----------|--------|------------------------------------------|
| `/manifest?url=<mpd>` | GET | Obtiene MPD vía CDN token generator, reescribe URLs |
| `/segment/{token}/{path}` | GET | Proxy inverso para segmentos DASH |
| `/license?url=<license_server>` | POST | Proxy inverso para requests de licencia DRM |
| `/player` | GET | Sirve player.html |

### MPD Rewriting — `MpdRewriter` Service

El `MpdRewriter` se encarga exclusivamente de reescribir URLs de segmentos en el MPD. Se inyecta en `ProxyService` con `ProxyBaseUrl` como parámetro de constructor.

**Estrategia de rewrite (2 pasos):**
1. **XML parsing** (`XDocument`): Busca `<SegmentTemplate media="...">`, `<SegmentTemplate initialization="...">`, y `<BaseURL>...</BaseURL>` en el namespace `urn:mpeg:dash:schema:mpd:2011`. Reescribe las URLs apuntando al proxy.
2. **Fallback Regex**: Si el XML parsing falla (malformed XML o namespace diferente), usa Regex como respaldo.

`GetFileNameFromUrl()` extrae solo el nombre del archivo de URLs absolutas; las relativas se usan tal cual.

**DRM Detection** (`DetectDrm()`): Analiza el MPD en busca de `<ContentProtection>` y extrae la license URL. Soporta:
- `<dashif:laurl>` — URL directa de licencia
- `<ms:laurl>` — URL de licencia Microsoft
- `<mspr:pro>` — PlayReady Object binario (estructura: 4B size + 2B count + records de 2B type + 2B length + XML UTF-16-LE). Busca `<LA_URL>` en el XML embebido.
- También busca elemento `pro` sin namespace (fallback para MPDs con namespace inline).

**Ejemplo:**
- Original: `media="DSports_1-$RepresentationID$-scale=48000-p=359352450761000-$Time$.mp4"`
- Reescribe: `media="http://localhost:5588/segment/3/DSports_1-$RepresentationID$-scale=48000-p=359352450761000-$Time$.mp4"`
- Shaka expande placeholders y solicita: `http://localhost:5588/segment/3/DSports_1-avc1_379968=10010-scale=48000-p=359352450761000-17310684974959.mp4`

### DRM — Auto-detección desde MPD + Clearkey / Widevine

**DRM Auto-detección** (`MpdRewriter.DetectDrm()`):
- Canales con `drm_license_uri` vacío y URL `.mpd` disparan detección automática.
- Busca `<ContentProtection>` en namespace `urn:mpeg:dash:schema:mpd:2011`.
- Reconoce UUIDs: `edef8ba9-79d6-4ace-a3c8-27dcd51d21ed` (Widevine), `9a04f079-9840-4286-ab92-e65be0885f95` (PlayReady), `e2719d58-a985-b3c9-781a-b030af78d30e` (ClearKey).
- Extrae license URL de:
  - `<dashif:laurl>` (namespace `urn:dashif:org:mpd:2014`)
  - `<ms:laurl>` (namespace `urn:microsoft:playready`)
  - `<mspr:pro>` binario (PlayReady Object): parsea estructura PRO (4B size + 2B count + records: 2B type + 2B length + XML UTF-16-LE). Busca `<LA_URL>` en el XML.
- Si detecta DRM, se usa la license URL encontrada y el tipo se fuerza a `"WIDEVINE"`.
- La license request se rutea a través del proxy local (`/license?url=...`) para evitar CORS y permitir inyección de headers.

**Clearkey:** El `drm_license_uri` del JSON contiene keyId y key como query params:
```
https://results.femon.net/?keyid=4db80473-af2e-d318-c41d-879f2822203e&key=d30f94e60344adf0f4b242a5952575ad
```
Se parsean en `Channel.ParseClearkeyLicense()` y se pasan a Shaka como:
```javascript
player.configure({ drm: { clearKeys: { '4db80473af2ed318c41d879f2822203e': 'd30f94e60344adf0f4b242a5952575ad' } } });
```
Los guiones del keyId se eliminan en el JS (`channel.keyId.replace(/-/g, '')`).

**Widevine:** Se usa la URI directamente:
```javascript
player.configure({ drm: { servers: { 'com.widevine.alpha': channel.drmLicenseUri } } });
```

---

## Formato JSON de Canales

```json
[
  {
    "name": "📺 DEPORTES",
    "samples": [
      {
        "name": "Dsports 1 🇦🇷",
        "url": "https://chromecast.cvattv.com.ar/live/c3eds/DSports_1/SA_Live_dash_enc/DSports_1.mpd",
        "type": "CLEARKEY",
        "drm_license_uri": "https://results.femon.net/?keyid=...&key=...",
        "icono": "https://...png",
        "headers": {
          "Origin": "https://portal.app.flow.com.py",
          "Referer": "https://portal.app.flow.com.py/",
          "User-Agent": "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 ..."
        }
      }
    ]
  }
]
```

Campos clave:
- `type`: `"CLEARKEY"` o `"WIDEVINE"` o `"HLS"`
- `drm_license_uri`: URL con keyid+key (Clearkey) o servidor de licencias (Widevine)
- `headers`: HTTP headers para agregar a requests de segmentos
- `icono`: URL del logo del canal

---

## Mecanismos Clave

### CDN Token Generator (Flow Signing) — `TokenService`

`TokenService` maneja toda la gestión de tokens (bearer + CDN). Se inyecta en `ProxyService` con el `HttpClient` para manifest/tokens como parámetro.

**Bearer token:** Se cachea en `_bearerToken` (en `TokenService`) con expiración de 1h (`_bearerExpiry`). `InvalidateToken()` lo limpia forzando renovación (usado en retry on 401).

**CDN token:** `RequestCdnTokenAsync()` obtiene un token JWT del CDN generator.

Solo aplica a canales Flow (los que tienen `headers != null` en el JSON). El proceso completo:

1. **Bearer Token**: `TokenService.GetBearerTokenAsync()` → `GET https://app.femon.net/pirata/piratacodigo.json` → devuelve JSON con `token` o `access_token`. Se cachea en `TokenService` con expiración de 1h para reutilizar entre requests.

2. **Solicitar token CDN**: `TokenService.RequestCdnTokenAsync(mpdUrl, bearerToken, headers)`
   ```
   GET https://cdn-token.app.flow.com.ar/cdntoken/v2/generator?path={urlencode(mpdUrl)}
   Headers: Authorization: Bearer <token>, Origin, Referer, User-Agent (del JSON)
   ```
   Respuesta: `{"token":"eyJhbGciOiJIUzUxMiIsInR5cCI6IkpXVCJ9..."}`

3. **Construir URL firmada**: `mpdUrl + "?cdntoken=" + token`

4. **Obtener MPD**: GET a la URL firmada → el CDN responde con redirect (302) a la edge CDN (ej: `edge-live13-hr.cvattv.com.ar/tok_.../...mpd`). El `HttpClient` con `AllowAutoRedirect=true` sigue automáticamente. Si el MPD request retorna 401, `SendManifestRequestAsync()` invalida el bearer token via `TokenService.InvalidateToken()` y reintenta automáticamente con uno nuevo.

5. **Base URL para segmentos**: Se usa la URL final resuelta (edge) como base, no la URL original del MPD. Esto asegura que los segmentos se sirvan desde el edge con el token.

### Dos HttpClient

| Propósito | Timeout | AutoRedirect | PooledConnectionLifetime |
|-----------|---------|--------------|--------------------------|
| Manifest + token CDN (`_httpClient`) | 30s | `true` | 30s |
| Segmentos (`_httpClientSegments`) | 120s | `false` | 60s |

Timeout diferenciado evita que segmentos de video grandes (que pueden tomar más de 30s en redes lentas) rompan la reproducción. `AllowAutoRedirect=false` en segmentos porque ya usan URL resuelta del edge. Ambos comparten `CookieContainer` para preservar cookies del token a través de requests.

### Detección de Canales Flow

La heurística es simple: si el campo `headers` del JSON del canal no es `null`, se considera un canal Flow que requiere firma CDN. Si es `null`, se obtiene el MPD directamente sin firmar.

**Canales Flow** (con headers, 19 en el ejemplo):
- `headers: { Origin: "https://portal.app.flow.com.py", Referer: "...", User-Agent: "..." }`
- Requieren: bearer token + CDN token generator + URL firmada

**Canales no-Flow** (sin headers, ej: Dsports +, Fox Deportes):
- `headers: null`
- Obtienen MPD directamente sin firma

### onRequestFilter — Routing de Requests en Shaka (solo DASH)

`player.html` registra un `requestFilter` en Shaka's `NetworkingEngine` que intercepta todos los requests HTTP. **Solo aplica a canales DASH** (los canales HLS usan hls.js, que no usa Shaka):

- `MANIFEST`: Reescribe URI a `manifestUrl` (pasa por proxy local)
- `LICENSE`: Usa `licenseProxyUrl` si está disponible (rutea a `localhost:{port}/license?url=...`), o `drmLicenseUri` directo como fallback
- Otros tipos (segmentos): Inyecta headers del canal (`Origin`, `Referer`, `User-Agent`) si existen
- Para canales sin headers, LICENSE requests igual se rutean por el proxy si `licenseProxyUrl` está presente

### hls.js — Reproducción de canales HLS

Los canales con `type: "HLS"` **no usan Shaka Player**. En su lugar, `player.html` usa **hls.js** cargado desde CDN (`https://cdn.jsdelivr.net/npm/hls.js@latest`).

**Flujo HLS:**

```
channel.type === 'HLS'
  → hls.js (directo al servidor de origen)
  → hls.loadSource(channel.url)  ← URL original m3u8, NO pasa por proxy
  → hls.attachMedia(video)
  → hls.js descarga playlists y segmentos directamente
```

**Razón:** Shaka Player v4.x usa mux.js para transmuxar TS→fMP4. Ciertos streams TS crashean WebView2 (out of memory en proceso GPU) cuando Shaka los procesa. hls.js tiene su propio transmuxer que maneja estos streams correctamente.

**Limitaciones actuales:**
- Canales HLS con **headers personalizados** (Origin/Referer para Flow, ~7 canales) no reciben inyección de headers con hls.js. Si se necesitan, se debe configurar `loader` o `xhrSetup` en hls.js.
- Canales HLS con **DRM (Widevine)** no están soportados por hls.js. Para esos casos se necesitaría proxy + Shaka o alternativa.

**Endpoint `/fetch` en ProxyService.cs:** Implementado como proxy genérico para cualquier URL, usado cuando se necesita pasar headers o CORS. Actualmente no se usa para HLS (los segmentos van directo), pero está disponible para canales que requieran autenticación.

### WebView2 Bridge (C# ↔ JS)

**C# → JS:** `CoreWebView2.PostWebMessageAsJson(json)`
- Envía objeto `{ action: "loadChannel", channel: { name, url, manifestUrl, type, drmLicenseUri, licenseProxyUrl, keyId, key, headers, icono } }`
- JS recibe vía `window.chrome.webview.addEventListener('message', ...)` (con fallback a `window.addEventListener`)

**JS → C#:** Script injection bridge (workaround para WebView2 SDK 1.0.3967.48, que no expone `CoreWebView2.WebMessageReceived` ni `ConsoleMessage`).
- Se inyecta vía `AddScriptToExecuteOnDocumentCreatedAsync()` un script que intercepta `console.log`, `console.warn`, `console.error` y los reenvía como `postMessage`:
  ```javascript
  window.chrome.webview.postMessage(JSON.stringify({ action: 'console', level: 'log', message: msg }));
  ```
- C# recibe en `WebView2.WebMessageReceived` y parsea el JSON para logging.
- Los mensajes se loguean como `WARN` con prefijo `WebView2 message:`.
- Mensajes específicos (`{ action: "loaded" }`, `{ action: "init", mse, eme }`, `{ action: "timeout" }`, `{ action: "loadError" }`, `{ action: "error" }`) se interpretan en C# para tracking de estado.

### Headers Injection

Los headers del JSON (Origin, Referer, User-Agent) se usan en tres lugares:

1. **CDN Token Generator**: Se envían como headers de la request para obtener el token (`Authorization: Bearer` se agrega aparte)
2. **Segmentos DASH**: Se agregan a cada request de segmento que el proxy hace a la CDN (`AddHeadersToRequest`)
3. **License Proxy** (`/license`): Se agregan al forward de la request de licencia, más Origin/Referer fijos (`https://www.amazon.com`) para servidores que los requieran

Nota: Los headers en el JSON actual usan dominio `.com.py` (ej: `portal.app.flow.com.py`). El CDN token generator puede requerir dominio `.com.ar` según la configuración de Firebase Remote Config, pero actualmente se usan los mismos headers del JSON para ambos casos. Si hay un mismatch de dominio, se debe ajustar la configuración en el JSON o agregar headers específicos para el CDN generator.

### Manejo de Live Streams

- `minimumUpdatePeriod="PT2S"` → Shaka re-fetch el MPD cada 2s
- Cada re-fetch pasa por el CDN token generator (obtiene nuevas URLs tokenizadas)
- Los segmentos en buffer continúan reproduciéndose sin problema

### Inicio y Carga de Canales

1. App inicia → constructor de MainWindow:
   - Lee `player.html` del disco
   - Configura `ProxyService` con el HTML
   - Inicia el proxy
   - Inicializa WebView2
2. `OnLoaded` → llama proxy.Start(), init WebView
3. `TryLoadDefaultJson()`:
   - Busca `canales_descifrados.json` en BaseDirectory, `samples/`, o path de desarrollo
   - Si no encuentra, muestra diálogo "Abrir JSON"
   - Si encuentra, carga grupos y canales en TreeView
4. Menú "Archivo → Abrir JSON" para carga manual

### Búsqueda de Puerto

`GetAvailablePort()`: bindea socket a puerto 0 → obtiene puerto disponible → cierra socket. Evita conflictos.

### Interfaces y Patrón de Servicios

El proyecto usa interfaces para desacoplar servicios, aunque sin DI container. Las implementaciones se instancian directamente con `new()` pero los campos usan el tipo de interfaz:

```csharp
private readonly IChannelService _channelService = new ChannelService();
private readonly IProxyService _proxyService = new ProxyService();
private readonly ISearchService _searchService = new SearchService();
private readonly IFullscreenManager _fullscreenManager = new FullscreenManager();
```

Esto facilita migrar a inyección de dependencias en el futuro y hace el código más testable. Las interfaces están en `Interfaces/`, las implementaciones en `Services/`.

### Servicios Extraídos de ProxyService

`ProxyService` se redujo de ~600 a ~300 líneas extrayendo:

| Servicio | Responsabilidad | Líneas aprox. |
|----------|----------------|---------------|
| `TokenService` | Bearer token + CDN token, caché con expiración, invalidación | ~80 |
| `MpdRewriter` | Rewrite XML (XDocument) + fallback Regex de URLs en MPD | ~60 |

`TokenService` recibe el `HttpClient` de manifest en el constructor. `MpdRewriter` recibe `ProxyBaseUrl`. Ambos se crean en el constructor de `ProxyService`.

### ChannelService.FindDefaultJson

Acepta un parámetro opcional `IEnumerable<string>? additionalPaths` para rutas adicionales de búsqueda del JSON de canales. Las rutas por defecto (`BaseDirectory` + `samples/`) se mantienen.

### EME Rooting

**Problema:** WebView2 con `NavigateToString()` usa origen `about:blank`, que no soporta EME (`requestMediaKeySystemAccess` no disponible). Shaka devuelve `EME support: false`, imposibilitando reproducción con DRM.

**Solución:** Cambiar a `Navigate("http://localhost:{port}/player")` en vez de `NavigateToString()`. Esto le da al WebView2 un origen HTTP válido (`localhost`) que sí soporta EME.

- `CoreWebView2.NavigateToString(html)` rechazado en favor de `CoreWebView2.Navigate(url)`.
- El proxy sirve `player.html` en la ruta `/player`.

### Autoplay Policy Bypass

**Problema original:** Política de Chromium bloquea unmuted autoplay sin gesto de usuario.

**Solución:** Flag `--autoplay-policy=no-user-gesture-required` en WebView2:

```csharp
var opts = new CoreWebView2EnvironmentOptions
{
    AdditionalBrowserArguments = "--enable-encrypted-media --autoplay-policy=no-user-gesture-required"
};
```

Esto desactiva completamente la política de autoplay. Después de `player.load()`, el video se desmutea directamente (`video.muted = false`) sin necesidad de interacción del usuario.

**Historial:** Intentos previos con `SendInput` (mouse/keyboard injection desde C#) fracasaron porque Chromium no reconoce eventos sintéticos como "user activation". También se implementó un sistema de click-to-unmute con overlay "Audio silenciado" que fue reemplazado al aplicar este flag.

**WebView2 flags activos (MainWindow.xaml.cs:63):**
```
--enable-encrypted-media --autoplay-policy=no-user-gesture-required
```

### Controles de Video (player.html)

**Estructura:** Barra inferior (`#controls`) con botones Play/Pause, Mute, Fullscreen, y nombre del canal centrado. Se superpone al video con `position:absolute; bottom:0`.

**Layout:**
- `.left` — grupo izquierdo con botones Play/Pause y Mute
- `#channel-name` — absolutamente centrado con `position:absolute; left:50%; transform:translateX(-50%)`
- `.right` — grupo derecho con botón Fullscreen, alineado a la derecha via `margin-left:auto`

**Visibilidad (CSS, sin inline styles):**
- `#controls { opacity: 0 }` — oculto por defecto
- `#controls.show { opacity: 1 }` — visible cuando el mouse está sobre el video (JS agrega/quita `.show`)
- `#controls.visible { opacity: 1 }` — visible al hacer clic en el video (toggle)
- `#controls:hover { opacity: 1 }` — visible cuando el mouse está sobre la barra

**Auto-hide timer:** 3 segundos después del último `mousemove` sobre el video se remueve `.show`. Si el mouse está sobre la barra, `:hover` la mantiene visible.

**Problema original (fix):** JS usaba `element.style.opacity = '0'` inline, que tiene mayor especificidad que cualquier selector CSS. `#controls:hover` no podía override. Solución: usar solo clases CSS (`.show`, `.visible`) y dejar que el cascade de CSS maneje la prioridad.

### Caché de Iconos de Canales

**Problema:** `RebindTreeView()` recrea los items visuales del TreeView, lo que dispara `Image.Loaded` para cada canal y re-descarga el icono vía HTTP.

**Solución:** `_iconCache: Dictionary<string, BitmapSource>` en `MainWindow`. Al cargar un icono, se consulta el cache primero:
```csharp
if (!_iconCache.TryGetValue(iconUrl, out var cached))
{
    cached = new BitmapImage(new Uri(iconUrl));
    _iconCache[iconUrl] = cached;
}
img.Source = cached;
```
El diccionario usa `StringComparer.OrdinalIgnoreCase` y nunca se limpia (los iconos son estáticos).

### Búsqueda en el TreeView (`SearchBox`) — `SearchService`

**Problema original:** `TextChanged` se dispara durante `InitializeComponent()` antes de que el TreeView exista → NullReferenceException.

**Solución:** Flag `_channelsLoaded` que solo se activa dentro de `LoadJsonFile` (después de que `ChannelTreeView` ya fue creado y los datos cargados). El handler retorna inmediatamente si `!_channelsLoaded`.

La lógica de filtrado está encapsulada en `SearchService` (implementa `ISearchService`), inyectado via field en `MainWindow`.

**Implementación (`MainWindow.xaml.cs`):**
```csharp
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
```

**Filtro accent-insensitive:** Helper `RemoveDiacritics()` que usa `String.Normalize(NormalizationForm.FormD)` para separar caracteres base de sus marcas diacríticas (acentos, diéresis, etc.), filtra las marcas, y recompone con `FormC`. Así "canál" coincide con "canal".

**Comportamiento:**
- Escribir texto filtra en tiempo real: solo canales cuyo nombre contenga el texto (case-insensitive, accent-insensitive).
- Categorías sin canales coincidentes se ocultan.
- Al limpiar el campo (o cuando tiene placeholder), se restaura la lista completa.
- `ChannelCountText` se actualiza con el conteo filtrado.

**Flags de inicialización (`MainWindow.xaml.cs`):**
- `_channelsLoaded` — `true` solo después de cargar JSON exitosamente en `LoadJsonFile`.
- `_webViewReady` — `true` solo después de `CoreWebView2.Initialized`.

### Toggle Panel de Canales (Checkbox "Mostrar panel de canales")

El menú `Ver → Mostrar panel de canales` es un `<MenuItem IsCheckable="True">` que oculta/muestra el panel izquierdo (`LeftColumn`).

**Implementación:**
- `MainWindow.xaml:218` — `ColumnDefinition` con `MinWidth="0"` (crítico: sin esto el panel no colapsa por completo aunque `Width=0`)
- `MainWindow.xaml.cs:246-249` — `ApplyPanelWidth()`: `LeftColumn.Width = MenuItemTogglePanel.IsChecked ? new GridLength(PanelWidth) : new GridLength(0)`
- `MainWindow.xaml:209` — `IsChecked="True"` por defecto (panel visible al inicio)

**Checkmark visual en el MenuItem:**
El `ControlTemplate` `MenuSubmenuItem` tiene un `<Border x:Name="check">` que se muestra cuando `IsCheckable="True"`. Dentro tiene un `<TextBlock x:Name="checkMark" Text="✓">` que se muestra solo cuando `IsChecked="True"`:

```xml
<Border x:Name="check" Width="16" Height="16" Margin="2,0,6,0" 
        BorderBrush="#888" BorderThickness="1" Background="Transparent">
    <TextBlock x:Name="checkMark" Text="✓" Foreground="#e0e0e0"
               HorizontalAlignment="Center" VerticalAlignment="Center"
               FontSize="12" Visibility="Collapsed"/>
</Border>
```

Triggers:
- `IsCheckable="True"` → `check.Visibility = Visible`, `icon.Visibility = Collapsed`
- `IsChecked="True"` → `checkMark.Visibility = Visible`

---

### Dark Mode en Barra de Título (Windows 10 20H1+)

La barra de título del sistema se cambia a modo oscuro (fondo gris oscuro, texto blanco) vía P/Invoke a `dwmapi.dll`:

```csharp
[DllImport("dwmapi.dll")]
private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);
```

**Llamada en evento `SourceInitialized`** (no en constructor ni en `Loaded`):

```csharp
SourceInitialized += OnSourceInitialized;

private void OnSourceInitialized(object? sender, EventArgs e)
{
    var hwnd = new System.Windows.Interop.WindowInteropHelper(this).Handle;
    if (hwnd != IntPtr.Zero)
    {
        var useDarkMode = 1;
        DwmSetWindowAttribute(hwnd, 20, ref useDarkMode, sizeof(int));
        // 20 = DWMWA_USE_IMMERSIVE_DARK_MODE
    }
}
```

**Requerimientos:**
- Windows 10 20H1+ (build 19041+) o Windows 11
- El atributo `20` (DWMWA_USE_IMMERSIVE_DARK_MODE) funciona en versiones modernas
- `SourceInitialized` es el momento correcto porque el HWND ya existe pero la ventana aún no se pintó

---

### Ícono de la Aplicación (EmbeddedResource)

El icono de la app (`.ico`) se genera desde `television.png` con PowerShell usando `System.Drawing.Bitmap` para redimensionar a 32×32 y 16×16 y embeber en formato ICO 32bpp BGRA.

**Build action correcto en SDK-style WPF projects:**

| Build Action | Resultado |
|---|---|
| `<Resource>` | ❌ No embeve el .ico — solo BAML se embeben como recursos WPF |
| `<EmbeddedResource>` | ✅ Sí embeve como `AssemblyName.filename.ico`, accesible via `GetManifestResourceStream()` |
| `<ApplicationIcon>` | ✅ Para el icono del .exe (taskbar/explorer) — funciona independientemente del build action |

```xml
<PropertyGroup>
    <ApplicationIcon>app.ico</ApplicationIcon>
</PropertyGroup>
<ItemGroup>
    <EmbeddedResource Include="app.ico" />
</ItemGroup>
```

**Carga programática desde recurso embebido (no usar `Icon="app.ico"` en XAML):**

```csharp
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
```

**Crítico:** `BitmapCacheOption.OnLoad` + `MemoryStream` — el stream del recurso se descarta después de cargar; sin `OnLoad` el `BitmapFrame` queda referenciando un stream cerrado y no se renderiza.

**Error típico si el icono falla:**
- `Icon="app.ico"` en XAML sin recurso embebido → `IOException: No se encuentra el recurso 'app.ico'`
- La excepción es capturada por `DispatcherUnhandledException` con `e.Handled = true` en `App.xaml.cs:34`
- **La app sigue corriendo pero sin ventana visible** — el proceso aparece en el administrador de tareas pero no hay ventana
- Síntoma: app inicia, proceso visible, ventana nunca aparece, sin error visible al usuario
- Solución: verificar logs FATAL en la salida de la app

**Generación del .ico desde PNG con PowerShell:**
```powershell
Add-Type -AssemblyName System.Drawing
$bmp = [System.Drawing.Bitmap]::new("input.png")
$resized = New-Object System.Drawing.Bitmap(32, 32)
$g = [System.Drawing.Graphics]::FromImage($resized)
$g.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
$g.DrawImage($bmp, 0, 0, 32, 32)
# LockBits → pixel bytes (BGRA) → flip vertical (LockBits es bottom-up, ICO espera top-down)
# BITMAPINFOHEADER con biHeight = 2 * height (pixels + AND mask)
# AND mask = all zeros para 32bpp
# Empaquetar en formato ICO (header + directory entries + image data)
```

---

### "Siempre visible durante transmisión" (Topmost condicional)

El menú `Ver → Siempre visible durante transmisión` controla `Window.Topmost` basado en el estado de reproducción.

**Lógica (`MainWindow.xaml.cs`):**

```csharp
private void UpdateTopmostState()
{
    Topmost = MenuItemAlwaysOnTop.IsChecked && _isChannelLoaded && _isPlaying;
}
```

| IsChecked | Canal cargado | Reproduciendo | Topmost |
|---|---|---|---|
| ✔ | ❌ | — | false |
| ✔ | ✔ | ❌ (pausado/error) | false |
| ✔ | ✔ | ✔ | **true** |
| ✖ | cualquiera | cualquiera | false |

**Estados:**
- `_isChannelLoaded = false`, `_isPlaying = false` → al seleccionar nuevo canal (resetea en `PlayChannelAsync`)
- `_isChannelLoaded = true`, `_isPlaying = true` → cuando JS envía `{ action: "loaded" }`
- `_isPlaying = false` → cuando JS envía `{ action: "playState", playing: false }` (toggle play/pause)
- `_isPlaying = false` → cuando JS envía `{ action: "loadError" }` o `{ action: "error" }`

**JS → C# playState:**
```javascript
// En togglePlay() de player.html:
window.chrome.webview.postMessage(JSON.stringify({ action: 'playState', playing: willPlay }));
```

**Archivos involucrados:**
- `MainWindow.xaml:215-218` — declaración del MenuItem
- `MainWindow.xaml.cs` — `_isPlaying`, `_isChannelLoaded`, `UpdateTopmostState()`, `OnAlwaysOnTopClick()`, casos en `OnWebMessageReceived()`, reset en `PlayChannelAsync()`
- `Web/player.html:269` — `postMessage` en `togglePlay()`

---

### Colapso rápido de categorías (tecla "/" del keypad)

Atajo de teclado para colapsar todas las categorías del TreeView con un solo golpe.

**Implementación (`MainWindow.xaml.cs`):**
```csharp
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
```

**Problema resuelto:** Inicialmente `VirtualizingStackPanel.IsVirtualizing="True"` impedía que `ContainerFromItem()` retornara `TreeViewItem` para grupos fuera del viewport; solo se colapsaban los visibles y requería múltiples presiones + scroll. Se eliminó la virtualización — con ~12 grupos/191 canales el rendimiento no se ve afectado y `ContainerFromItem()` siempre encuentra todos los containers.

**Archivos modificados:**
- `MainWindow.xaml:251` — `KeyDown="OnChannelTreeViewKeyDown"` en el `<TreeView>`
- `MainWindow.xaml.cs:262-277` — `CollapseAllCategories()` + `OnChannelTreeViewKeyDown`

### Fullscreen — `FullscreenManager`

**Implementación híbrida (HTML Fullscreen API + WPF):**

1. Botón ⛶ en controles HTML → `video.requestFullscreen()` (fullscreenchange event)
2. WebView2 propaga a C# mediante `ContainsFullScreenElementChanged`
3. C# (`OnFullScreenElementChanged`):
   - `_fullscreenManager.Enter(this)` — guarda estado previo (`FullscreenState` con WindowStyle, ResizeMode, WindowState) y aplica modo fullscreen (None, NoResize, Maximized, Topmost=true)
   - Oculta: Menu, StatusBar, GridSplitter, panel izquierdo (LeftColumn Width=0, MinWidth=0)
   - Al salir: `_fullscreenManager.Exit(this)` restaura desde `FullscreenState`; MainWindow restaura UI components
4. Escape o botón ⛶ → `document.exitFullscreen()` → restaura ventana normal

**Separación de responsabilidades:**
- `FullscreenManager` (implements `IFullscreenManager`): maneja solo el estado de la ventana (border, resize, topmost). Inyectado via field en MainWindow.
- `MainWindow`: maneja visibilidad de UI components (Menu, LeftColumn, GridSplitter, StatusBar).

**Componentes ocultos en fullscreen:**
- `Menu` (barra de menú Archivo/Ver/Ayuda)
- `GridSplitter` (separador del panel)
- `LeftColumn` (panel de canales TreeView)
- `StatusBar` (barra de estado inferior)

---

## Build y Ejecución

```powershell
cd D:\Sources\amaillo\utilities\Dash MPD Player\DashMPDPlayer
dotnet build
dotnet run
```

Salida: `bin\Debug\net10.0-windows\DashMPDPlayer.exe` con `canales_descifrados.json` y `Web\player.html` copiados automáticamente.

---

## Dependencias Externas

- **Shaka Player**: `https://cdn.jsdelivr.net/npm/shaka-player@4/dist/shaka-player.compiled.js` (cargado por player.html)
- **WebView2 Runtime**: Microsoft Edge WebView2 (requerido en el sistema)
- **CDN**: CDN token generator en `cdn-token.app.flow.com.ar`

---

## Logging (NLog)

Toda la aplicación usa NLog para logging estructurado a consola y archivo.

### Configuración

Archivo: `nlog.config` (copiado al output directory)

| Parámetro      | Valor |
|----------------|-------|
| Console target | `ColoredConsole` con highlighting por nivel |
| File target    | `logs/dash-player-{yyyy-MM-dd}.log` |
| Archive        | Rotación diaria, máximo 10MB por archivo, 30 días de retención |
| Layout         | `{longdate} | {level:padding=-5} | {logger:shortName=true} | {message}{exception}` |
| Min level      | `Trace` (consola y archivo) |

### Uso por clase

Cada clase tiene su propio logger estático:
```csharp
private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
```

### Niveles usados

| Nivel  | Uso |
|--------|-----|
| `Fatal` | Excepciones no controladas (DispatcherUnhandledException) |
| `Error` | Excepciones capturadas, fallos de red, parse errors |
| `Warn`  | Situaciones inesperadas no críticas, fallbacks, tokens faltantes |
| `Info`  | Eventos principales: inicio, carga de canales, conexión, firma |
| `Debug` | Request HTTP, segmentos servidos, detalles de configuración |
| `Trace` | Búsquedas de archivos, pasos muy detallados |

### Integración con UI StatusBar

El `ProxyService` mantiene el evento `OnLog` que el `MainWindow` suscribe para actualizar `StatusTextBlock.Text`. Esto permite que los mensajes principales del proxy se reflejen en la barra de estado de la UI además del logging a disco/consola.

### Archivos con logging

- `App.xaml.cs` — Startup/shutdown, excepciones no controladas
- `MainWindow.xaml.cs` — Eventos de UI, selección/reproducción de canales
- `ProxyService.cs` — Requests HTTP, firma CDN, segmentos, config
- `ChannelService.cs` — Carga de JSON, búsqueda de archivos
- `TokenService.cs` — Bearer token + CDN token management
- `MpdRewriter.cs` — Rewrite de MPD (XML + Regex)

---

## Troubleshooting

### App inicia pero no se ve la ventana (proceso visible en Task Manager)

**Causa:** `DispatcherUnhandledException` en `App.xaml.cs:34` tiene `e.Handled = true`, lo que **silencia cualquier error de UI** (XAML parse errors, recursos faltantes). La app sigue corriendo con su message pump pero sin ventana.

**Síntomas:**
- Proceso aparece en Task Manager
- No hay ventana visible
- LOG FATAL con `XamlParseException` o `IOException` en la salida estándar

**Solucionar:**
1. Revisar la salida estándar de la app (`STDOUT`) buscando mensajes `FATAL`
2. Si no hay salida visible, ejecutar `dotnet run --project ... 2>&1` para capturar todo
3. Causas comunes:
   - `Icon="app.ico"` en XAML sin recurso embebido → `IOException: No se encuentra el recurso 'app.ico'`
   - Cualquier otro error de binding o recurso en XAML

**Regla:** Siempre que la ventana no aparezca, buscar `FATAL` en los logs antes de asumir otro tipo de fallo.

1. No hay manejo de errores de red completo (timeouts, caídas)
2. Las URLs con `=` en medio de query params pueden romperse (se usa path-based como workaround)
3. No hay persistencia de canal seleccionado entre sesiones
4. Los headers del JSON del canal (`.com.py`) se usan también para el CDN token generator, que internamente espera dominio `.com.ar` — si el CDN valida Origin/Referer estrictamente, puede fallar
5. El bearer token se obtiene de `piratacodigo.json` sin autenticación previa; si ese endpoint cambia o requiere auth, el flujo se rompe
6. La detección de canales Flow se hace solo por presencia de `headers` (heurística); no hay una señal explícita en el JSON
7. Algunos servidores de licencia (ej: `prls.atv-ps.amazon.com`) no son accesibles desde internet público — canales con DRM cuyo license server es interno no pueden reproducirse
8. Canales HLS con headers personalizados (Flow) no inyectan headers via hls.js — requiere implementar `loader` o `xhrSetup` en hls.js
9. Canales HLS con DRM (Widevine) no funcionan con hls.js

**Problemas resueltos:**
- ~~Autoplay con sonido requiere gesto de usuario~~ → Flag `--autoplay-policy=no-user-gesture-required`
- ~~EME no disponible con `NavigateToString()`~~ → HTTP origin via `Navigate("http://localhost:{port}/player")`
- ~~URLs de segmento muy largas para HttpListener~~ → Session tokens numéricos en vez de base64
- ~~Controles ocultos al hoverear botones~~ → CSS classes en vez de inline styles
- ~~Sin búsqueda/filtro en TreeView~~ → `OnSearchTextChanged` con filtro accent-insensitive + flag `_channelsLoaded`
- ~~Memory leak de session tokens~~ → `SessionEntry` con `Created` + `CleanupStaleSessions()` cada 50 entradas
- ~~Errores de segmento siempre devuelven 500~~ → Catch filter propaga status code original del CDN
- ~~Timeout único de 30s para todo~~ → `_httpClient` (30s manifest) + `_httpClientSegments` (120s segmentos)
- ~~Re-descarga de iconos en cada búsqueda~~ → `_iconCache` con `Dictionary<string, BitmapSource>`
- ~~Bearer token vencido sin retry~~ → `TokenService.InvalidateToken()` + reintento en `SendManifestRequestAsync()` (401)
- ~~ProxyService God Class (~600 líneas)~~ → Extraídos `TokenService`, `MpdRewriter`, interfaz `IProxyService` (~300 líneas)
- ~~Dead code (ViewModels, endpoints, Base64Url, modelos)~~ → Eliminado `MainViewModel.cs`, `/config`/`/status`, `Base64Url*`, `CountSegmentUrls`, `Headers2`, `HeadersM3u8`, `HeadersUrl`, `GlobalIndex`, `HiddenSamples`, `AssemblyInfo.cs`
- ~~Búsqueda inline + duplicación de RemoveDiacritics~~ → `SearchService` + `ISearchService`
- ~~Fullscreen state esparcido en 4 fields~~ → `FullscreenState` struct + `IFullscreenManager`/`FullscreenManager`
- ~~DRM no detectado en canales con PlayReady CENC~~ → `MpdRewriter.DetectDrm()` parsea `<mspr:pro>` binario (WORD length, no DWORD)
- ~~LICENSE requests fallan por CORS con servidores externos~~ → Proxy local `/license` endpoint rutea requests server-side con headers Origin/Referer
- ~~Canales HLS no se reproducen (Shaka transmuxer crash)~~ → hls.js para canales HLS, Shaka solo para DASH
- ~~Icono en XAML con `Icon="app.ico"` causa crash silencioso~~ → `<EmbeddedResource>` en csproj + `BitmapFrame.Create` desde stream del assembly
- ~~Panel de canales no colapsa completamente por `MinWidth="200"`~~ → `MinWidth="0"` en el `ColumnDefinition`
- ~~Checkbox del menú no muestra tilde visualmente~~ → TextBlock "✓" dentro del Border, controlado por trigger `IsChecked`
- ~~Barra de título con fondo blanco/claro~~ → `DwmSetWindowAttribute` con `DWMWA_USE_IMMERSIVE_DARK_MODE=20` en evento `SourceInitialized`
- ~~Window.Topmost bloquea la ventana aunque no haya reproducción~~ → `Topmost` condicional: solo cuando check activo + canal cargado + reproduciendo
