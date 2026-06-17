import shaka from "shaka-player";
import Hls from "hls.js";
import { Channel, parseClearkeyLicense } from "../models/Channel";
import { TokenService } from "../services/TokenService";
import { CONFIG } from "../config";

export class PlayerSetup {
  private video: HTMLVideoElement;
  private shakaPlayer: shaka.Player | null = null;
  private hlsPlayer: Hls | null = null;
  private currentChannel: Channel | null = null;
  private onError: ((msg: string) => void) | null = null;
  private isFlow: boolean = false;
  /** True if the URL has a pre-signed tok_ JWT in the path (e.g. edge-live21-sl.../tok_<JWT>/...).
   *  These don't need CDN token generation; the token is baked into the URL. */
  private hasEmbeddedToken: boolean = false;
  private channelHeaders: Record<string, string> | null = null;
  private cdnToken: string | null = null;
  private cdnTokenExpiresAt: number = 0;
  private _tokenRefreshTimer: ReturnType<typeof setInterval> | null = null;

  constructor(videoElement: HTMLVideoElement) {
    this.video = videoElement;

    // Tizen Chromium 77: window.fetch is broken (returns fake 200 with empty body).
    // Delete it so Shaka falls back to XMLHttpRequest.
    if (typeof window !== "undefined" && (window as any).fetch) {
      try { (window as any).fetch = undefined; } catch (_) {}
    }

    shaka.polyfill.installAll();
    if (shaka.Player.isBrowserSupported()) {
      this.shakaPlayer = new shaka.Player(this.video);
      this.registerShakaFilters();
      this.shakaPlayer.addEventListener('error', (event: any) => {
        const error = event.detail;
        if (error?.code === 1002 && error?.data?.[1] === 403) {
          this.handleTokenExpired();
        }
      });
    }
  }

  setOnError(cb: (msg: string) => void): void {
    this.onError = cb;
  }

  async loadChannel(channel: Channel): Promise<void> {
    this.currentChannel = channel;
    this.channelHeaders = channel.headers || null;
    this.isFlow = TokenService.isFlowChannel(channel.headers);
    this.hasEmbeddedToken = channel.url.includes("/tok_");
    this.cdnToken = null;
    this.cdnTokenExpiresAt = 0;
    this.stopTokenRefreshTimer();
    this.destroyCurrent();

    const url = channel.url;
    const isHls = url.includes(".m3u8") || channel.type === "HLS";

    if (isHls) {
      await this.loadHls(url, channel);
    } else {
      await this.loadDash(url, channel);
    }
  }

  private configureDrm(channel: Channel): void {
    if (!this.shakaPlayer) return;
    const type = channel.type?.toUpperCase();
    const uri = channel.drm_license_uri;

    if (type === "CLEARKEY" && uri) {
      const parsed = parseClearkeyLicense(uri);
      if (parsed) {
        const clearKeys: Record<string, string> = {};
        clearKeys[parsed.keyId] = parsed.key;
        this.shakaPlayer.configure({ drm: { clearKeys } });
        console.log("[Player] DRM: Clearkey configured");
      }
    } else if (type === "WIDEVINE" && uri) {
      this.shakaPlayer.configure({
        drm: { servers: { "com.widevine.alpha": uri } },
      });
      console.log("[Player] DRM: Widevine configured");
    }
  }

  private async loadDash(url: string, channel: Channel): Promise<void> {
    if (!this.shakaPlayer) {
      this.emitError("Shaka Player no disponible");
      return;
    }
    try {
      this.configureDrm(channel);

      let manifestUrl = url;
      if (this.isFlow && this.channelHeaders) {
        if (this.hasEmbeddedToken) {
          // New URL format: token is pre-embedded in the path (/tok_<JWT>/live/...).
          // Load the manifest directly. No CDN token generation needed.
          // Segments resolve relative to this URL (including the tok_ prefix).
          console.log("[Player] URL has embedded token, loading directly");
        } else {
          // Old URL format (chromecast.cvattv.com.ar): sign URL with CDN token,
          // then let requestFilter route all Shaka requests through the backend proxy.
          const jwt = this.getJwtFromStorage();
          if (!jwt) throw new Error("No JWT token available (not logged in?)");
          const cdnToken = await TokenService.requestCdnTokenViaBackend(url, CONFIG.serverUrl, jwt, this.channelHeaders ?? undefined);
          this.cdnToken = cdnToken;
          this.cdnTokenExpiresAt = Date.now() + 55000;
          this.startTokenRefreshTimer();
          const sep = url.includes("?") ? "&" : "?";
          manifestUrl = `${url}${sep}cdntoken=${cdnToken}`;
        }
      }

      // Disable Shaka text visibility by default (we control it manually)
      this.shakaPlayer.configure({ textDisplay: { visibility: false } });

      await this.shakaPlayer.load(manifestUrl);
      console.log("[Player] DASH playing:", channel.name);

      // Defaults: prefer Spanish audio, subtitles off
      this.applyDefaults();
    } catch (e: any) {
      this.emitError(`Error DASH: ${e.message || e}`);
    }
  }

  /** Apply default track preferences after stream loads. */
  private applyDefaults(): void {
    if (!this.shakaPlayer) return;

    // Set preferred audio language (Shaka will auto-select on load)
    // For MPDs where both audio tracks have lang="es", try to find
    // the Spanish track by label / representation ID
    try {
      const audioTracks = this.shakaPlayer.getAudioTracks();
      if (audioTracks.length > 1) {
        const spanish = (audioTracks as any[]).find((t: any) => {
          const tid = String(t.id).toLowerCase();
          const label = (t.label || '').toLowerCase();
          return tid.includes('spa') || label.includes('spa') ||
                 tid.includes('español') || label.includes('español') ||
                 tid.includes('spanish') || label.includes('spanish');
        });
        if (spanish) {
          this.shakaPlayer.selectAudioTrack(spanish);
          console.log("[Player] Audio: selected Spanish track (id=" + spanish.id + ")");
        }
      }
    } catch (e: any) {
      console.warn("[Player] applyDefaults audio:", e);
    }

    // Ensure subtitles are hidden by default
    try {
      this.shakaPlayer.setTextTrackVisibility(false);
    } catch (_) {}
  }

  private async loadHls(url: string, channel: Channel): Promise<void> {
    if (!Hls.isSupported()) {
      this.emitError("hls.js no soportado");
      return;
    }
    const hls = new Hls();
    this.hlsPlayer = hls;
    hls.attachMedia(this.video);
    hls.on(Hls.Events.MANIFEST_PARSED, () => {
      this.video.play().catch(() => {});
    });
    hls.on(Hls.Events.ERROR, (_e, data) => {
      if (data.fatal) {
        this.emitError(`Error HLS: ${data.type}`);
      }
    });
    hls.loadSource(url);
    console.log("[Player] HLS playing:", channel.name);
  }

  private registerShakaFilters(): void {
    if (!this.shakaPlayer) return;
    const engine = this.shakaPlayer.getNetworkingEngine();
    if (!engine) return;

    engine.registerRequestFilter((_type: shaka.net.NetworkingEngine.RequestType, request: shaka.extern.Request) => {
      const channel = this.currentChannel;
      if (!channel?.headers) return;

      if (this.isFlow) {
        // Flow channels: XHR can't set Origin/Referer/User-Agent → 403 from CDN.
        // Rewrite ALL Shaka requests to go through the backend proxy.
        // The backend uses native HttpClient (no header restrictions).

        // Skip non-HTTP URIs (data:/blob:) that Shaka generates internally
        let originalUrl = request.uris[0];
        if (originalUrl.startsWith("data:") || originalUrl.startsWith("blob:")) return;

        if (!this.hasEmbeddedToken) {
          // Old URL format: append or replace CDN token on ALL Flow requests.
          // The manifest URL already has it from loadDash, but segments resolved
          // from the MPD don't. Token must be refreshed before expiry (60s), so
          // always use the current this.cdnToken.
          if (this.cdnToken) {
            if (originalUrl.includes("cdntoken=")) {
              originalUrl = originalUrl.replace(/cdntoken=[^&]+/, `cdntoken=${this.cdnToken}`);
            } else {
              const sep = originalUrl.includes("?") ? "&" : "?";
              originalUrl += `${sep}cdntoken=${this.cdnToken}`;
            }
          }
        } else {
          // New URL format: token is already in the path (/tok_<JWT>/...).
          // No need to append cdntoken query param. Segments resolved from the
          // MPD inherit the token via the base URL path.
        }

        const proxyBase = CONFIG.serverUrl.replace(/\/+$/, "") + "/api/proxy/fetch";

        // Pass real headers as custom X-Proxy-* headers (XHR accepts these)
        const h = channel.headers;
        if (h["Origin"]) request.headers["X-Proxy-Origin"] = h["Origin"];
        if (h["Referer"]) request.headers["X-Proxy-Referer"] = h["Referer"];
        if (h["User-Agent"]) request.headers["X-Proxy-User-Agent"] = h["User-Agent"];

        // Rewrite URI to backend proxy
        request.uris[0] = proxyBase + "?url=" + encodeURIComponent(originalUrl);
      } else {
        // Non-Flow channels: set allowed headers on direct requests
        for (const [key, value] of Object.entries(channel.headers)) {
          const lk = key.toLowerCase();
          if (lk === "content-type" || lk === "origin" || lk === "referer" || lk === "user-agent") continue;
          request.headers[key] = value;
        }
      }
    });
  }

  private async handleTokenExpired(): Promise<void> {
    if (!this.currentChannel || !this.isFlow || this.hasEmbeddedToken) return;
    try {
      const jwt = this.getJwtFromStorage();
      if (!jwt) throw new Error("No JWT token available");
      const cdnToken = await TokenService.requestCdnTokenViaBackend(
        this.currentChannel.url, CONFIG.serverUrl, jwt, this.channelHeaders ?? undefined
      );
      this.cdnToken = cdnToken;
      this.cdnTokenExpiresAt = Date.now() + 55000;
      const url = this.currentChannel.url;
      const sep = url.includes("?") ? "&" : "?";
      if (this.shakaPlayer) {
        await this.shakaPlayer.load(`${url}${sep}cdntoken=${cdnToken}`);
      }
    } catch (e: any) {
      this.emitError(`Error renovando token CDN: ${e.message || e}`);
    }
  }

  private startTokenRefreshTimer(): void {
    this.stopTokenRefreshTimer();
    this._tokenRefreshTimer = setInterval(async () => {
      if (!this.isFlow || !this.currentChannel || this.hasEmbeddedToken) return;
      try {
        const jwt = this.getJwtFromStorage();
        if (!jwt) return;
        const cdnToken = await TokenService.requestCdnTokenViaBackend(
          this.currentChannel.url, CONFIG.serverUrl, jwt, this.channelHeaders ?? undefined
        );
        this.cdnToken = cdnToken;
        this.cdnTokenExpiresAt = Date.now() + 55000;
        console.log("[Player] CDN token refreshed (timer)");
      } catch (e: any) {
        console.warn("[Player] CDN token refresh failed:", e);
      }
    }, 50000);
  }

  private getJwtFromStorage(): string | null {
    try {
      return localStorage.getItem("mitube_token");
    } catch (_) {
      return null;
    }
  }

  private stopTokenRefreshTimer(): void {
    if (this._tokenRefreshTimer !== null) {
      clearInterval(this._tokenRefreshTimer);
      this._tokenRefreshTimer = null;
    }
  }

  private destroyCurrent(): void {
    if (this.hlsPlayer) {
      this.hlsPlayer.destroy();
      this.hlsPlayer = null;
    }
  }

  /** Unload current manifest/stream and prepare for next channel.
   *  Unlike destroy() this keeps the Player instance alive for reuse. */
  async unload(): Promise<void> {
    this.stopTokenRefreshTimer();
    this.destroyCurrent();
    this.cdnToken = null;
    this.cdnTokenExpiresAt = 0;
    this.currentChannel = null;
    this.isFlow = false;
    this.hasEmbeddedToken = false;
    this.channelHeaders = null;
    if (this.shakaPlayer) {
      await this.shakaPlayer.unload();
      // Reset DRM config so next channel's configureDrm() starts clean
      this.shakaPlayer.configure({ drm: { clearKeys: {}, servers: {} } });
    }
  }

  play(): void { this.video.play().catch(() => {}); }
  pause(): void { this.video.pause(); }
  get isPaused(): boolean { return this.video.paused; }
  get duration(): number { return this.video.duration || 0; }
  get currentTime(): number { return this.video.currentTime || 0; }
  seek(time: number): void { this.video.currentTime = time; }

  // ───────── Track selection API (proxy to shakaPlayer) ─────────

  getVariantTracks(): any[] {
    return this.shakaPlayer ? this.shakaPlayer.getVariantTracks() : [];
  }
  getAudioTracks(): any[] {
    return this.shakaPlayer ? this.shakaPlayer.getAudioTracks() : [];
  }
  getTextTracks(): any[] {
    return this.shakaPlayer ? this.shakaPlayer.getTextTracks() : [];
  }
  selectVariantTrack(track: any, clearBuffer?: boolean): void {
    this.shakaPlayer?.selectVariantTrack(track, clearBuffer);
  }
  selectAudioTrack(track: any, clearBuffer?: boolean): void {
    (this.shakaPlayer as any)?.selectAudioTrack(track, clearBuffer);
  }
  selectTextTrack(track: any): void {
    this.shakaPlayer?.selectTextTrack(track);
  }
  setTextTrackVisibility(visible: boolean): void {
    this.shakaPlayer?.setTextTrackVisibility(visible);
  }
  configureAbr(enabled: boolean): void {
    this.shakaPlayer?.configure({ abr: { enabled } });
  }

  /** Register a callback for Shaka 'trackschanged' event. Returns an unsubscribe function. */
  onTracksChanged(cb: () => void): () => void {
    if (!this.shakaPlayer) return () => {};
    const handler = () => cb();
    this.shakaPlayer.addEventListener('trackschanged', handler);
    return () => {
      this.shakaPlayer?.removeEventListener('trackschanged', handler);
    };
  }

  async destroy(): Promise<void> {
    this.destroyCurrent();
    if (this.shakaPlayer) {
      await this.shakaPlayer.destroy();
      this.shakaPlayer = null;
    }
  }

  private emitError(msg: string): void {
    console.error("[Player]", msg);
    this.onError?.(msg);
  }
}

// Expose on window for static script tag loading
(window as any).MiTubePlayer = PlayerSetup;
