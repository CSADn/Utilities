# FemonReverse - Detailed Logic

## Execution Flow (`Program.cs`)
The tool follows a linear sequence of operations:
1. **Fetch Remote Config**: Calls `RemoteConfig.Fetch()` to get a dictionary of settings.
2. **Parameter Extraction**: Extracts `claveapp` (AES key) and the JSON URL (priority: `json3_url` > `json_url` > default).
3. **Data Loading**: 
   - Checks for `canales_descifrados.json` on disk.
   - If missing, calls `ChannelDecryptor.DownloadAndDecrypt`.
4. **Flow Channel Processing**:
   - Identifies "Flow" channels by checking if the `Referer` header contains specific domains from Remote Config.
   - Distinguishes between **Standard** and **Personal** channels based on the referer domain used.
5. **Signing**: For the first detected Flow channel, it fetches the bearer token and resolves the final signed URL.
6. **Final Export**: Serializes the entire category list to JSON with `UnsafeRelaxedJsonEscaping`.

## Channel Classification Logic
Channels are classified as `FlowChannel` if:
- The URL ends with `.m3u8` or contains `.mpd`.
- The `Headers` dictionary contains a `referer` key.
- The referer value matches either `flow_referer_domain` (Standard) or `flow_referer_domain_personal` (Personal).

## URL Resolution Process
The final URL is not just signed but resolved:
1. The signed token is appended to the original URL.
2. An HTTP GET request is made to this signed URL.
3. The tool follows all redirects (`AllowAutoRedirect = true`).
4. The final `RequestUri` is captured and query parameters are stripped to get the clean playback source.
