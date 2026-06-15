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
  private channelHeaders: Record<string, string> | null = null;
  private cdnToken: string | null = null;

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
    }
  }

  setOnError(cb: (msg: string) => void): void {
    this.onError = cb;
  }

  async loadChannel(channel: Channel): Promise<void> {
    this.currentChannel = channel;
    this.channelHeaders = channel.headers || null;
    this.isFlow = TokenService.isFlowChannel(channel.headers);
    this.cdnToken = null;
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
        // Flow: sign URL with CDN token, then let requestFilter
        // route all Shaka requests through the backend proxy
        const bearer = await TokenService.getBearerToken();
        const cdnToken = await TokenService.requestCdnToken(url, bearer, this.channelHeaders);
        this.cdnToken = cdnToken;
        const sep = url.includes("?") ? "&" : "?";
        manifestUrl = `${url}${sep}cdntoken=${cdnToken}`;
      }

      await this.shakaPlayer.load(manifestUrl);
      console.log("[Player] DASH playing:", channel.name);
    } catch (e: any) {
      this.emitError(`Error DASH: ${e.message || e}`);
    }
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

        // Append CDN token to ALL Flow requests so the backend proxy
        // forwards it to the CDN upstream (otherwise CDN returns 403 "token required").
        // The manifest URL already has it from loadDash, but segments resolved
        // from the MPD don't.
        if (this.cdnToken && !originalUrl.includes("cdntoken=")) {
          const sep = originalUrl.includes("?") ? "&" : "?";
          originalUrl += `${sep}cdntoken=${this.cdnToken}`;
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

  private destroyCurrent(): void {
    if (this.hlsPlayer) {
      this.hlsPlayer.destroy();
      this.hlsPlayer = null;
    }
  }

  /** Unload current manifest/stream and prepare for next channel.
   *  Unlike destroy() this keeps the Player instance alive for reuse. */
  async unload(): Promise<void> {
    this.destroyCurrent();
    this.cdnToken = null;
    this.currentChannel = null;
    this.isFlow = false;
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
