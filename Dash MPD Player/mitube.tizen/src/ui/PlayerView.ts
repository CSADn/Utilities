import { Channel } from "../models/Channel";

export class PlayerView {
  private container: HTMLElement;
  private video: HTMLVideoElement;
  private overlay: HTMLElement;
  private channelNameEl: HTMLElement;
  private errorEl: HTMLElement;
  private timeEl: HTMLElement;
  private playBtn: HTMLElement;
  private backBtn: HTMLElement;
  private player: any = null;
  private overlayTimer: ReturnType<typeof setTimeout> | null = null;
  private onBack: (() => void) | null = null;

  constructor(containerId: string) {
    this.container = document.getElementById(containerId)!;
    this.video = document.getElementById("video-element") as HTMLVideoElement;
    this.overlay = document.getElementById("player-overlay")!;
    this.channelNameEl = document.getElementById("player-channel-name")!;
    this.errorEl = document.getElementById("player-error")!;
    this.timeEl = document.getElementById("player-time")!;
    this.playBtn = document.getElementById("btn-play")!;
    this.backBtn = document.getElementById("btn-back")!;
  }

  setOnBack(cb: () => void): void {
    this.onBack = cb;
  }

  async show(channel: Channel): Promise<void> {
    this.container.classList.add("active");
    this.errorEl.style.display = "none";
    this.channelNameEl.textContent = channel.name;
    this.showOverlay();
    this.startOverlayTimer();

    // PlayerSetup loaded via static <script> tag in index.html
    const PlayerSetupCtor = (window as any).MiTubePlayer;
    if (!PlayerSetupCtor) {
      this.showError("Error: MiTubePlayer no disponible");
      return;
    }

    try {
      // Lazy init: create PlayerSetup once, reuse across plays.
      // This avoids destroy() race conditions and keeps Shaka stable.
      if (!this.player) {
        this.player = new PlayerSetupCtor(this.video);
        this.player.setOnError((msg: string) => this.showError(msg));
      } else {
        await this.player.unload();
      }
      await this.player.loadChannel(channel);
    } catch (e: any) {
      this.showError("Error al cargar el reproductor: " + (e.message || e));
    }

    this.bindEvents();
  }

  hide(): void {
    if (this.keyHandler) {
      document.removeEventListener("keydown", this.keyHandler, true);
      this.keyHandler = null;
    }
    this.video.pause();
    this.player?.unload().catch(() => {});
    this.container.classList.remove("active");
  }

  private keyHandler: ((e: KeyboardEvent) => void) | null = null;

  private bindEvents(): void {
    const toggleOverlay = () => {
      if (this.overlay.style.opacity === "0" || this.overlay.style.opacity === "") {
        this.showOverlay();
        this.startOverlayTimer();
      } else {
        this.hideOverlay();
      }
    };

    if (this.keyHandler) document.removeEventListener("keydown", this.keyHandler, true);
    this.keyHandler = (e: KeyboardEvent) => {
      if (!this.container.classList.contains("active")) return;
      // Samsung remote Back button → keyCode 10009
      if (e.keyCode === 10009) {
        e.preventDefault();
        e.stopPropagation();
        this.onBack?.();
        return;
      }
      switch (e.key) {
        case "Enter":
          e.preventDefault();
          toggleOverlay();
          break;
        case "Backspace":
        case "Escape":
          e.preventDefault();
          this.onBack?.();
          break;
      }
    };
    document.addEventListener("keydown", this.keyHandler, true);

    this.playBtn.addEventListener("click", () => {
      if (!this.player) return;
      if (this.player.isPaused) { this.player.play(); this.playBtn.textContent = "⏸"; }
      else { this.player.pause(); this.playBtn.textContent = "▶"; }
    });

    this.backBtn.addEventListener("click", () => this.onBack?.());
    this.video.addEventListener("timeupdate", () => this.updateTime());
    this.video.addEventListener("click", () => toggleOverlay());
  }

  private showOverlay(): void {
    this.overlay.style.opacity = "1";
    this.overlay.style.pointerEvents = "auto";
  }

  private hideOverlay(): void {
    this.overlay.style.opacity = "0";
    this.overlay.style.pointerEvents = "none";
  }

  private startOverlayTimer(): void {
    if (this.overlayTimer) clearTimeout(this.overlayTimer);
    this.overlayTimer = setTimeout(() => this.hideOverlay(), 4000);
  }

  private updateTime(): void {
    const fmt = (s: number) => {
      const m = Math.floor(s / 60);
      const sec = Math.floor(s % 60);
      return `${String(m).padStart(2, "0")}:${String(sec).padStart(2, "0")}`;
    };
    this.timeEl.textContent = `${fmt(this.video.currentTime)} / ${fmt(this.video.duration || 0)}`;
  }

  private showError(msg: string): void {
    this.errorEl.textContent = msg;
    this.errorEl.style.display = "block";
  }
}
