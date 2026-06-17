const BEARER_URL = "https://app.femon.net/pirata/piratacodigo.json";
const CDN_TOKEN_BASE = "https://cdn-token.app.flow.com.ar/cdntoken/v2/generator";

let cachedBearer: { token: string; expiresAt: number } | null = null;

/** XHR-based GET that returns response text (proven working on Tizen) */
function xhrFetchText(url: string, headers?: Record<string, string>): Promise<string> {
  return new Promise((resolve, reject) => {
    const xhr = new XMLHttpRequest();
    xhr.open("GET", url, true);
    xhr.timeout = 30000;
    if (headers) {
      for (const [k, v] of Object.entries(headers)) {
        // XHR refuses to set "unsafe" headers: Origin, Referer, User-Agent.
        // They are set by the browser automatically; skip them to avoid console warnings.
        const lk = k.toLowerCase();
        if (lk === "origin" || lk === "referer" || lk === "user-agent") continue;
        xhr.setRequestHeader(k, v);
      }
    }
    xhr.onreadystatechange = () => {
      if (xhr.readyState !== 4) return;
      const text = xhr.responseText || "";
      if (xhr.status >= 200 && xhr.status < 300) {
        resolve(text);
      } else {
        reject(new Error(`HTTP ${xhr.status}: ${text.substring(0, 100)}`));
      }
    };
    xhr.onerror = () => reject(new Error("XHR connection failed"));
    xhr.ontimeout = () => reject(new Error("XHR timeout"));
    xhr.send();
  });
}

function extractJsonString(text: string, ...keys: string[]): string | null {
  try {
    const obj = JSON.parse(text);
    for (const key of keys) {
      const val = obj[key];
      if (typeof val === "string") return val;
    }
  } catch {
    // not JSON
  }
  return null;
}

async function getBearerToken(): Promise<string> {
  const now = Date.now();
  if (cachedBearer && cachedBearer.expiresAt > now) {
    return cachedBearer.token;
  }
  const text = await xhrFetchText(BEARER_URL);
  let token = extractJsonString(text, "bearerToken", "token", "access_token");
  if (!token) {
    // fallback: treat raw text as token
    token = text.trim();
  }
  // Strip "Bearer " prefix if present (C# port: AuthenticationHeaderValue adds it)
  if (token.startsWith("Bearer ")) {
    token = token.substring("Bearer ".length);
  }
  cachedBearer = { token, expiresAt: now + 60 * 60 * 1000 };
  console.log("[TokenService] Bearer token refreshed");
  return token;
}

function invalidateBearer(): void {
  cachedBearer = null;
}

async function requestCdnToken(
  mpdUrl: string,
  bearerToken: string,
  headers?: Record<string, string>
): Promise<string> {
  const encodedPath = encodeURIComponent(mpdUrl);
  const url = `${CDN_TOKEN_BASE}?path=${encodedPath}`;
  const reqHeaders: Record<string, string> = {
    Authorization: `Bearer ${bearerToken}`,
    ...(headers || {}),
  };
  const text = await xhrFetchText(url, reqHeaders);
  const cdnToken = extractJsonString(text, "token");
  if (!cdnToken) {
    throw new Error(`CDN token response missing 'token' field: ${text.substring(0, 100)}`);
  }
  console.log("[TokenService] CDN token obtained");
  return cdnToken;
}

/**
 * Request CDN token via the backend proxy.
 * The backend's HttpClient can set Origin/Referer/User-Agent headers
 * (unlike XHR in the browser), producing a valid CDN token.
 * Requires a valid JWT (from login) sent as Bearer auth.
 * Channel headers (Origin/Referer/User-Agent) are forwarded as
 * X-Proxy-* headers so the backend's CdnTokenService can use the
 * correct channel-specific values when requesting the CDN token.
 */
async function requestCdnTokenViaBackend(
  mpdUrl: string,
  backendUrl: string,
  jwtToken: string,
  channelHeaders?: Record<string, string>
): Promise<string> {
  const cleanUrl = backendUrl.replace(/\/+$/, "");
  const endpoint = `${cleanUrl}/api/proxy/cdn-token?url=${encodeURIComponent(mpdUrl)}`;
  const reqHeaders: Record<string, string> = {
    Authorization: `Bearer ${jwtToken}`,
  };
  // Forward channel headers as X-Proxy-* so the backend can inject them
  // into the CDN token generation request (XHR can't set Origin/Referer/UA directly).
  if (channelHeaders) {
    if (channelHeaders["Origin"]) reqHeaders["X-Proxy-Origin"] = channelHeaders["Origin"];
    if (channelHeaders["Referer"]) reqHeaders["X-Proxy-Referer"] = channelHeaders["Referer"];
    if (channelHeaders["User-Agent"]) reqHeaders["X-Proxy-User-Agent"] = channelHeaders["User-Agent"];
  }
  const text = await xhrFetchText(endpoint, reqHeaders);
  const parsed = JSON.parse(text);
  const cdnToken = parsed.cdnToken || parsed.token;
  if (!cdnToken) {
    throw new Error(`Backend CDN token response missing cdnToken: ${text.substring(0, 100)}`);
  }
  console.log("[TokenService] CDN token obtained via backend");
  return cdnToken;
}

function isFlowChannel(headers?: Record<string, string>): boolean {
  return headers !== undefined && headers !== null && Object.keys(headers).length > 0;
}

export const TokenService = { getBearerToken, invalidateBearer, requestCdnToken, requestCdnTokenViaBackend, isFlowChannel };
