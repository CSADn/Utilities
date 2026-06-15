# Dash MPD Player → MiTube — Knowledge Base

## Descripción General

Proyecto de migración de un reproductor IPTV DASH en WPF (.NET 10) hacia una arquitectura híbrida:
- **mitube.service** — ASP.NET Core Web API 10 (backend ligero, JWT, caché)
- **mitube.app** — Android App Kotlin nativo (proxy local + WebView + Shaka/hls.js)
- **DashMPDPlayer** — Proyecto WPF original (se conserva como referencia)

Formato JSON canales en `samples/`:

```json
[{ "name": "📺 DEPORTES", "samples": [{ "name": "Dsports 1", "url": "...", "type": "CLEARKEY", "drm_license_uri": "...", "icono": "...", "headers": { "Origin": "...", "Referer": "...", "User-Agent": "..." } }] }]
```

---

## Stack Tecnológico

| Componente | Tecnología |
|---|---|
| Backend | .NET 10 ASP.NET Core Web API |
| DB | SQLite (EF Core) |
| Auth | JWT (Bearer) |
| Android App | Kotlin, Gradle, NanoHTTPd, OkHttp |
| DASH Player | Shaka Player 4.x |
| HLS Player | hls.js |
| DRM | Clearkey / Widevine (EME) |

---

## Estructura del Proyecto

```
Dash MPD Player/
├── AGENTS.md                           ← Este archivo
├── DashMPDPlayer/                      ← Proyecto WPF original (NET 10 WPF + WebView2)
│   ├── DashMPDPlayer.csproj
│   ├── AGENTS.md                       ← Knowledge base del WPF original
│   ├── MainWindow.xaml / .cs
│   ├── App.xaml / .cs
│   ├── Models/  Channel.cs, ChannelGroup.cs, DrmInfo.cs
│   ├── Services/ ProxyService.cs, TokenService.cs, MpdRewriter.cs, ChannelService.cs, ...
│   ├── Interfaces/ IProxyService.cs, IChannelService.cs, ...
│   └── Web/ player.html               ← Shaka + hls.js embebido
│
├── mitube.service/                     ← Backend API (.NET 10)
│   ├── Program.cs                      ← Startup con JWT, CORS, DI, Swagger
│   ├── appsettings.json                ← JWT secret, connection string, channel JSON path
│   ├── mitube.service.csproj
│   ├── Controllers/
│   │   ├── AuthController.cs           ← POST /api/auth/login (solo login)
│   │   └── ChannelsController.cs       ← GET /api/channels, PUT /api/channels/upload
│   ├── Services/
│   │   ├── JwtService.cs               ← Generación de JWT (HS256)
│   │   ├── IJwtService.cs
│   │   ├── CdnTokenService.cs          ← Fetch bearer token + CDN token (piratacodigo + cdn-token generator)
│   │   ├── ChannelCacheService.cs      ← IMemoryCache + FileSystemWatcher
│   │   ├── IChannelCacheService.cs
│   │   └── Repositories/
│   │       ├── UserRepository.cs       ← BCrypt verify + SQLite query
│   │       └── IUserRepository.cs
│   ├── Models/
│   │   ├── Channel.cs                  ← Mismo que el WPF (con JsonPropertyName)
│   │   ├── ChannelGroup.cs
│   │   └── User.cs                     ← Entity EF Core (Id, Username, PasswordHash, ...)
│   ├── Data/
│   │   ├── AppDbContext.cs
│   │   ├── canales.json                ← JSON de canales (cacheado)
│   │   └── seed/seed-users.sql         ← Script para alta manual de usuarios
│   └── Properties/launchSettings.json
│
├── mitube.app/                         ← Android App (Kotlin, multi-módulo)
│   ├── settings.gradle.kts
│   ├── build.gradle.kts
│   ├── gradle.properties
│   ├── core/                           ← Módulo compartido (mobile + TV)
│   │   ├── build.gradle.kts
│   │   └── src/main/java/com/mitube/core/
│   │       ├── proxy/
│   │       │   ├── ProxyServer.kt      ← NanoHTTPd (manifest, segment, license, fetch, player)
│   │       │   ├── TokenService.kt     ← CDN token via backend /api/proxy/cdn-token (port C#)
│   │       │   └── MpdRewriter.kt      ← XML + Regex rewrite + PlayReady binary parser (port C#)
│   │   ├── api/
│   │   │   ├── ApiClient.kt        ← Retrofit + OkHttp client singleton
│   │   │   └── MitubeApi.kt        ← login, getChannels, validate
│   │   ├── SessionManager.kt       ← EncryptedSharedPreferences (AES256-GCM) — JWT, server URL, username (compartido mobile + TV)
│   │   ├── models/
│   │   │   ├── Channel.kt          ← Data class con parseClearkeyLicense()
│   │   │   ├── ChannelGroup.kt
│   │   │   ├── DrmInfo.kt          ← UUIDs Widevine/PlayReady/ClearKey
│   │   │   └── LoginResponse.kt    ← LoginRequest, LoginResponse
│   │       ├── player/
│   │       │   ├── JsBridge.kt         ← @JavascriptInterface bridge WebView ↔ Kotlin
│   │       │   └── PlayerWebViewFragment.kt ← WebView con EME, autoplay, bridge
│   │       └── service/
│   │           └── ProxyForegroundService.kt ← Service persistente para proxy
│   │
│   ├── mobile/                         ← Módulo teléfonos
│   │   ├── build.gradle.kts
│   │   └── src/main/
│   │       ├── AndroidManifest.xml
│   │       ├── java/com/mitube/mobile/
│   │       │   ├── MiTubeMobileApp.kt    ← Application + init SessionManager (from core)
│   │       │   ├── ui/
│   │       │   │   ├── LoginActivity.kt  ← Login con JWT persistente + auto-fill
│   │       │   │   ├── HomeActivity.kt   ← Auto-login + SwipeRefresh + ViewSwitcher
│   │       │   │   └── PlayerActivity.kt ← Landscape lock + overlay táctil + back
│   │       │   └── adapters/
│   │       │       ├── CategoryAdapter.kt    ← RecyclerView vertical + LinearSnapHelper
│   │       │       └── ChannelCardAdapter.kt ← RecyclerView horizontal + Coil
│   │       ├── res/layout/
│   │       │   ├── activity_login.xml
│   │       │   ├── activity_home.xml     ← SwipeRefreshLayout + ViewSwitcher + SearchView
│   │       │   ├── activity_player.xml   ← FrameLayout + overlay bar
│   │       │   ├── item_category.xml
│   │       │   └── item_channel_card.xml
│   │       ├── res/drawable/bg_input.xml
│   │       ├── res/values/themes.xml     ← Theme.MiTube + Theme.MiTube.Translucent
│   │       └── assets/player.html      ← Shaka + hls.js (adaptado Android)
│   │
│   └── tv/                             ← Módulo Android TV
│       ├── build.gradle.kts
│       └── src/main/
│           ├── AndroidManifest.xml      ← LEANBACK_LAUNCHER + landscape locked
│           ├── java/com/mitube/tv/
│           │   ├── MiTubeTvApp.kt        ← Application + init SessionManager (from core)
│           │   └── ui/
│           │       ├── LoginActivity.kt   ← JWT persistente + auto-fill (como mobile)
│           │       ├── MainBrowseActivity.kt ← Auto-login + container
│           │       ├── MainBrowseFragment.kt ← BrowseSupportFragment + 401 handling + item click → PlayerActivity
│           │       ├── CardPresenter.kt      ← ImageCardView + Glide (DiskCacheStrategy.ALL, error fallback)
│           │       └── PlayerActivity.kt     ← Landscape lock + fullscreen + overlay + WebView proxy
│           ├── res/layout/
│           │   ├── activity_login.xml
│           │   ├── activity_main_browse.xml  ← FrameLayout container for BrowseFragment
│           │   └── activity_player.xml       ← FrameLayout + overlay con nombre + badge "EN VIVO"
│           ├── res/values/themes.xml         ← Theme.Leanback.Dark (parent correcto para Android TV)
│           └── assets/player.html
│
├── samples/                            ← JSON de muestra y MPDs de ejemplo
│   ├── canales_descifrados.json.argentina
│   ├── canales_descifrados.json.small
│   ├── DSports_1.mpd
│   └── reverse-2026-06-06.log
```

---

## mitube.service — API Endpoints

| Método | Ruta | Auth | Body | Respuesta |
|---|---|---|---|---|
| POST | `/api/auth/login` | ❌ | `{ username, password }` | `{ token, expiresIn, username, displayName }` |
| GET | `/api/auth/validate` | ✅ JWT | — | `{ valid, username }` |
| GET | `/api/channels` | ✅ JWT | — | `[{ name, samples: [...] }]` |
| PUT | `/api/channels/upload` | ✅ JWT | `[{ name, samples: [...] }]` | `{ message, count }` |
| POST | `/api/channels/reload` | ✅ JWT | — | `{ message, count }` |
| GET | `/api/proxy/cdn-token?url=<mpd_url>` | ✅ JWT | — | `{ cdnToken, bearerToken }` |

### Seed por Defecto

| Username | Password | Hash |
|---|---|---|
| `adn` | `123456La` | `$2a$11$gpqCRQOwALco6B7B4q/FT.J84v0FmxRavuGYoz6Ynn4YUibmSTA6y` |

### Mecanismo de Cache de Canales

- `ChannelCacheService` implementa `IMemoryCache` con sliding expiration de 30 min
- `FileSystemWatcher` suscribe cambios en `canales.json` → invalida cache automáticamente
- Endpoint `POST /api/channels/reload` fuerza invalidación manual
- `PUT /api/channels/upload` escribe JSON + invalida cache

### Base de Datos (SQLite)

- Tabla única: `Users` (Id, Username UNIQUE, PasswordHash, DisplayName, CreatedAt, IsActive)
- Auto-migrate en startup con `db.Database.EnsureCreated()`
- Alta de usuarios manual vía SQL o herramienta externa (BCrypt hash)

---

## mitube.app — Arquitectura Android

### Core (`:core` module — compartido mobile + TV)

| Componente | Responsabilidad |
|---|---|
| `ProxyServer.kt` | NanoHTTPd: endpoints `/manifest`, `/segment/{token}`, `/license`, `/player`, `/fetch` |
| `TokenService.kt` | Bearer token + CDN token (port desde C# TokenService) |
| `MpdRewriter.kt` | XML/XDocument + Regex rewrite + PlayReady binary parser (port desde C#) |
| `JsBridge.kt` | `@JavascriptInterface` para bridge WebView ↔ Kotlin |
| `PlayerWebViewFragment.kt` | WebView con EME, autoplay, bridge JS |
| `ProxyForegroundService.kt` | Service persistente para mantener vivo el proxy |
| `ApiClient.kt` | Retrofit client para comunicarse con mitube.service |
| `MitubeApi.kt` | Interface Retrofit: login, getChannels |
| `Channel.kt` / `ChannelGroup.kt` | Data classes (mismo schema JSON) |
| `SessionManager.kt` | EncryptedSharedPreferences (AES256-GCM) — JWT, server URL, username (compartido mobile + TV) |

### Mobile (`:mobile` module)

- Carousel horizontal por categoría (RecyclerView vertical anidando horizontal)
- MaterialCardView por canal con icono (Coil) + nombre
- Navegación touch (swipe) + dpad opcional
- Login screen, Player screen con WebView

### Mobile — Phase 3 Refinements

| Feature | Archivos | Detalle |
|---|---|---|
| **JWT Persistente** | `SessionManager.kt` | EncryptedSharedPreferences (AES256-GCM) guarda JWT, server URL y último username |
| **Auto-fill Login** | `LoginActivity.kt:30-31` | Restaura server URL y username desde sesión previa al mostrar pantalla |
| **Auto-login** | `HomeActivity.kt:65-85` | Si hay JWT guardado → `validate()` server-side → si token inválido/expirado, limpia sesión y redirige a login. Si 401 en `getChannels()`, mismo comportamiento. |
| **SwipeRefresh** | `activity_home.xml:40-65` | SwipeRefreshLayout envuelve ViewSwitcher con pull-to-refresh para recargar canales |
| **ViewSwitcher** | `activity_home.xml` | Muestra "Cargando..." (child 0) hasta que datos llegan, luego RecyclerView (child 1) |
| **LinearSnapHelper** | `CategoryAdapter.kt:47-49` | Snap centrado en cada canal horizontal (efecto carrusel) |
| **Player Landscape** | `PlayerActivity.kt:23-30` | `SCREEN_ORIENTATION_SENSOR_LANDSCAPE` + flags FULLSCREEN + IMMERSIVE_STICKY + KEEP_SCREEN_ON |
| **Player Overlay** | `activity_player.xml` | Barra superior semitransparente con botón cerrar + nombre canal + badge "EN VIVO" |
| **Theme Translucent** | `themes.xml` | `Theme.MiTube.Translucent` para PlayerActivity (sin action bar, transparente) |
| **Back navigation** | `activity_player.xml:12-19` | ImageView close button → `finish()` + hardware back soportado

### TV (`:tv` module) — Phase 4

- `MiTubeTvApp.kt` inicializa SessionManager + ApiClient.restoreToken() desde core
- `LoginActivity.kt` con JWT persistente, auto-fill de server URL + último username (idéntico a mobile)
- `MainBrowseActivity.kt` con auto-login check + validate() server-side, redirige a Login si no hay JWT o token inválido
- `MainBrowseFragment.kt`: BrowseSupportFragment con rows de canales, 401 → limpia sesión, item click → PlayerActivity
- `CardPresenter.kt`: ImageCardView con Glide (DiskCacheStrategy.ALL + error fallback)
- `PlayerActivity.kt`: Landscape locked, FULLSCREEN + IMMERSIVE_STICKY + KEEP_SCREEN_ON, overlay semitransparente con nombre y badge "EN VIVO"
- `activity_main_browse.xml`: FrameLayout simple para el BrowseFragment
- `activity_player.xml`: FrameLayout con overlay semitransparente superior + badge
- Tema: `Theme.Leanback.Dark` (parent correcto para Leanback, no AppCompat)
- Search: falta implementar Leanback SearchSupportFragment (pendiente)

### PlayerWebViewFragment — Carga de player.html

El `PlayerWebViewFragment` en core carga `player.html` desde `assets/` del módulo activo (mobile o TV) y lo inyecta en `ProxyServer.setPlayerHtml()`. El proxy sirve el HTML en `localhost:{puerto}/player`. Esto asegura que ambos módulos usen el mismo fragment de player sin duplicar lógica.

### Conexión entre Android y Servidor (Flujo de Login/Navegación)

```
App launch → MiTubeMobileApp.onCreate()
  └→ SessionManager init ← EncryptedSharedPreferences
  └→ Restaura serverUrl → ApiClient.configure(url)     ← CRÍTICO: antes del primer acceso a api (lazy)
  └→ Restaura JWT → ApiClient.restoreToken(token)
  └→ LoginActivity.onCreate()
       ├→ ¿JWT guardado? → HomeActivity (auto-login, sin formulario)
       └→ ¿No JWT?       → Muestra login con server URL + username pre-rellenados
                            → Login → POST /api/auth/login → SessionManager guarda JWT
                            → HomeActivity

HomeActivity.onCreate()
  └→ ¿token null? → LoginActivity
  └→ GET /api/auth/validate (server-side)
       ├→ Válido  → GET /api/channels → ViewSwitcher (child 1)
       └→ Inválido → SessionManager.clear() (solo token, conserva serverUrl/username)
                     → ApiClient.setToken(null) → LoginActivity

Usuario toca canal → PlayerActivity.onCreate()

Usuario toca canal → PlayerActivity.onCreate()
  └→ SCREEN_ORIENTATION_SENSOR_LANDSCAPE + Fullscreen + No ActionBar
  └→ ProxyServer puerto libre → PlayerWebViewFragment
  └→ Overlay visible: botón cerrar + nombre canal + badge "EN VIVO"
  └→ 4s después: overlay se oculta automáticamente (auto-hide timer)
  └→ Usuario toca video (mobile) / presiona Enter (TV) → overlay visible otra vez
  └→ 4s después: overlay se oculta nuevamente
  └→ Back / close → finish() → HomeActivity
```

### Overlay Auto-hide (Mobile + TV)

El overlay del player (header con nombre, botón cerrar, badge "EN VIVO") tiene auto-hide y toggle. También sincroniza el footer de controles JS (play/pause, mute, fullscreen definidos en `player.html`):

| Comportamiento | Mobile (`dispatchTouchEvent`) | TV (`dispatchKeyEvent`) |
|---|---|---|
| Auto-hide inicial | 4s después de `onCreate` | 4s después de `onCreate` |
| Mostrar overlay | Tap en cualquier parte del video | Presionar Enter/DPAD_CENTER en control remoto |
| Re-ocultar | 4s después del último tap | 4s después del último Enter |
| Sincronización footer | `forceHideControls()` via `evaluateJavascript` limpia clases `show`/`visible` y cancela timer JS del footer | Misma lógica |

**Implementación:** `Handler.postDelayed()` con `Runnable` que setea `playerOverlay.visibility = View.GONE` y llama `playerFragment?.forceHideControls()`. El toggle (show/hide) se maneja con `overlayVisible` boolean flag. El footer de `player.html` (controles play/pause/mute/fullscreen con timer JS de 5s) se fuerza a ocultar junto con el header para mantener sincronía visual.

**Archivos:**
- `core/src/main/java/com/mitube/core/player/PlayerWebViewFragment.kt` — `forceHideControls()` inyecta JS para limpiar estado del footer
- `mobile/src/main/java/com/mitube/mobile/ui/PlayerActivity.kt` — `dispatchTouchEvent`, timer, store fragment ref
- `tv/src/main/java/com/mitube/tv/ui/PlayerActivity.kt` — `dispatchKeyEvent`, timer, store fragment ref

### Flujo de Reproducción en Tizen

```
1. TV enciende app → carga bundle.js + player.bundle.js (static <script> tags)
2. Sincroniza JSON canales desde mitube.service (GET /api/channels + JWT via XHR)
3. Usuario selecciona canal en carrusel ← dpad navigation (no Tizen spatial nav)
4. PlayerView.ts → PlayerSetup.loadChannel(channel)
5. Flow channel: loadDash firma URL con CDN token, llama a shakaPlayer.load(signedUrl)
6. Shaka NetworkingEngine requestFilter (verificar):
   └→ Flow: rewrite request.uris[0] → http://backend:5241/api/proxy/fetch?url=<encoded>
   └→ Flow: agrega X-Proxy-Origin, X-Proxy-Referer, X-Proxy-User-Agent headers
   └→ No-Flow: setea headers permitidos directo
   └→ Skip data:/blob: URIs
7. Backend ProxyController recibe:
   └→ GET/POST /api/proxy/fetch?url=<upstream_url>
   └→ HttpClient (TryAddWithoutValidation) con Origin/Referer/User-Agent reales
   └→ Si Content-Type es dash+xml/mpd → bufferiza, inyecta <BaseURL>, retorna XML modificado
   └→ Si es segmento/licencia → stream directo
8. Shaka recibe MPD con BaseURL → resuelve segmentos contra CDN origin
9. Shaka pide cada segmento → requestFilter → backend proxy → CDN con headers correctos
10. Shaka pide licencia DRM (Widevine) → requestFilter → backend proxy → DRM server
11. Video se reproduce. CERO tráfico de video pasa por el internet del backend
    (el backend es relay local, los bytes van TV ↔ PC (LAN) ↔ CDN (internet))
```

### Conexión entre Android y Servidor

Usos:
- Login (JWT)
- Sincronización de JSON de canales
- Obtención de CDN tokens para canales Flow (`GET /api/proxy/cdn-token?url=<mpd>` con JWT)
- Validación de token JWT al inicio (`GET /api/auth/validate`)
- NO para tráfico de video

### ProxyServer.kt — Endpoints (NanoHTTPd)

| Ruta | Método | Función |
|---|---|---|
| `/player` o `/player.html` | GET | Sirve player.html |
| `/manifest?url=<mpd>` | GET | Obtiene MPD vía CDN token generator, reescribe URLs |
| `/segment/{token}/{path}` | GET | Proxy inverso para segmentos DASH |
| `/license?url=<license_server>` | POST | Proxy inverso para requests de licencia DRM |
| `/fetch?url=<url>` | GET | Proxy genérico |

### player.html (Android) — Cambios vs WPF original

| WPF Original | Android |
|---|---|
| `window.chrome.webview.postMessage(...)` | `window.Android.postMessage(JSON.stringify(msg))` |
| `window.chrome.webview.addEventListener('message', ...)` | `postToAndroid()` helper function |
| Console bridge script | ❌ Eliminado (console nativo) |
| `CoreWebView2.PostWebMessageAsJson(json)` | `WebView.evaluateJavascript("loadChannel(...)")` |
| Input: `onWebViewMessage(event)` | Input: `Android.onLoadChannel(channelJson)` via `@JavascriptInterface` |

---

## WPF Original — Arquitectura (Referencia para el port)

### Proxy Local

El WPF corre un `HttpListener` en un puerto aleatorio. Endpoints:

| Ruta | Método | Función |
|---|---|---|
| `/manifest?url=<mpd>` | GET | Obtiene MPD vía CDN token generator, reescribe URLs |
| `/segment/{token}/{path}` | GET | Proxy inverso para segmentos DASH |
| `/license?url=<license_server>` | POST | Proxy inverso para requests de licencia DRM |
| `/player` | GET | Sirve player.html |
| `/fetch?url=<url>` | GET | Proxy genérico |

### MpdRewriter (C# → Kotlin)

- `Rewrite(mpdXml, token)`: XML XDocument parsing + fallback Regex
- `DetectDrm(mpdXml)`: extrae `<ContentProtection>`, UUIDs Widevine/PlayReady/ClearKey, `<dashif:laurl>`, `<mspr:pro>` binario

### TokenService (C# → Kotlin)

- `GetBearerTokenAsync()`: GET `https://app.femon.net/pirata/piratacodigo.json` → cache 1h
- `RequestCdnTokenAsync(mpdUrl, bearerToken, headers)`: GET `cdn-token.app.flow.com.ar/cdntoken/v2/generator?path=<url>` → JWT
- `InvalidateToken()`: limpia cache forzando renovación (retry on 401)

### DRM Auto-detección

Canales con `drm_license_uri` vacío + URL `.mpd` → detecta automático:
- Widevine UUID: `edef8ba9-79d6-4ace-a3c8-27dcd51d21ed`
- PlayReady UUID: `9a04f079-9840-4286-ab92-e65be0885f95`
- ClearKey UUID: `e2719d58-a985-b3c9-781a-b030af78d30e`

### Canales Flow (con headers)

Heurística: si `headers != null` → requiere bearer token + CDN token + URL firmada.

### Dos HttpClient en ProxyService

| Propósito | Timeout | AutoRedirect |
|---|---|---|
| Manifest + tokens CDN | 30s | true |
| Segmentos | 120s | false |

---

## Android — Flow Channels con CDN Token vía Backend

Android usa su propio proxy local (NanoHTTPd en `ProxyServer.kt`), pero el TokenService ahora centraliza la obtención de tokens CDN a través del backend:

```
Android ProxyServer.handleManifest()
  └→ TokenService.getCdnTokenAsync(mpdUrl)
      └→ GET /api/proxy/cdn-token?url=<encoded_mpd> (JWT autenticado)
          └→ Backend CdnTokenService.GetBearerTokenAsync() → piratacodigo.json (1h cache)
          └→ Backend CdnTokenService.RequestCdnTokenAsync() → cdn-token.app.flow.com.ar
          └→ Retorna { cdnToken, bearerToken }
  └→ Firma URL del MPD con cdntoken
  └→ Fetch del MPD firmado con headers Origin/Referer
  └→ Rewrite de URLs de segmentos al proxy local
```

### manifestUrl / licenseProxyUrl para canales Flow

`PlayerWebViewFragment.loadChannel()` computa `manifestUrl` y `licenseProxyUrl` apuntando al proxy local ANTES de serializar el Channel a JSON para el WebView:

```kotlin
val enrichedChannel = if (proxyPort > 0 && channel.headers != null) {
    channel.copy(
        manifestUrl = "http://localhost:$proxyPort/manifest?url=${encode(channel.url)}",
        licenseProxyUrl = "http://localhost:$proxyPort/license?url=${encode(channel.drm_license_uri)}"
    )
}
```

`player.html` prefiere `channel.manifestUrl` sobre `channel.url` en `player.load()`. El requestFilter de Shaka reescribe licencias a `licenseProxyUrl`.

---

## Limitaciones Conocidas

1. Canales Flow requieren proxy local para inyectar headers Origin/Referer — no funciona desde browser puro
2. Clearkey no requiere proxy de licencia (keyId+key se pasan directo a Shaka)
3. Autoplay con sonido requiere user gesture (muted autoplay funciona)
4. Android WebView debe tener EME habilitado (System WebView Chromium ≥ API 24)
5. El proxy en Android debe correr como Foreground Service para no ser killado
6. Tokens CDN tienen expiración corta — MPD re-fetch cada 2s (live streams)
7. `piratacodigo.json` es punto único de fallo (sin autenticación)
8. Android WebView muestra un placeholder gris con botón de play en `<video>` antes de cargar la transmisión. El shadow DOM interno de Chromium renderiza un overlay que `poster=""` no desactiva. **Solución validada** (3 capas):
   - `poster` con data URI de píxel transparente: `poster="data:image/gif;base64,R0lGODlhAQABAIAAAAAAAP///yH5BAEAAAAALAAAAAABAAEAAAIBRAA7"`
   - CSS para ocultar shadow DOM: `video::-webkit-media-controls{display:none!important}` + `video::-webkit-media-controls-overlay-play-button{display:none!important}` + `video::-webkit-media-controls-start-playback-button{display:none!important}`
   - El reproductor permanece en negro durante la carga de la transmisión.

---

## Android Debugging (ADB + LDPlayer)

### Captura de Crash Stack Trace

Cuando la app crashea, usar `logcat` para capturar el stack trace completo:

```powershell
$adb = "D:\Android\platform-tools\adb.exe"
$device = "127.0.0.1:5555"

# 1. Limpiar log buffer
& $adb -s $device logcat -c

# 2. Lanzar la app
& $adb -s $device shell am start -n "com.mitube.mobile/.ui.LoginActivity"

# 3. (Reproducir el crash manualmente en el emulador)

# 4. Capturar stack trace
$log = & $adb -s $device logcat -d -v time 2>&1
$log | Select-String -Pattern "AndroidRuntime|FATAL|Caused by|Exception" -Context 0,20 | Select-Object -First 1
```

**Importante:** `logcat -d` (dump) retorna TODO el buffer desde el último `logcat -c` (clear). Si hay múltiples crashes en el buffer, filtrar por `AndroidRuntime` para encontrar el último.

### Errores Comunes de Runtime en Android

| Error | Causa Raíz | Síntoma | Solución |
|---|---|---|---|
| `ClassCastException: androidx.appcompat.widget.SearchView cannot be cast to android.widget.SearchView` | Layout XML usa clase AndroidX (`androidx.appcompat.widget.SearchView`) pero Kotlin importa clase framework (`android.widget.SearchView`) | Crash al inflar HomeActivity | Cambiar import a `androidx.appcompat.widget.SearchView` |
| `IllegalArgumentException: required Theme.MaterialComponents` | `MaterialCardView` verifica en runtime que el tema herede de `Theme.MaterialComponents` | Crash al inflar `item_channel_card.xml` | Cambiar parent del tema a `Theme.MaterialComponents.DayNight.DarkActionBar` |
| Layout XML y Kotlin import mismatch | Cualquier clase que existe tanto en framework (`android.*`) como en AndroidX (`androidx.*`) | `ClassCastException` en `findViewById()` | Siempre usar el mismo namespace en XML y Kotlin |
| `Shaka error code=1002 400` en segmentos (puerto -1 en URLs) | `NanoHTTPD.getListeningPort()` retorna **-1** cuando `myServerSocket` es null (antes de `start()`). `MpdRewriter` se inicializa durante la construcción de `ProxyServer`, ANTES de `start()`, usando `proxyBaseUrl()` → `listeningPort` → `-1`. Todas las URLs de segmentos se reescriben con puerto `-1`. | Shaka reporta error 1002, cat=1, sev=1, data contiene `http://localhost%3A-1/segment/...`, error HTTP 400. | Almacenar el puerto como propiedad `serverPort` y usarlo directamente en vez de `listeningPort` para la inicialización de `MpdRewriter` y `proxyBaseUrl()`. |

### NanoHTTPD 2.3.1 — `getListeningPort()` devuelve -1 antes de `start()`

**Problema:** En NanoHTTPD 2.3.1, `getListeningPort()` NO retorna `myPort` (el puerto del constructor). En su lugar, retorna `myServerSocket.getLocalPort()` si el socket no es null, o **-1** si es null:

```
// Bytecode decompilado de NanoHTTPD 2.3.1
getListeningPort() {
    if (myServerSocket != null)
        return myServerSocket.getLocalPort();  // funciona después de start()
    else
        return -1;                              // ANTES de start()
}
```

**Por qué ocurre:** `myPort` es `private final` — no se usa en `getListeningPort()`. El método delega completamente en `myServerSocket`, que se asigna en `start()`.

**Solución:**

```kotlin
class ProxyServer(port: Int) : NanoHTTPD(port) {
    private val serverPort = port              // ← almacenar el puerto

    private val mpdRewriter = MpdRewriter("http://localhost:$serverPort")  // ← usar serverPort, NO listeningPort

    private fun proxyBaseUrl() = "http://localhost:$serverPort"            // ← usar serverPort, NO listeningPort
}
```

**Archivo afectado:** `mitube.app/core/src/main/java/com/mitube/core/proxy/ProxyServer.kt`

**Aprendizaje:** Nunca usar `NanoHTTPD.listeningPort` (getListeningPort()) durante la construcción de la subclase ni antes de llamar a `start()`. Siempre almacenar el puerto del constructor en una propiedad.

### ADB Comandos Útiles

```powershell
$adb = "D:\Android\platform-tools\adb.exe"
$device = "127.0.0.1:5555"

# Instalar/reinstalar APK
& $adb -s $device install -r mobile/build/outputs/apk/debug/mobile-debug.apk

# Verificar si la app está corriendo
& $adb -s $device shell ps | Where-Object { $_ -match "mitube" }

# Limpiar datos de la app (forzar login fresh)
& $adb -s $device shell pm clear com.mitube.mobile

# Ver log filtrado por proceso
& $adb -s $device logcat -d -v time | Select-String -Pattern "mitube" -SimpleMatch | Select-Object -Last 20
```

---

## Build y Ejecución

```powershell
# Backend
cd D:\Sources\amaillo\utilities\Dash MPD Player\mitube.service
dotnet build
dotnet run

# WPF original (referencia)
cd D:\Sources\amaillo\utilities\Dash MPD Player\DashMPDPlayer
dotnet build
dotnet run
```

---

## Dependencias Externas

- Shaka Player 4.x: `https://cdn.jsdelivr.net/npm/shaka-player@4/dist/shaka-player.compiled.js`
- hls.js: `https://cdn.jsdelivr.net/npm/hls.js@latest`
- CDN Flow: `cdn-token.app.flow.com.ar`, `app.femon.net/pirata/piratacodigo.json`

---

## Dependencias Android

| Dependencia | Propósito | Módulo |
|---|---|---|
| `androidx.security:security-crypto:1.1.0-alpha06` | EncryptedSharedPreferences (AES256-GCM) para JWT persistente | core, mobile, tv |
| `androidx.swiperefreshlayout:swiperefreshlayout:1.1.0` | Pull-to-refresh en HomeActivity | mobile |
| `com.github.bumptech.glide:glide:4.16.0` | Carga de iconos en ImageCardView (TV) | tv |
| `androidx.leanback:leanback:1.0.0` | BrowseSupportFragment + Leanback UI | tv |
| `androidx.fragment:fragment-ktx:1.7.1` | Fragment transactions en TV | tv |
| `androidx.lifecycle:lifecycle-runtime-ktx:2.7.0` | Coroutines lifecycle-scoped | tv |

---

## Android Build Environment (set up on dev machine)

### Herramientas Instaladas

| Componente | Ruta | Propósito |
|---|---|---|
| JDK 17 | `C:\Program Files\Eclipse Adoptium\jdk-17.0.19.10-hotspot` | Compilación Kotlin/Android |
| Android SDK | `D:\Android\sdk` | Android SDK (platform 35, build-tools 35.0.0, cmdline-tools) |
| Android cmdline-tools | `D:\Android\sdk\cmdline-tools\latest\` | sdkmanager, avdmanager |
| Android Studio | `C:\Program Files\Android\Android Studio` | IDE (instalado via winget, no necesario para build CLI) |
| Gradle | `D:\Android\gradle-8.10.2` | Build system |
| platform-tools | `D:\Android\platform-tools` | ADB, fastboot |

### Variables de Entorno Requeridas

```powershell
$env:JAVA_HOME = "C:\Program Files\Eclipse Adoptium\jdk-17.0.19.10-hotspot"
$env:ANDROID_HOME = "D:\Android\sdk"
$env:ANDROID_SDK_ROOT = "D:\Android\sdk"
$env:Path = "$env:JAVA_HOME\bin;$env:ANDROID_HOME\platform-tools;$env:Path"
```

### Build del APK

```powershell
Set-Location -LiteralPath "D:\Sources\amaillo\utilities\Dash MPD Player\mitube.app"
# Compilar ambos módulos
& ".\gradlew.bat" :mobile:assembleDebug :tv:assembleDebug --no-daemon
```

Outputs:

| Módulo | Ruta | Tamaño |
|---|---|---|
| mobile | `mobile/build/outputs/apk/debug/mobile-debug.apk` | ~7.9 MB |
| tv | `tv/build/outputs/apk/debug/tv-debug.apk` | ~6.4 MB |

### Instalación en Dispositivo

```powershell
# Conectar dispositivo via ADB
& "D:\Android\platform-tools\adb.exe" connect <device-ip>:5555
# Instalar APK
& "D:\Android\platform-tools\adb.exe" install mobile/build/outputs/apk/debug/mobile-debug.apk
```

### Gradle Wrapper

El wrapper está en `mitube.app/gradle/wrapper/`. Se generó con:

```powershell
& "D:\Android\gradle-8.10.2\bin\gradle.bat" wrapper --gradle-version 8.10.2 --project-dir $projDir
```

El `local.properties` apunta al SDK:

```
sdk.dir=D\:\\Android\\sdk
```

### Configuración del SDK

El SDK se instaló desde cero vía cmdline-tools descargados manualmente:

```powershell
# 1. Descargar cmdline-tools
# URL: https://dl.google.com/android/repository/commandlinetools-win-11076708_latest.zip
# Extraer a D:\Android\sdk\cmdline-tools\latest\

# 2. Aceptar licencias
& "D:\Android\sdk\cmdline-tools\latest\bin\sdkmanager.bat" --sdk_root="D:\Android\sdk" --licenses --yes

# 3. Instalar platform y build-tools
& "D:\Android\sdk\cmdline-tools\latest\bin\sdkmanager.bat" --sdk_root="D:\Android\sdk" "platforms;android-35" "build-tools;35.0.0"
```

### Problemas de Compilación Resueltos

#### 1. Conflicto de imports `okhttp3.Response` vs `NanoHTTPD.Response`

**Problema:** El wildcard `import okhttp3.*` importa `okhttp3.Response` que choca con `NanoHTTPD.Response`. Código usaba `Response.Status.BAD_GATEWAY` y `Response.lookup()` que no existen en `okhttp3.Response`.

**Solución:** Cambiar `import okhttp3.*` por imports específicos de las clases usadas:
```kotlin
import fi.iki.elonen.NanoHTTPD
import fi.iki.elonen.NanoHTTPD.Response  // Ahora Response = NanoHTTPD.Response
import okhttp3.CookieJar
import okhttp3.OkHttpClient
import okhttp3.Request
import okhttp3.RequestBody
```

**Importante:** `NanoHTTPD.Response.Status.BAD_GATEWAY` NO existe en NanoHTTPD 2.3.1. Usar `Response.Status.lookup(502)` en su lugar.

#### 2. `javax.xml.xpath.XPathConstants` no disponible en Android compile SDK

**Problema:** `javax.xml.xpath.XPathConstants` y `javax.xml.xpath.XPathFactory` están disponibles en Android runtime desde API 24+ pero NO en los stubs de compilación (android.jar). Causa error `Unresolved reference 'NODES'`.

**Solución:** Reemplazar XPath por `Document.getElementsByTagNameNS()`:
```kotlin
// Antes (no compila):
val xPath: XPath = XPathFactory.newInstance().newXPath()
val nodes = xPath.evaluate("//*[local-name()='ContentProtection']", doc, XPathConstants.NODES)

// Después (compila):
val nodes = doc.getElementsByTagNameNS("*", "ContentProtection")
```

También `javax.xml.transform.TransformerFactory` está afectado de la misma manera — usar `Document.getElementsByTagNameNS()` en lugar de XPath queries.

#### 3. Dependencias `implementation` vs `api`

**Problema:** Las clases de Retrofit (`retrofit2.Response`) y NanoHTTPD (`NanoHTTPD`, `Response`) se usan en los módulos `:mobile` y `:tv` a través del módulo `:core`. Con `implementation`, las dependencias no son transitivas.

**Solución:** Usar `api` para dependencias que se exponen en la API pública del módulo `:core`:
```kotlin
// core/build.gradle.kts
dependencies {
    api("org.nanohttpd:nanohttpd:2.3.1")
    api("com.squareup.okhttp3:okhttp:4.12.0")
    api("com.squareup.retrofit2:retrofit:2.9.0")
    api("com.squareup.retrofit2:converter-gson:2.9.0")
    api("com.google.code.gson:gson:2.10.1")
    api("androidx.webkit:webkit:1.10.0")
    api("androidx.security:security-crypto:1.1.0-alpha06")
}
```

#### 4. `fragment-ktx` faltante en mobile

**Problema:** `PlayerActivity.kt` usa `import androidx.fragment.app.commit` (extension function `FragmentManager.commit {}`) que requiere `fragment-ktx`.

**Solución:** Agregar a `mobile/build.gradle.kts`:
```kotlin
implementation("androidx.fragment:fragment-ktx:1.7.1")
```

#### 5. `Theme.Leanback.Dark` no disponible en leanback 1.0.0

**Problema:** El tema `Theme.Leanback.Dark` no existe en `androidx.leanback:leanback:1.0.0`. Causa error AAPT.

**Solución:** Usar `Theme.AppCompat.NoActionBar` como parent:
```xml
<style name="Theme.Leanback" parent="@style/Theme.AppCompat.NoActionBar">
```

#### 6. CardPresenter — API de Leanback 1.0.0

**Problema:** En Leanback 1.0.0, `ImageCardView.CARD_INFO_VISIBLE_WHEN_ACTIVATED` no existe, `infoVisibility` setter no está disponible. Además `onBindViewHolder`/`onUnbindViewHolder` esperan `Presenter.ViewHolder` no un tipo local.

**Solución:**
- No llamar `setMainImageDimensions()` ni `infoVisibility`
- Usar `Presenter.ViewHolder` en las firmas de override
- Usar `cardView.context.getDrawable(R.drawable.xxx)` en lugar de `cardView.setMainImage(resId)`

#### 7. minSdk conflict — core requiere API 24

**Problema:** `:core` tiene `minSdk = 24` (usa `javax.xml.parsers`). `:tv` tenía `minSdk = 21`. El manifest merger falla.

**Solución:** Subir `minSdk` del módulo tv a 24.

#### 8. `SearchView` ClassCastException en HomeActivity (android.widget vs androidx.appcompat.widget)

**Problema:** El layout `activity_home.xml` usa `<androidx.appcompat.widget.SearchView>` pero `HomeActivity.kt:5` importa `android.widget.SearchView`. Al hacer `findViewById(R.id.searchView)` el cast falla porque la vista inflada es `androidx.appcompat.widget.SearchView` y el código espera `android.widget.SearchView`.

```
java.lang.ClassCastException: androidx.appcompat.widget.SearchView cannot be cast to android.widget.SearchView
    at com.mitube.mobile.ui.HomeActivity.onCreate(HomeActivity.kt:48)
```

**Solución:** Cambiar el import en `HomeActivity.kt:5`:
```kotlin
// ❌ import android.widget.SearchView
// ✅ import androidx.appcompat.widget.SearchView
```

**Regla general:** Siempre que el layout XML use una clase del namespace `androidx.*`, el import en Kotlin debe usar el mismo `androidx.*` — las clases framework (`android.widget.*`, `android.view.*`) y AndroidX NO son intercambiables aunque tengan el mismo nombre.

#### 9. `MaterialCardView` requiere `Theme.MaterialComponents` en runtime

**Problema:** El layout `item_channel_card.xml` usa `com.google.android.material.card.MaterialCardView`, que verifica en runtime que el tema de la actividad herede de `Theme.MaterialComponents`. El tema `Theme.MiTube` usaba `Theme.AppCompat.DayNight.DarkActionBar` como parent, causando:

```
java.lang.IllegalArgumentException: The style on this component requires your app theme to be Theme.MaterialComponents (or a descendant).
    at com.google.android.material.internal.ThemeEnforcement.checkTheme(ThemeEnforcement.java:247)
    at com.google.android.material.card.MaterialCardView.<init>(MaterialCardView.java:175)
```

**Solución:** Cambiar el parent del tema en `mobile/src/main/res/values/themes.xml`:
```xml
<!-- ❌ <style name="Theme.MiTube" parent="Theme.AppCompat.DayNight.DarkActionBar"> -->
<!-- ✅ -->
<style name="Theme.MiTube" parent="Theme.MaterialComponents.DayNight.DarkActionBar">
```

**Nota:** `com.google.android.material:material:1.11.0` ya estaba declarado en `mobile/build.gradle.kts`, por lo que no se necesita agregar una nueva dependencia.

### Resumen de Archivos Modificados

| Archivo | Cambio |
|---|---|
| `core/build.gradle.kts` | `implementation` → `api` para varias dependencias; agregado `logging-interceptor` |
| `mobile/build.gradle.kts` | Agregado `fragment-ktx:1.7.1` |
| `tv/build.gradle.kts` | `minSdk = 21` → `minSdk = 24` |
| `tv/src/main/res/values/themes.xml` | `Theme.Leanback.Dark` → `Theme.AppCompat.NoActionBar` |
| `core/src/main/.../proxy/ProxyServer.kt` | Eliminado `import okhttp3.*`; imports específicos; imports `NanoHTTPD.Response`; `BAD_GATEWAY` → `Status.lookup(502)` |
| `core/src/main/.../proxy/MpdRewriter.kt` | Reemplazado XPath por `getElementsByTagNameNS()`; imports simplificados |
| `mobile/src/main/.../ui/HomeActivity.kt` | `android.widget.SearchView` → `androidx.appcompat.widget.SearchView` (fix ClassCastException) |
| `tv/src/main/.../ui/CardPresenter.kt` | Signature override con `Presenter.ViewHolder`; métodos deprecated eliminados |
| `mobile/src/main/res/values/themes.xml` | `Theme.AppCompat.DayNight.DarkActionBar` → `Theme.MaterialComponents.DayNight.DarkActionBar` (fix MaterialCardView runtime crash) |
| `mobile/src/main/res/mipmap-anydpi-v26/ic_launcher.xml` | Creado icono adaptive placeholder |
| `mobile/src/main/res/mipmap-hdpi/ic_launcher.png` | Creado icono placeholder (1×1 PNG) |
| `mobile/src/main/res/drawable/ic_launcher_*.xml` | Vector drawables para icon |
| `mobile/src/main/res/values/themes.xml` | `Theme.MaterialComponents.DayNight.DarkActionBar` → `Theme.MaterialComponents.DayNight.NoActionBar` (eliminar ActionBar azul "MiTube") |
| `mobile/src/main/assets/player.html` | Agregado `poster="data:image/..."` + CSS `::-webkit-media-controls{display:none}` para eliminar placeholder gris de Chromium |
| `tv/src/main/assets/player.html` | Mismo fix que mobile (poster data URI + CSS shadow DOM) |
| `core/.../SessionManager.kt` | Agregada propiedad `displayName` con persistencia en EncryptedSharedPreferences (clave `KEY_DISPLAY_NAME`) |
| `mobile/.../ui/LoginActivity.kt` | Guarda `session.displayName = body?.displayName` tras login exitoso |
| `tv/.../ui/LoginActivity.kt` | Guarda `session.displayName = body?.displayName` tras login exitoso |
| `mobile/.../res/layout/activity_login.xml` | Eliminado TextView "MiTube" header |
| `tv/.../res/layout/activity_login.xml` | Eliminado TextView "MiTube TV" header |
| `mobile/.../res/layout/activity_home.xml` | Toolbar rediseñado a 1 fila: SearchView (weight 1) + channelCount + userNameText + logoutBtn |
| `mobile/.../ui/HomeActivity.kt` | Carga displayName desde session, wire up logoutBtn → clear() + ApiClient.setToken(null) + redirect |
| `mobile/.../res/layout/item_channel_card.xml` | Agregado `android:foreground="?attr/selectableItemBackground"` para ripple al presionar |
| `mobile/.../adapters/ChannelCardAdapter.kt` | Agregado `OnFocusChangeListener`: al enfocar → `strokeWidth=2dp`, `setStrokeColor(white)`, fondo `#3d3d3d`; al perder foco → restaurar default |
| `mobile/.../ui/HomeActivity.kt` | SearchView: texto blanco, hint gris claro, icono lupa y X con `setColorFilter(white)` |
| `mobile/.../res/layout/activity_login.xml` | Eliminados `nextFocus*` atributos XML (no resuelven correctamente con `@+id` antes de la declaración del view); agregados `imeOptions="actionNext"`/`actionDone` |
| `mobile/.../ui/LoginActivity.kt` | Cadena de foco programática vía `setNextFocusForwardId()` / `setNextFocusDownId()` usando `R.id` (confiable) |
| `tv/.../res/layout/activity_login.xml` | Mismos cambios que mobile |
| `tv/.../ui/LoginActivity.kt` | Mismos cambios que mobile |
| `core/.../SessionManager.kt` | Agregada propiedad `lastChannelName` persistida para recordar canal seleccionado |
| `mobile/.../ui/PlayerActivity.kt` | Guarda `sessionManager.lastChannelName` al cerrar reproductor (back button + onDestroy) |
| `mobile/.../ui/HomeActivity.kt` | `restoreLastChannel()` busca canal por nombre y hace `smoothScrollToPosition()` tras cargar lista |
| `mobile/.../adapters/ChannelCardAdapter.kt` | En `onBindViewHolder`: primer item → `nextFocusLeftId = self`; último → `nextFocusRightId = self`; items medios → `View.NO_ID`. Previene salto a otra categoría en bordes. |
| `mobile/.../res/layout/item_category.xml` | Agregados `paddingStart="48dp"`/`paddingEnd="48dp"` + `clipToPadding="false"` al `channelRecyclerView` para que items en bordes del carrusel se vean completos. |
| `mobile/.../adapters/CategoryAdapter.kt` | `addOnGlobalFocusChangeListener` con filtro `wasInside` (oldFocus en mismo RecyclerView?). Al entrar a una categoría desde otra: scroll categoría anterior a 0, scroll categoría nueva a 0, focus en item 0. No interfiere con navegación horizontal. |
| `mobile/.../MiTubeMobileApp.kt` | Inicialización de Coil: `ImageLoader` con caché de memoria (25% heap) + disco (50MB en `image_cache/`) |
| `mobile/.../res/drawable/ic_channel_placeholder.xml` | Vector drawable skeleton gris para placeholder de carga de iconos |
| `mobile/.../adapters/ChannelCardAdapter.kt` | Agregado `.placeholder(R.drawable.ic_channel_placeholder)` al request Coil |
| | |
| | |
| | |

---

## mitube.tizen — Tizen Web App para Samsung TV

### Stack

| Componente | Tecnología |
|---|---|
| Runtime | Tizen Web Runtime ≥5.5 (Chromium ~77, TVs 2020+) |
| App type | Packaged Web App (.wgt), sideload vía Tizen Studio |
| Lenguaje | TypeScript + HTML5 + CSS3, bundler Webpack |
| Player | Shaka Player 4.x (DASH) + hls.js (HLS) |
| DRM | Widevine Modular + PlayReady (EME nativo Tizen, sin proxy) |
| Streaming | MFG-DASH + HLS, resolución máxima FHD (1080p) |
| Backend | mitube.service sin cambios |

### Estructura del Proyecto

```
mitube.tizen/
├── config.xml                    ← Manifest Tizen (widget, privilegios: internet, input)
├── config.json                   ← Server URL (parametrizado, sin UI)
├── index.html                    ← Entry point, 3 views (login/browse/player/settings)
├── package.json                  ← shaka-player, hls.js, webpack, typescript
├── webpack.config.js             ← Entry: src/index.ts → dist/bundle.js
├── tsconfig.json
├── .gitignore
├── css/
│   ├── styles.css                ← Variables CSS, reset, toolbar, layout base
│   ├── login.css                 ← Login form (inputs, botón, error, wallpaper bg)
│   ├── browse.css                ← Category rows, horizontal scroll, cards
│   └── player.css                ← Video fullscreen, overlays, settings modal
├── src/
│   ├── index.ts                  ← Bootstrap: loadConfig (sync) → session check → router
│   ├── config.ts                 ← CONFIG.serverUrl hardcodeado (NO fetch — cuelga en Tizen)
│   ├── api/
│   │   └── client.ts             ← fetch() wrapper con JWT Bearer, login/getChannels
│   ├── models/
│   │   ├── Channel.ts            ← Interface + parseClearkeyLicense()
│   │   └── ChannelGroup.ts       ← Interface
│   ├── services/
│   │   ├── SessionStore.ts       ← localStorage (JWT, username, displayName)
│   │   ├── TokenService.ts       ← Port JS: bearer + CDN token (piratacodigo, cdn-token)
│   │   ├── MpdRewriter.ts        ← Port JS: XML rewrite + DRM detection
│   │   └── ImageCache.ts         ← XHR blob fetch + blob:// URL cache
│   ├── player/
│   │   └── PlayerSetup.ts        ← Shaka + hls.js setup, NetworkingEngine filters
│   └── ui/
│       ├── LoginView.ts          ← Login: readonly toggle pattern (arrow nav + IME handling)
│       ├── BrowseView.ts         ← Categorías + cards horizontales + dpad engine
│       ├── PlayerView.ts         ← Fullscreen video + overlay + controls
│       └── SettingsView.ts       ← Info sesión + botón "Cerrar sesión"
└── assets/icons/
    ├── icon.png                ← App icon (referenciado en config.xml)
    ├── wallpaper.png           ← Background login screen
    └── placeholder.png         ← Fallback para canales sin icono
```

### Paleta de Colores (exact-match Android TV)

| Token | Hex | Uso |
|---|---|---|
| `--bg-primary` | `#1c1c1c` | Fondos principales |
| `--bg-card` | `#2a2a2a` | Cards, toolbar |
| `--bg-input` | `#333333` | Input fields |
| `--bg-overlay` | `rgba(0,0,0,.5)` | Overlay player |
| `--border-input` | `#555555` | Borde inputs |
| `--accent` | `#005a9e` | Botones, focus ring |
| `--text-primary` | `#ffffff` | Títulos, toolbar, botones |
| `--text-secondary` | `#dddddd` | Nombre canal en card |
| `--text-muted` | `#888888` | Hints, contador |
| `--text-loading` | `#666666` | "Cargando..." |
| `--live-green` | `#4caf50` | Badge "EN VIVO" |
| `--live-bg` | `rgba(255,255,255,.2)` | Fondo badge |
| `--error-bg` | `rgba(220,30,30,.9)` | Banner error |
| `--player-bg` | `#000000` | Fondo video |

### Diseño Visual (Card)

```
Ancho: 210px, alto automático
Fondo: #2a2a2a (#222222 sin foco), border-radius: 8px
Border: 2px solid transparent (default) — evita desplazamiento al enfocar
Padding: 18px 12px
Gap entre cards: 8.57px
Icono: 120×120px, object-fit: contain
Nombre: 18px, #dddddd, centered, max 2 líneas con ellipsis
Focus: background #1a3a5c + border 2px solid var(--accent) + box-shadow intenso (NO outline)
```

### Configuración (server URL fija)

- `config.json` contiene la URL del servidor: `{ "serverUrl": "http://192.168.1.100:5000" }`
- ⚠️ **NO usar `fetch("config.json")`** — `fetch()` sobre archivos locales (file://) **cuelga indefinidamente** en Tizen Web Runtime. En su lugar, hardcodear la URL en `config.ts` o usar `XMLHttpRequest`.
- **No hay campo de server URL en el formulario de login**
- Para cambiar la URL: editar `config.ts` y recompilar

### Sesión Persistente (indefinida hasta logout explícito)

- JWT guardado en `localStorage`
- Al iniciar: si hay JWT → `validateToken()` → si válido, BrowseView directo
- Si el servidor responde 401 → `SessionStore.clear()` → LoginView
- Logout manual desde SettingsView → borra localStorage
- **No hay expiración forzada por tiempo**

### Flujo de Navegación

```
App launch → loadConfig() (sync, no fetch) → SessionStore.token existe?
  ├── Sí → setToken() → validateToken() → BrowseView
  └── No → LoginView (username + password)

Login (Visual-only Navigation Pattern — no spatial nav):
  ├── Inicio: focusIndex=0, clase .focused aplicada (sin foco nativo)
  ├── ⬆/⬇/⬅/➡ → cicla clase .focused entre campos (sin foco nativo)
  ├── Enter en campo → input.focus() nativo (modo edición)
  ├── En modo edición: flechas pasan al input (cursor movement)
  ├── Back/Escape/IME Done/Cancel → blur() + restore .focused visual
  ├── Enter en password → envía formulario
  └── Login OK → setToken() → SessionStore guarda JWT → BrowseView

BrowseView (keydown handler, no spatial nav):
  ⬆/⬇ → cambiar categoría
  ⬅/➡ → cambiar card + scroll horizontal programático
  Enter → PlayerView
  ⚙ → SettingsView

PlayerView:
  Enter → toggle overlay (autohide 4s)
  Back/Escape → BrowseView

SettingsView:
  Logout → SessionStore.clear() → LoginView
  Close/Back → BrowseView
```

#### Diferencias clave vs. Readonly Toggle Pattern original:
- **No se usa `readonly`** en inputs — el Samsung metadata `use.keypad.without.useraction=false` previene que el IME se abra al hacer `.focus()` programático
- **No se usa `document.body.focus()`** — el `<body>` no necesita foco porque el handler usa `document.addEventListener('keydown', handler, true)` (capture phase)
- **No se depende de Tizen spatial navigation** — en modelo 5300 (Tizen 5.x) la navegación espacial nativa NO funciona; el movimiento entre campos se maneja 100% con clases CSS `.focused`
- **Visual focus vía clase `.focused`** en lugar de `:focus` pseudo-clase (que no es confiable en Tizen)

### Proxy Flow en Tizen (Flow Channels)

En Tizen **no hay proxy local** (NanoHTTPd), pero los canales Flow requieren headers Origin/Referer/User-Agent que XHR no puede setear. La solución es **routing a través del backend** (`mitube.service`) en el mismo LAN:

```
TV (Shaka requestFilter)
  └→ request.uris[0] reescrito a http://backend:5241/api/proxy/fetch?url=<encoded_cdn_url>
  └→ headers: X-Proxy-Origin, X-Proxy-Referer, X-Proxy-User-Agent
  └→ Backend: HttpClient (TryAddWithoutValidation) → CDN con headers correctos
  └→ Backend: si es DASH manifest → inyecta <BaseURL>CDN_origin</BaseURL>
  └→ Shaka: resuelve segmentos relativos a BaseURL (CDN) → requestFilter → proxy otra vez
```

| Función | Android Proxy Local | Tizen (Flow) | Tizen (no-Flow) |
|---|---|---|---|
| Manifest fetch | Proxy reescribe MPD | Backend proxy + BaseURL injection | Directo (XHR) |
| Segment fetch | Proxy forwardea | Backend proxy | Directo (XHR) |
| License proxy | Proxy forwardea | Backend proxy POST | Directo (EME nativo) |
| Origin/Referer headers | Proxy local los inyecta | Backend HttpClient (sin restricciones) | N/A |
| player.html serving | Servido por proxy local | `<script>` tags estáticos | igual |

**Shaka requestFilter en `PlayerSetup.ts`**:

```typescript
// Flow channels: rewrite ALL Shaka requests to backend proxy
let originalUrl = request.uris[0];
// Guard: skip data:/blob: URIs
if (originalUrl.startsWith("data:") || originalUrl.startsWith("blob:")) return;

// Append CDN token to ALL requests (manifest already has it from loadDash,
// but segments resolved from MPD don't — CDN returns 403 "token required")
if (this.cdnToken && !originalUrl.includes("cdntoken=")) {
  const sep = originalUrl.includes("?") ? "&" : "?";
  originalUrl += `${sep}cdntoken=${this.cdnToken}`;
}

const proxyBase = CONFIG.serverUrl + "/api/proxy/fetch";
const h = channel.headers;
if (h["Origin"]) request.headers["X-Proxy-Origin"] = h["Origin"];
if (h["Referer"]) request.headers["X-Proxy-Referer"] = h["Referer"];
if (h["User-Agent"]) request.headers["X-Proxy-User-Agent"] = h["User-Agent"];
request.uris[0] = proxyBase + "?url=" + encodeURIComponent(originalUrl);
```

**Problema crítico resuelto: resolución de URLs relativas en Shaka**

Cuando Shaka obtiene un manifest a través del proxy (`http://backend/api/proxy/fetch?url=...`), resuelve los segmentos relativos (ej. `DSports_1-avc1_...mp4`) contra la URL base del proxy, resultando en `http://backend/api/proxy/DSports_1-...mp4` (ruta inválida → 404).

**Solución: Backend inyecta `<BaseURL>` en el MPD**. El `ProxyController` bufferiza el MPD, extrae la URL base del CDN desde el parámetro `url`, e inserta `<BaseURL>https://cdn-origin/path/</BaseURL>` después del tag `<MPD ...>`. Shaka entonces resuelve segmentos contra la CDN, y el requestFilter los redirige al proxy de nuevo.

```csharp
// ProxyController.cs — ExtractCdnBaseUrl helper
private static string ExtractCdnBaseUrl(string url)
{
    if (!Uri.TryCreate(url, UriKind.Absolute, out var uri)) return "";
    var path = uri.AbsolutePath;
    var lastSlash = path.LastIndexOf('/');
    if (lastSlash > 0)
        return $"{uri.Scheme}://{uri.Host}{path[..(lastSlash + 1)]}";
    return $"{uri.Scheme}://{uri.Host}/";
}
```

Inyección solo para DASH manifests (Content-Type contiene `dash+xml` o `mpd`). Segmentos/licencias binarias se streamean sin modificar. Ver `mitube.service/Controllers/ProxyController.cs:100-126`.

### Ports (Kotlin → TypeScript)

| Android Kotlin | Tizen TypeScript |
|---|---|
| `TokenService.kt` | `TokenService.ts` — bearer + CDN token |
| `MpdRewriter.kt` | `MpdRewriter.ts` — XML parse + DRM detect |
| `SessionManager.kt` | `SessionStore.ts` — localStorage |
| `Channel.kt` / `ChannelGroup.kt` | `Channel.ts` / `ChannelGroup.ts` — interfaces |
| `ApiClient.kt` + `MitubeApi.kt` | `client.ts` — fetch + JWT |
| `LoginActivity.kt` | `LoginView.ts` — sin campo server URL, arrow key nav (⬆⬇ entre campos), Enter submit |
| `MainBrowseFragment.kt` | `BrowseView.ts` — dpad navigation engine |
| `CardPresenter.kt` | CSS `.channel-card` + focus styles (background/border/shadow, no outline) |
| `PlayerActivity.kt` | `PlayerView.ts` — Shaka + overlay |
| `MainBrowseActivity.kt` | `index.ts` — bootstrap + router |

### Control Remoto (Teclas Tizen)

| Tecla | KeyCode | Acción en Login | Acción en Browse | Acción en Player |
|---|---|---|---|---|
| ⬆ | 38 | Campo anterior | Cambiar categoría arriba | — |
| ⬇ | 40 | Campo siguiente | Cambiar categoría abajo | — |
| ⬅ | 37 | Campo anterior | Card anterior | — |
| ➡ | 39 | Campo siguiente | Card siguiente | — |
| Enter | 13 | Editar campo / Submit | Abrir Player | Toggle overlay |
| Back | 10009 / 27 | Salir del IME | Volver | Volver a Browse |
| IME Done | 65376 | Cerrar teclado, confirmar | — | — |
| IME Cancel | 65385 | Cerrar teclado, cancelar | — | — |

### Dependencias npm

```json
{
  "dependencies": {
    "shaka-player": "^4.3.0",
    "hls.js": "^1.5.0"
  },
  "devDependencies": {
    "typescript": "^5.4.0",
    "webpack": "^5.90.0",
    "webpack-cli": "^5.1.0",
    "ts-loader": "^9.5.0"
  }
}
```

> **Nota:** `tizen-tv-webapis@^3.0.0` y `@types/tizen-tv-webapis@^1.0.0` no existen en npm. Si se necesitan webapis de Tizen, buscar el package correcto o usar declaraciones de tipo manuales.

### Webpack: Entry Points Separados (requerido para Tizen)

shaka-player y hls.js **deben ir en entry points separados** (NO lazy chunks) porque los scripts creados dinámicamente con `import()` no pueden cargar archivos locales (file://) en Tizen Chromium 77. Usar `module: "es2020"` en tsconfig para compatibilidad.

**Configuración requerida en `tsconfig.json`**:
```json
{
  "compilerOptions": {
    "module": "es2020",
    "lib": ["ES2020", "DOM"]
  }
}
```

**En `PlayerView.ts`, usar referencia global en vez de `import` estático o dinámico**:
```typescript
// ❌ No: import { PlayerSetup } from "../player/PlayerSetup";
// ❌ No: const { PlayerSetup } = await import("../player/PlayerSetup");
// ✅ Sí: referencia global (PlayerSetup se expone en window por player.bundle.js)
const PlayerSetup = (window as any).MiTubePlayer;
this.player = new PlayerSetup(this.video);
```

**Resultado del build (entry points separados, sin lazy chunks):**
| Archivo | Tamaño | Contenido |
|---|---|---|
| `dist/bundle.js` | 36.8 KiB | Entry point, routing, login, browse, settings, ImageCache |
| `dist/player.bundle.js` | 2.01 MiB | PlayerSetup + shaka-player + TokenService (unminified) |

### config.xml — Requisitos Críticos (Samsung TV)

```xml
<?xml version="1.0" encoding="UTF-8"?>
<widget xmlns="http://www.w3.org/ns/widgets" xmlns:tizen="http://tizen.org/ns/widgets"
        version="1.0.0" viewmodes="maximized">
    <tizen:application id="XXXXXXXXXX.AppName" package="XXXXXXXXXX" required_version="3.0"/>
    <tizen:profile name="tv"/>
    <feature name="http://tizen.org/feature/screen.size.all"/>
    <content src="index.html"/>
    <icon src="assets/icons/icon.png"/>
    <name>AppName</name>
    <tizen:privilege name="http://tizen.org/privilege/internet"/>
    <tizen:privilege name="http://tizen.org/privilege/tv.inputdevice"/>
    <tizen:setting screen-orientation="landscape" context-menu="enable" 
                   background-support="disable" hwkey-event="enable"/>
    <tizen:metadata key="http://samsung.com/tv/metadata/use.keypad.without.useraction" value="false"/>
</widget>
```

| Atributo | Regla | Notas |
|---|---|---|
| `package` | **Exactamente 10 caracteres** alfanuméricos `[0-9,a-z,A-Z]` | Si tiene 11+ falla con `[118, -19] Parsing error` |
| `id` (application) | `{package}.{AppName}` | Debe coincidir con el package |
| `required_version` | `3.0` o superior | Coincidir con API del TV; `2.4` también funciona en TVs modernas |
| `tizen:profile` | `name="tv"` | **Obligatorio** para TV; sin esto falla instalación |
| `viewmodes` | `maximized` | Necesario para pantalla completa |
| `feature screen.size.all` | | Requerido para TV profile |
| `privilege internet` | | Necesario para fetch() |
| `privilege tv.inputdevice` | | Para registrar keys no-mandatorias (color buttons, media keys) |
| `hwkey-event="enable"` | | Habilita hardware key events — **crítico** para navegación |
| `use.keypad.without.useraction=false` | Samsung metadata | Previene que el IME se abra automáticamente al hacer `.focus()` programático en `<input>` |

**Error `[118, -19] Parsing error`** → causas más comunes:
1. `package` no tiene exactamente 10 caracteres
2. Falta `<tizen:profile name="tv"/>`
3. Falta `<feature name="http://tizen.org/feature/screen.size.all"/>`

### Node.js / npm

| Propiedad | Valor |
|---|---|
| Ruta | `D:\Software\nodejs\npm.cmd` |
| Node | v24.16.0 |
| npm | 11.17.0 |
| PATH en deploy.ps1 | `$env:PATH = "D:\Software\nodejs;$env:PATH"` |

> npm y node **no están en el PATH global del sistema**. Todos los scripts deben usar la ruta completa `D:\Software\nodejs\npm.cmd` o anteponer `D:\Software\nodejs` al PATH.

### Build y Deploy (Samsung TV)

Usar `deploy.ps1` (push-button, todo-en-uno):

```powershell
& "D:\Sources\amaillo\utilities\Dash MPD Player\mitube.tizen\deploy.ps1"
```

El script `deploy.ps1` automatiza los 5 pasos:

| Paso | Comando | Descripción |
|---|---|---|
| 1 | webpack build | `npm run build` → `dist/bundle.js` (36.8 KiB) + `dist/player.bundle.js` (2.01 MiB) |
| 2 | tizen build-web | `tizen build-web` → `.buildResult/` |
| 3 | Limpiar node_modules | Elimina `node_modules/` del `.buildResult/` (~34 MB si no se borra) |
| 4 | Firmar y empaquetar | `tizen package -t wgt -s ADn` → `MiTube.wgt` |
| 5 | Instalar en TV | `tizen install -s 192.168.0.121:26101` |

**Manual paso a paso:**
```powershell
$node = "D:\Software\nodejs\npm.cmd"
$tizen = "D:\Software\Tizen\TizenStudio\tools\ide\bin\tizen"
$tv = "192.168.0.121:26101"
$root = "D:\Sources\amaillo\utilities\Dash MPD Player\mitube.tizen"
$env:PATH = "D:\Software\nodejs;$env:PATH"

# 1. Build webpack
& $node run build

# 2. Build Tizen
Remove-Item "$root\.buildResult" -Recurse -Force -ErrorAction SilentlyContinue
& $tizen build-web -- $root

# 3. Limpiar node_modules (NO excluido automáticamente)
Remove-Item -LiteralPath "$root\.buildResult\node_modules" -Recurse -Force -ErrorAction SilentlyContinue

# 4. Firmar y empaquetar .wgt
& $tizen package -t wgt -s ADn -- "$root\.buildResult"

# 5. Sideload al TV
& $tizen install -s $tv -n "$root\.buildResult\MiTube.wgt"
```

> **Importante:** `tizen build-web` excluye automáticamente: `.build/*`, `.sign/*`, `webUnitTest/*`, `.externalToolBuilders/*`, `.buildResult/*`, `.settings/*`, `.package/*`, `.tproject`, `.project`, `.sdk_delta.info`, `.rds_delta`, `*.wgt`, `.tizen-ui-builder-tool.xml`. **NO excluye** `node_modules/`, `src/`, `dist/`, etc. Los entry points (`bundle.js`, `player.bundle.js`) están en `dist/` y se copian automáticamente al `.buildResult`.

### Troubleshooting — Instalación en TV

| Problema | Causa | Solución |
|---|---|---|
| Problema | Causa | Solución |
|---|---|---|---|
| `install failed[118, -19] Parsing error` | `package` != 10 chars, falta `<tizen:profile name="tv"/>` o falta `<feature name="http://tizen.org/feature/screen.size/all">` | Verificar config.xml contra la tabla de requisitos |
| `install failed[118, -12]` | Error de certificado | Re-crear certificados y hacer `install-permit` |
| `install failed[118, -14]` | Privilegio restringido con cert público | Quitar privilegios `partner`/`platform` del config.xml |
| `sdb install` no funciona para .wgt | `sdb install` solo soporta `.tpk` | Usar `tizen install` |
| Package no se instala (sin error claro) | TV no tiene "Permit to install" | `tizen install-permit -s <tv-ip>:26101` |
| `""node"" not recognized` al hacer `npm run build` | `node` no está en PATH | Anteponer `D:\Software\nodejs` al PATH con `$env:PATH = "D:\Software\nodejs;$env:PATH"` |
| Backend devuelve 500 al correr DLL directo | `JwtSettings` es null porque no carga `appsettings.json` | Usar `dotnet run --project` en vez de `dotnet <dll>` para que use `launchSettings.json` |
| Backend devuelve 500 al correr DLL directo | `JwtSettings` es null porque no carga `appsettings.json` | Usar `dotnet run --project` en vez de `dotnet <dll>` para que use `launchSettings.json` |

**Comandos útiles SDB:**
```powershell
# Listar apps instaladas
& "D:\Software\Tizen\TizenStudio\tools\sdb.exe" shell 0 applist

# Obtener DUID del TV
& "D:\Software\Tizen\TizenStudio\tools\sdb.exe" shell 0 duid

# Push manual de archivo
& "D:\Software\Tizen\TizenStudio\tools\sdb.exe" push <local> <remote>

# Conectar al TV
& "D:\Software\Tizen\TizenStudio\tools\sdb.exe" connect 192.168.0.121
```

**Certificados:**
- Perfil `ADn` en `C:\Users\Inspiron15\SamsungCertificate\ADn\`
- `device-profile.xml` incluye DUID del TV (`CPCFC353V7B5G`)
- Creado via Samsung Certificate Manager con cuenta `mailloa@gmail.com`
- `tizen_public_signer.p12` y `tizen_partner_signer.p12` en `D:\Software\Tizen\TizenStudio\tools\tizen-core\certificates\` (password desconocida para estos certs built-in)

### Control Remoto — Samsung TV (Investigación Completa)

#### El Problema Central

Cuando un `<input>` recibe foco nativo en Tizen TV, el OS **abre automáticamente el teclado en pantalla (IME)**. Mientras el IME está abierto, las teclas de flecha son consumidas por el IME para mover el cursor — los handlers `keydown` de la app no los reciben correctamente en TVs Tizen 5.x+.

#### Por Qué Fallan los Enfoques Comunes

| Enfoque | Por qué falla en Tizen |
|---|---|
| `document.addEventListener('keydown')` con `preventDefault()` | Samsung dice explícitamente: "No llamar `preventDefault()` en keydown para elementos input" |
| `stopPropagation()` en capture phase | El IME intercepta las teclas antes de que lleguen al JS |
| Focus management nativo con `.focus()` | Abre el IME, bloqueando flechas |
| `:focus` CSS | No funciona confiablemente en Tizen Web Runtime |
| `tabindex` en inputs | No resuelve el conflicto IME vs navegación |

#### La Solución: "Readonly Toggle Pattern" (Jellyfin + Samsung Samples)

El patrón probado en producción (Jellyfin, TizenPortal, Samsung SampleWebApps-IME):

1. **Inputs empiezan `readonly`** — bloquea el IME/teclado de abrir
2. **Modo Navegación** — flechas ciclan entre campos (sin foco nativo)
3. **Visual focus** — clase CSS `.focused` con `box-shadow` (no `:focus`)
4. **Enter en campo** → remove `readonly`, llama `input.focus()` → IME abre para escribir
5. **Back/Escape/IME Done/Cancel** → restaura `readonly`, blur, `document.body.focus()`
6. **Flechas en modo edición** — pasan al input (movimiento de cursor)

```typescript
// Patrón core (simplificado)
private enterEditMode(): void {
  const input = this.fields[this.focusIndex] as HTMLInputElement;
  this.isEditMode = true;
  input.removeAttribute("readonly");
  input.focus();  // Abre IME para escribir
}

private exitEditMode(): void {
  this.isEditMode = false;
  this.usernameInput.setAttribute("readonly", "readonly");
  this.passwordInput.setAttribute("readonly", "readonly");
  if (document.activeElement instanceof HTMLElement) {
    document.activeElement.blur();
  }
  document.body.focus();  // CRÍTICO: restaura navegación
}
```

#### Requisitos Críticos (Samsung Official)

| Requisito | Fuente | Detalle |
|---|---|---|
| `document.body.focus()` al inicio | Samsung SampleWebApps-IME | Sin esto, keydown events no se disparan |
| `blur()` + `document.body.focus()` después de IME | Samsung IME sample | Restaura navegación después de cerrar teclado |
| **Nunca** `preventDefault()` en keydown cuando input tiene foco | Samsung Developer docs | Rompe elementos input completamente |
| `<tizen:setting hwkey-event="enable"/>` | Samsung config docs | Habilita hardware key events |
| `tv.inputdevice` privilege | Samsung Remote Control guide | Para registrar keys no-mandatorias |

#### Teclas Mandatorias (auto-registradas, sin `registerKey()`)

| Tecla | KeyCode | Comportamiento |
|---|---|---|
| ArrowLeft | 37 | Mover cursor izquierda (en input) / navegar izquierda |
| ArrowUp | 38 | Mover cursor arriba (en input) / navegar arriba |
| ArrowRight | 39 | Mover cursor derecha (en input) / navegar derecha |
| ArrowDown | 40 | Mover cursor abajo (en input) / navegar abajo |
| Enter | 13 | Seleccionar / Abrir IME |
| Back | 10009 | Volver / Cerrar IME |

#### Teclas IME (específicas Samsung)

| Tecla | KeyCode | Acción |
|---|---|---|
| Done (IME) | 65376 | Cierra teclado, confirma texto |
| Cancel (IME) | 65385 | Cierra teclado, cancela |

#### Diferencias por Año de TV

| Año | Comportamiento |
|---|---|
| 2016 | Flechas NO se bloquean cuando IME abierto — navegación se dispara mientras el usuario intenta escribir (bug conocido, sin workaround oficial) |
| 2017+ | Flechas SÍ se bloquean cuando IME abierto — correcto comportamiento |
| Tizen 5.x (2020+) | Chromium ~77, soporta EME nativo, MSE limitado a FHD |

#### Config.xml — Configuración Requerida

```xml
<tizen:privilege name="http://tizen.org/privilege/tv.inputdevice"/>
<tizen:setting screen-orientation="landscape" 
               context-menu="enable" 
               background-support="disable" 
               hwkey-event="enable"/>
```

#### Referencias de Código (GitHub)

| Repo | Qué muestra |
|---|---|
| `SamsungDForum/SampleWebApps-IME` | Official Samsung IME + navigation: `document.body.focus()`, blur+refocus, IME key codes |
| `gifflet/tizen-user-app-demo` | Login form: cycle-forward focus, no preventDefault, simple keydown handler |
| `axelnanol/tizenportal` | D-pad spatial nav + text input protection (readonly until Enter) |
| `jellyfin/jellyfin-web` | Production Tizen app: readonly toggle pattern, comprehensive key mapping |
| `NoriginMedia/Norigin-Spatial-Navigation` | React spatial nav library (441 stars), `shouldFocusDOMNode` for inputs |
| `christopherklint97/streamvault` | React Tizen IPTV: `useFocusNavigation` hook |

#### Samsung Design Guidelines (Navegación)

- Focus **NO** loop en listas — para en primer/último item
- Al mover de categoría a contenido → primer item recibe focus
- Al volver de contenido a categoría → categoría actual recibe focus
- Back debe funcionar desde **todas** las pantallas (QA check explícito)

### Limitaciones Conocidas (Tizen)

1. MSE limitado a FHD (1080p) en Tizen Web Runtime. No soporta UHD/4K.
2. Service Workers no disponibles en Tizen Web (file:// origin).
3. localStorage no es cifrado (no hay equivalente a EncryptedSharedPreferences).
4. Sin app store pública sin cuenta Samsung Seller Office.
5. Flow channels: los headers Origin/Referer se inyectan vía Shaka requestFilter, no hay proxy local para casos extremos.
6. Ciclo de vida TV: la app puede ser suspendida al apagar el TV. La sesión persiste en localStorage al reabrir.
7. `tizen build-web` **NO excluye** `node_modules/` del `.buildResult` — hay que borrarlo manualmente antes de empaquetar o el .wgt se hincha a ~34 MB.
8. `package` en config.xml debe tener **exactamente 10 caracteres alfanuméricos**. 11+ produce `[118, -19] Parsing error`.
9. El elemento `<tizen:profile name="tv"/>` es **obligatorio** para que la TV acepte la instalación.
10. Login form requiere "readonly toggle pattern" para navegar entre campos — el IME intercepta flechas cuando inputs tienen foco nativo.
11. `:focus` CSS no funciona confiablemente en Tizen — usar clase `.focused` con `box-shadow` en su lugar.
12. En TVs 2016, las flechas no se bloquean cuando IME abierto — bug conocido sin workaround.
13. `document.body.focus()` es **crítico** al inicio y después de cerrar IME — sin esto, keydown events no se disparan.
14. **Nunca** llamar `preventDefault()` en keydown cuando un input tiene foco — rompe la funcionalidad del input según Samsung docs oficiales.
15. **No usar webpack lazy chunks (`import()` dinámico)** en Tizen Chromium 77 — los scripts creados dinámicamente con `document.createElement('script')` no pueden cargar archivos locales (file://). En su lugar, usar **entry points separados** cargados con `<script>` tags estáticos en el HTML. Ver solución completa más abajo.
16. **`fetch()` está completamente roto en Tizen Chromium 77** — devuelve objetos Response falsos con status 200, body vacío, y URL `about:blank`. Incluso para IPs inexistentes (`192.168.0.254:9999`) devuelve `status=200 type=cors`. **No usar `fetch()` para nada** en Tizen. Usar `XMLHttpRequest` con `onreadystatechange` en su lugar.
17. **`XMLHttpRequest` con `onreadystatechange` funciona** en Tizen. Probar siempre con `onreadystatechange` en vez de `onload`, ya que `onload` puede dispararse antes de que `responseText` esté poblado.
18. **`fetch("config.json")` cuelga indefinidamente** en Tizen para archivos locales dentro del .wgt. No usar `fetch()` para archivos empaquetados — hardcodear URLs o usar `XMLHttpRequest`.
19. **Spatial navigation nativa de Tizen NO funciona** en modelo 5300 (Tizen 5.x). No se puede depender de ella para mover foco entre elementos. Toda la navegación debe ser manejada vía JavaScript con clases CSS `.focused`.
20. **`defer` en scripts no es confiable** en Tizen Web Runtime. Usar `<script src="...">` sin `defer`/`async` cuando sea crítico que el script se ejecute.
21. **El minificador (terser) puede producir sintaxis `?.1`** al convertir `0.1` a `.1` después de un ternario: `a===void 0?.1:a`. Chrome 77 interpreta `?.` como optional chaining aunque sea `?` ternario + `.1` número. **Solución**: deshabilitar minificación (`optimization: { minimize: false }` en webpack) o parchear shaka-player (`?.1` → `?0.1`).
22. **shaka-player compiled.js contiene `?.1`** en su código fuente compilado. Parchear antes del build: buscar `?.1(` y reemplazar con `?0.1(`.
23. **Shaka Player usa `fetch()` internamente** en Tizen (falla porque `fetch()` está roto). **Solución**: eliminar `window.fetch` (setear a `undefined`) antes de inicializar shaka para que use `XMLHttpRequest` automáticamente.
24. **hls.js también usa `fetch()` internamente**. Alternativa en Tizen: usar HLS nativo del TV (`<video>` con src directa a .m3u8, sin hls.js).
25. **TokenService (bearer + CDN token) debe usar XHR**, no fetch.
26. **SDB shell no soporta comandos Unix** (`echo`, `ls`, `cat`, `ps`). Solo comandos Tizen como `applist`, `duid`, `shell 0 applist`. `dlog` no retorna output en este modelo.
27. **Chrome DevTools via port forwarding (9222) no responde** en modelo 5300 — usar puerto 45102 (ver Remote Debugging más abajo).
28. **El inspector web** (`web-inspector="enable"` en config.xml) No habilita DevTools funcional en este modelo.

29. **BrowseView keydown listeners se acumulan** en cada `show()`. Trackear `keyboardHandler` y remover viejo listener en `hide()` y antes de `bindKeyboard()`. Sin esto, arrow keys saltan 2+ cards porque múltiples listeners avanzan el índice.
30. **PlayerSetup NO debe recrearse entre plays** — `shakaPlayer.destroy()` es async; recrear produce race condition en el mismo video element. Lazy-init una vez, reusar via `shakaPlayer.unload()`.
31. **ImageCache evita re-descarga de iconos**: usar XHR con `responseType: 'blob'` + `URL.createObjectURL()`. Cache en `Map<string, string>`. `getSync()` para hit sincrónico en re-renders.
32. **placeholder.png debe existir físicamente** en `assets/icons/`. Si falta, el `onerror` del `<img>` loop infinito (carga placeholder → error → cambia src a placeholder otra vez). Guard: solo intentar placeholder una vez chequeando `this.src`.
33. **CDN token debe ir en TODOS los requests Flow**, no solo el manifest. Guardar como field en PlayerSetup, appendear `?cdntoken=` en requestFilter antes de reescribir al proxy.
34. **requestFilter debe skipear `data:` y `blob:` URIs** — Shaka, hls.js y DRM generan estos internamente. Si pasan al proxy, el backend lanza `NotSupportedException: The 'data' scheme is not supported`.
35. **`* { outline: none; }` global** — Tizen Web Runtime dibuja outline por defecto en elementos enfocados. Para navegación CSS-based (`.focused` class), outline interfiere visualmente. Reemplazar con `border` + `box-shadow`.
36. **Default card border** — poner `border: 2px solid transparent` en cards default; así al enfocar (con `border: 2px solid accent`) no hay desplazamiento del layout.
37. **Debug port varía por instancia** — `sdb shell 0 debug` devuelve un puerto distinto (ej: `33558`) cada vez que la app se lanza. Leer el output y forwardear `tcp:45102 → tcp:<DEVICE_PORT>` en vez de asumir 45102.

### Remote Debugging — Tizen 5300

En modelo 5300, DevTools NO funciona con el método estándar (port forwarding 9222 + chrome://inspect). Sin embargo, se puede conectar usando **Chromium 88**:

**Procedimiento validado:**

```powershell
# 1. Cerrar la app en el TV manualmente
# 2. Conectar SDB
& "D:\Software\Tizen\TizenStudio\tools\sdb.exe" connect 192.168.0.121

# 3. Habilitar debug mode en la app (NOTA: el puerto varía, ej: port: 33558)
& "D:\Software\Tizen\TizenStudio\tools\sdb.exe" shell 0 debug miTubeTV10.MiTube

# 4. Forward puerto 45102 → al puerto real de debug (leer del output del paso 3)
#    Si el output dice "port: 33558":
#    & "D:\Software\Tizen\TizenStudio\tools\sdb.exe" forward tcp:45102 tcp:33558

# 5. Abrir Chromium 88 (NO Chrome estándar)
#    Ruta: C:\Temp\ungoogled-chromium-88.0.4324.190-2_Win64\chrome.exe
# 6. Navegar a chrome://inspect
# 7. Agregar "localhost:45102" en "Discover network targets"
# 8. Aparece "MiTube TV" en Remote Target → click "inspect"
```

**Nota:** El debug port cambia cada vez que se lanza la app. Leer el puerto del output de `sdb shell 0 debug` y forwardear `tcp:45102` → `tcp:<PORT_DEVICE>`.

**Requisitos:**
- App instalada y ABIERTA (con `debug` habilitado) en el TV
- Chromium 88 específicamente (versiones más nuevas no funcionan con Tizen 5.x)
- `web-inspector="enable"` en config.xml (aunque no funcione solo, es necesario)

**Nota:** Si se cierra la app en el TV, hay que repetir los pasos 1-4.

### Solución Completa — Reproducción de Video en Tizen Chromium 77

#### Arquitectura de Build (webpack)

En vez de usar webpack lazy chunks (que NO funcionan en Tizen), usar **entry points separados**:

```javascript
// webpack.config.js
module.exports = {
  entry: {
    bundle: './src/index.ts',                    // App principal (17 KB)
    'player.bundle': './src/player/PlayerSetup.ts', // Shaka + hls.js (~2 MB)
  },
  output: {
    filename: '[name].js',
    path: path.resolve(__dirname, 'dist'),
    clean: true,
  },
  optimization: {
    minimize: false,  // ← CRÍTICO: evita sintaxis ?.1 que Chrome 77 no parsea
  },
  target: 'web',
};
```

En `index.html`:
```html
<script src="dist/bundle.js?v=5"></script>
<script src="dist/player.bundle.js"></script>   <!-- SIN defer, SIN async -->
```

**Orden crítico**: `bundle.js` primero, `player.bundle.js` después (síncrono, bloqueante). No usar `defer`.

#### PlayerSetup — Exposición Global

`PlayerSetup.ts` expone la clase en `window`:
```typescript
(window as any).MiTubePlayer = PlayerSetup;
```

`PlayerView.ts` la referencia directamente (sin dynamic import):
```typescript
const PlayerSetup = (window as any).MiTubePlayer;
this.player = new PlayerSetup(this.video);
```

#### Forzar XHR en Shaka Player

En el constructor de `PlayerSetup`, ELIMINAR `window.fetch` ANTES de inicializar shaka. Esto fuerza a shaka a usar `XMLHttpRequest` automáticamente:

```typescript
constructor(videoElement: HTMLVideoElement) {
    this.video = videoElement;
    // Tizen: fetch() está roto. Forzar shaka a usar XHR.
    try { (window as any).fetch = undefined; } catch (_) {}
    shaka.polyfill.installAll();
    if (shaka.Player.isBrowserSupported()) {
        this.shakaPlayer = new shaka.Player(this.video);
        this.registerShakaFilters();
    }
}
```

#### Parchear shaka-player.compiled.js

El source compilado de shaka contiene `?.1` que Chrome 77 no parsea. Parchear después de npm install:

```powershell
$file = "node_modules\shaka-player\dist\shaka-player.compiled.js"
$content = [System.IO.File]::ReadAllText($file)
$newContent = $content -replace '\?\.1(\W)', '?0.1$1'
[System.IO.File]::WriteAllText($file, $newContent, [System.Text.UTF8Encoding]::new($false))
```

O incluirlo como build step en `package.json`:
```json
"scripts": {
    "postinstall": "powershell -Command \"...\"",
    "build": "webpack --mode production"
}
```

#### TokenService con XHR

Todas las llamadas a `fetch()` en `TokenService` deben reemplazarse por `XMLHttpRequest`:

```typescript
function xhrFetchText(url: string, headers?: Record<string, string>): Promise<string> {
  return new Promise((resolve, reject) => {
    const xhr = new XMLHttpRequest();
    xhr.open("GET", url, true);
    xhr.timeout = 30000;
    if (headers) {
      for (const [k, v] of Object.entries(headers)) {
        xhr.setRequestHeader(k, v);
      }
    }
    xhr.onreadystatechange = () => {
      if (xhr.readyState !== 4) return;
      const text = xhr.responseText || "";
      if (xhr.status >= 200 && xhr.status < 300) resolve(text);
      else reject(new Error(`HTTP ${xhr.status}: ${text.substring(0, 100)}`));
    };
    xhr.onerror = () => reject(new Error("XHR connection failed"));
    xhr.ontimeout = () => reject(new Error("XHR timeout"));
    xhr.send();
  });
}
```

**Importante**: usar `onreadystatechange` (NO `onload`). En Tizen, `onload` puede dispararse antes de que `responseText` esté disponible.

#### client.ts — API Calls con XHR

Todas las llamadas HTTP (login, channels, validate) deben usar `XMLHttpRequest` con `onreadystatechange`:

```typescript
function xhrRequest<T>(method: string, path: string, body?: string): Promise<T> {
  const url = baseUrl() + path.replace(/^\//, "");
  return new Promise((resolve, reject) => {
    const xhr = new XMLHttpRequest();
    xhr.open(method, url, true);
    xhr.timeout = 15000;
    xhr.setRequestHeader("Content-Type", "application/json");
    if (authToken) xhr.setRequestHeader("Authorization", `Bearer ${authToken}`);
    xhr.onreadystatechange = () => {
      if (xhr.readyState !== 4) return;
      const text = xhr.responseText || "";
      if (xhr.status >= 200 && xhr.status < 300) {
        if (!text) return reject(new Error("Empty response body"));
        try { resolve(JSON.parse(text) as T); }
        catch (e: any) { reject(new Error(`JSON parse error: ${text.substring(0, 100)}`)); }
      } else {
        reject(new ApiError(xhr.status, text.substring(0, 200) || `HTTP ${xhr.status}`));
      }
    };
    xhr.send(body || undefined);
  });
}
```

#### Resumen de Problemas y Soluciones

| Problema | Síntoma | Causa | Solución |
|---|---|---|---|---|
| `fetch()` devuelve 200 vacío | Login/ping fallan con "Empty body" | Tizen fetch() bug | Usar XHR |
| `player.bundle.js` no carga | "Loading chunk 943 failed" | webpack lazy chunks no soportados en file:// | Entry points separados |
| `SyntaxError: Unexpected token ?` | "MiTubePlayer no disponible" | minificador produce `?.1`, Chrome 77 no lo parsea | `minimize: false` + parchear shaka |
| `fetch` no funciona en shaka | "Error DASH: Failed to execute fetch" | shaka usa fetch internamente | `window.fetch = undefined` antes de init |
| `TokenService` falla | Error en bearer/CDN token | fetch en TokenService | TokenService con XHR |
| Shaka resuelve segmentos contra proxy | 404 en segmentos como `/api/proxy/DSports_1-...` | Shaka resuelve URLs relativas contra base del manifest URL (proxy) | Backend inyecta `<BaseURL>` en el MPD |
| `data:` scheme en proxy | 500 "The 'data' scheme is not supported" | requestFilter no filtra data:/blob: URIs | Skip `data:`, `blob:` URIs en requestFilter |
| Segmentos sin CDN token | 403 "token required" del CDN | Sólo el manifest lleva `?cdntoken=`, segmentos resueltos del MPD no | requestFilter append `cdntoken` a todos los requests Flow antes de codificar para proxy |
| Flechas skip 2+ cards en Browse | Navegación errática en browse | `bindKeyboard()` agrega listener nuevo en cada `show()` | Track `keyboardHandler` ref, remove old listener en `hide()` y antes de `bindKeyboard()` |
| Pantalla negra al segundo playback | Video no se reproduce la 2da vez | `PlayerSetup` destruido y recreado, `shakaPlayer.destroy()` async causa race condition | Lazy-init una vez, reusar via `async unload()` con `shakaPlayer.unload()`, nunca destroy/recreate |
| Iconos no cargan por placeholder faltante | Card muestra icono roto, error loop | `assets/icons/placeholder.png` no existe físicamente | Crear archivo placeholder.png + guard `onerror` para evitar re-trigger |

### Lecciones Aprendidas — Debugging en Tizen 5300

Debido a que SDB shell, dlog, y DevTools (std) no funcionan en este TV, los métodos de debugging son:

1. **Debug overlay inline** — colocar un script sincrónico en `index.html` (ANTES del bundle) que escriba a un `<div>` visible en pantalla. Así se puede ver en el TV si JS ejecuta.
2. **Marcador de módulo** — agregar un IIFE al inicio del entry point que escriba al DOM directamente para confirmar que el bundle se evaluó.
3. **`document.addEventListener('keydown', handler, true)`** para capturar teclas — usar `capture: true` y probar en TV si los eventos llegan.
4. **Descartar causas una por una** — reducir el bundle quitando dependencias grandes (shaka/hls) para aislar el crash.
5. **Observar la pantalla del TV** — los mensajes en el overlay de debug son la primera fuente de información.
6. **Remote DevTools con Chromium 88** — ver sección "Remote Debugging — Tizen 5300" más arriba. Es el método más efectivo una vez configurado.

**Nota:** `sdb shell 0 applist` funciona, pero `dlog`, `echo`, `ls`, `cat`, `ps` NO funcionan en este modelo.

### TV de Prueba

| Propiedad | Valor |
|---|---|
| IP | `192.168.0.121` |
| Puerto SDB | `26101` |
| Modelo | 5300 (Tizen 5.x) |
| DUID | `CPCFC353V7B5G` |
| Developer Mode | Activado |
| App ID | `miTubeTV10.MiTube` |
| Apps sideloaded | MiTube, Stremio, Plex, Pluto TV, HBO Max, Disney+, YouTube |
| Backend IP (dev) | `192.168.0.173` |
| Backend port | `5241` |

### deploy.ps1

El script `D:\Sources\amaillo\utilities\Dash MPD Player\mitube.tizen\deploy.ps1` automatiza build + package + install:

```powershell
# Uso
& "D:\Sources\amaillo\utilities\Dash MPD Player\mitube.tizen\deploy.ps1"

# Parámetros opcionales
& "D:\Sources\amaillo\utilities\Dash MPD Player\mitube.tizen\deploy.ps1" -TvAddress "192.168.0.121:26101"
```

El script requiere `D:\Software\nodejs` en PATH o usa la ruta completa internamente.

### Para iniciar el backend

```powershell
# Lanzamiento (desde cualquier directorio)
dotnet run --project "D:\Sources\amaillo\utilities\Dash MPD Player\mitube.service"

# Para ventana visible (recomendado durante desarrollo, log en tiempo real):
Start-Process -WindowStyle Normal -FilePath powershell -ArgumentList "-NoExit -Command dotnet run --project 'D:\Sources\amaillo\utilities\Dash MPD Player\mitube.service'"

# Para que corra en segundo plano (ventana oculta):
Start-Process -WindowStyle Hidden -FilePath powershell -ArgumentList "-Command dotnet run --project 'D:\Sources\amaillo\utilities\Dash MPD Player\mitube.service'"
```

El backend escucha en `http://0.0.0.0:5241`, accesible desde el TV como `http://192.168.0.173:5241`. Login: `adn` / `123456La`.
