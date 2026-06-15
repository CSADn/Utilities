# Femon Reverse Engineering Tool

## Descripción General

Herramienta de consola .NET 10.0 que replica el comportamiento de la aplicación Android **Femon Play** para extraer y descifrar la configuración remota, los canales de TV en vivo y firmar URLs de CDN Flow.

El flujo completo es:

1. Obtener `Remote Config` desde Firebase (sin autenticación)
2. Extraer clave AES y URL del JSON de canales desde la config
3. Descargar y descifrar `piratachanel.json` (doble AES/ECB)
4. Identificar canales que usan CDN Flow y firmar sus URLs con bearer token
5. Exportar el JSON descifrado a disco

---

## Archivos del Proyecto

| Archivo | Propósito |
|---|---|
| `Program.cs` | Entry point, orquesta todo el flujo |
| `RemoteConfig.cs` | Fetch de Firebase Remote Config vía REST API |
| `ChannelDecryptor.cs` | Descifrado AES/ECB de canales + modelos de datos |
| `FlowSigner.cs` | Identificación y firma de URLs de CDN Flow |
| `NLog.config` | Configuración de logging (consola simple + archivo completo) |
| `FemonReverse.csproj` | Proyecto .NET 10.0 con dependencia NLog |

---

## Flujo Detallado

### 1. Firebase Remote Config (`RemoteConfig.cs`)

```
POST https://firebaseremoteconfig.googleapis.com/v1/projects/femon-play/namespaces/firebase:fetch?key={FirebaseApiKey}
```

- **Autenticación:** Solo API Key + App ID (sin autenticación de usuario)
- **Payload:** `appId`, `appInstanceId` (UUID), `appVersion`, `countryCode`, `languageCode`, `platformVersion`, `sdkVersion`, `packageName`
- **Headers:** `X-Goog-Api-Key`, `X-Android-Package`, `X-Firebase-GMPID`, `User-Agent` (Firebase/Android), `X-Firebase-AppCheck`
- **Respuesta:** JSON con `entries` → `Dictionary<string, string>`
- **Credenciales extraídas del APK:**
  - `FirebaseApiKey`: `AIzaSyADcEYKamrewxL8CDA8NmAuRZjp8eZ2XzY`
  - `FirebaseProjectId`: `femon-play`
  - `FirebaseAppId`: `1:539591373021:android:88e80ca11e7a6d934aeb34`

**Claves de Remote Config utilizadas:**

| Clave | Propósito |
|---|---|
| `claveapp` | Clave AES para descifrar canales |
| `json3_url` / `json_url` | URL del JSON de canales |
| `flow_bearer_json_url` | URL del JSON con bearer token |
| `flow_cdn_gen_base_url` | Base URL del generador CDN |
| `flow_referer_domain` | Dominio Referer para identificar canales Flow |
| `flow_referer_domain_personal` | Dominio Referer personal |
| `flow_site_origin` | Origin del sitio Flow |
| `flow_site_origin_personal` | Origin personal |
| `flow_cdn_gen_base_url_personal` | Base URL CDN personal |
| `flow_bearer_json_url_personal` | URL bearer token personal |

---

### 2. Descifrado de Canales (`ChannelDecryptor.cs`)

#### 2.1 Modelos de Datos

```csharp
CategoryItem
├── Name: string
├── Samples: List<ChannelItem>?
└── HiddenSamples: List<ChannelItem>?

ChannelItem
├── Name: string
├── Url: string (encrypted → decrypted)
├── Type: string? (HLS, etc.)
├── DrmLicenseUri: string? (encrypted → decrypted)
├── Icono: string?
├── GlobalIndex: int
├── Headers: Dictionary<string, string>?
├── Headers2: Dictionary<string, string>?
├── HeadersM3u8: Dictionary<string, string>?
└── HeadersUrl: Dictionary<string, string>?
```

#### 2.2 Algoritmo de Descifrado

**Doble AES/ECB (método `DecryptDouble`):**

```
resultado = AES_DECRYPT(AES_DECRYPT(valor_cifrado_b64, clave), clave)
```

- **Modo:** ECB
- **Padding:** PKCS7 (equivalente a PKCS5 en Android)
- **Input:** Base64
- **Output:** UTF-8

**Método `BuildKeyBytes` — soporta 3 formatos de clave:**

1. **Base64** — si al decodificar da exactamente 16 bytes
2. **Hex** — si son exactamente 32 caracteres hex
3. **UTF-8 raw** — padding/truncado a 16 bytes

**Método `SmartDecrypt` — descifrado inteligente (hasta 3 capas):**

Para cada capa:
1. Decodifica Base64 → texto plano
2. Si ya es una URL válida (`http://` o `https://`), retorna
3. Si el largo es múltiplo de 16, intenta AES/ECB
4. Si falla, continúa con el texto decodificado

---

### 3. Flow Signer (`FlowSigner.cs`)

#### 3.1 Identificación de Canales Flow

Un canal se clasifica como **Flow** si su header `Referer` contiene el dominio configurado en Remote Config (`flow_referer_domain` o `flow_referer_domain_personal`).

Solo aplica a URLs que terminan en `.m3u8` o contienen `.mpd`.

#### 3.2 Obtención del Bearer Token

```
GET {flow_bearer_json_url}
Headers: User-Agent (Chrome 125)
```

- Busca en el JSON respuesta los campos: `bearerToken`, `token`, `bearer`, `access_token`
- Fallback: primer string de más de 10 caracteres
- Si no es JSON, asume que el body mismo es el token

#### 3.3 Firma de URL (método `SignChannel`)

**Paso 1 — Construir URL del CDN:**
- Extraer `PathAndQuery` de la URL original del canal
- URL-encodear el path
- Reemplazar `$encodedPath` (o `{encoded_url}`, `{url}`) en la plantilla `cdnGenBase`
- Si no hay placeholder, concatenar con `?url=`

**Paso 2 — Obtener token firmado del CDN:**
```
GET {cdnUrl}
Headers:
  Authorization: Bearer {bearerToken}
  Origin: {siteOrigin}
  Referer: {siteOrigin}/
  User-Agent: Chrome 125
```
- Busca en la respuesta JSON: `url`, `token`, `signedUrl`
- Fallback: el body completo

**Paso 3 — Resolver URL final:**
- Si el token firmado es una URL completa, úsalo directamente
- Si no, concatenar a la URL original como `?token={signedToken}`
- Seguir redirects HTTP para obtener la URL de reproducción real
- Limpiar query params de la URL final

---

### 4. Logging con NLog

#### Configuración (`NLog.config`)

| Target | Tipo | Layout | Nivel mínimo |
|---|---|---|---|
| `console` | `ColoredConsole` | Solo mensaje (simple) | Info |
| `logfile` | `File` (rotativo diario) | `{longdate} | {level} | {logger} | {message} | {exception}` | Debug |

- Archivos de log en `logs/reverse-{fecha}.log`
- Rotación diaria, máximo 14 archivos
- `autoReload="true"` — cambios en caliente

#### Niveles usados

| Nivel | Uso |
|---|---|
| `Info` | Progreso del flujo, datos descifrados |
| `Warn` | Fallos recuperables (token no encontrado, AES falló) |
| `Error` | Excepciones no recuperables |
| `Debug` | Detalles finos de descifrado (catch silenciosos) |

---

### 5. Dependencias

| Paquete | Versión |
|---|---|
| `NLog` | 5.4.0 |

Sin otras dependencias externas. Usa `System.Text.Json`, `System.Security.Cryptography` y `System.Net.Http` del runtime.

---

### 6. Ejecución

```bash
cd reverse
dotnet run
```

**Salida esperada:**
- Consola: mensajes simples del progreso (mismos textos que la app original)
- `canales_descifrados.json` en el directorio de salida
- `logs/reverse-{fecha}.log` con detalle completo

**Errores comunes:**
- Remote Config falla si la API key fue revocada o el proyecto cambió
- El JSON de canales puede estar ofuscado con capas adicionales de cifrado
- Los tokens bearer expiran; puede requerir refresco
