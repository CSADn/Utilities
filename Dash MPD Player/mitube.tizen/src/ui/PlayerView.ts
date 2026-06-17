import { Channel } from "../models/Channel";

interface Option {
  label: string;
  value: any;
}

interface CategoryState {
  options: Option[];
  currentIndex: number;
}

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

  // Settings
  private settingsEl: HTMLElement;
  private qualityValEl: HTMLElement;
  private audioValEl: HTMLElement;
  private subtitleValEl: HTMLElement;
  private settingsRows: HTMLElement[] = [];
  private settingsVisible: boolean = false;
  private settingsFocusRow: number = 0; // 0=quality, 1=audio, 2=subtitle

  // Track options state
  private qualityOptions: Option[] = [{ label: 'Auto', value: 'auto' }];
  private audioOptions: Option[] = [];
  private subtitleOptions: Option[] = [{ label: 'Off', value: null }];
  private qualityIndex: number = 0; // Auto
  private audioIndex: number = 0;
  private subtitleIndex: number = 0; // Off

  private unsubTracksChanged: (() => void) | null = null;

  // Toolbar D-pad navigation
  private toolbarFocusIndex: number = -1; // -1 = no button focused
  private toolbarButtons: HTMLElement[] = [];

  constructor(containerId: string) {
    this.container = document.getElementById(containerId)!;
    this.video = document.getElementById("video-element") as HTMLVideoElement;
    this.overlay = document.getElementById("player-overlay")!;
    this.channelNameEl = document.getElementById("player-channel-name")!;
    this.errorEl = document.getElementById("player-error")!;
    this.timeEl = document.getElementById("player-time")!;
    this.playBtn = document.getElementById("btn-play")!;
    this.backBtn = document.getElementById("btn-back")!;

    // Toolbar buttons in D-pad order (left to right)
    const settingsBtn = document.getElementById("btn-settings");
    this.toolbarButtons = [
      this.playBtn,
      ...(settingsBtn ? [settingsBtn] : []),
      this.backBtn,
    ];

    // Settings elements
    this.settingsEl = document.getElementById("player-settings")!;
    this.qualityValEl = document.getElementById("ps-quality-val")!;
    this.audioValEl = document.getElementById("ps-audio-val")!;
    this.subtitleValEl = document.getElementById("ps-subtitle-val")!;
    this.settingsRows = Array.from(document.querySelectorAll(".ps-row")) as HTMLElement[];
  }

  setOnBack(cb: () => void): void {
    this.onBack = cb;
  }

  async show(channel: Channel): Promise<void> {
    this.container.classList.add("active");
    this.errorEl.style.display = "none";
    this.channelNameEl.textContent = channel.name;
    this.hideSettings();
    this.showOverlay();
    this.startOverlayTimer();

    // PlayerSetup loaded via static <script> tag in index.html
    const PlayerSetupCtor = (window as any).MiTubePlayer;
    if (!PlayerSetupCtor) {
      this.showError("Error: MiTubePlayer no disponible");
      return;
    }

    // Reset indices for new channel
    this.qualityIndex = 0;   // Auto
    this.audioIndex = 0;
    this.subtitleIndex = 0;  // Off

    try {
      // Lazy init: create PlayerSetup once, reuse across plays.
      if (!this.player) {
        this.player = new PlayerSetupCtor(this.video);
        this.player.setOnError((msg: string) => this.showError(msg));
      } else {
        await this.player.unload();
      }
      await this.player.loadChannel(channel);

      // Listen for track changes (live DASH manifest reloads)
      if (this.unsubTracksChanged) this.unsubTracksChanged();
      this.unsubTracksChanged = this.player.onTracksChanged(() => {
        this.refreshTrackOptions();
      });

      // Initial track enumeration
      this.refreshTrackOptions();

      // Sync UI indices with Shaka's active tracks
      try {
        const activeAudio = (this.player.getAudioTracks() || []).findIndex((t: any) => t.active);
        if (activeAudio >= 0) this.audioIndex = activeAudio;
      } catch (_) {}
      try {
        const activeText = (this.player.getTextTracks() || []).findIndex((t: any) => t.active);
        this.subtitleIndex = activeText >= 0 ? Math.min(activeText + 1, this.subtitleOptions.length - 1) : 0;
      } catch (_) {}
      this.updateSettingsDisplay();
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
    this.hideSettings();
    if (this.unsubTracksChanged) {
      this.unsubTracksChanged();
      this.unsubTracksChanged = null;
    }
    this.video.pause();
    this.player?.unload().catch(() => {});
    this.container.classList.remove("active");
  }

  // ───────── Settings ─────────

  private refreshTrackOptions(): void {
    if (!this.player) return;

    // Quality options from variant tracks
    try {
      const variants: any[] = this.player.getVariantTracks() || [];
      const heights: number[] = [...new Set(variants.map((t: any) => t.height || 0))];
      const sorted = heights.filter(h => h > 0).sort((a, b) => b - a);
      this.qualityOptions = [
        { label: 'Auto', value: 'auto' },
        ...sorted.map((h: number) => ({ label: h + 'p', value: h }))
      ];
    } catch (e: any) {
      console.warn("[PlayerView] refresh quality:", e);
    }

    // Audio track options
    try {
      const audioTracks = this.player.getAudioTracks() || [];
      this.audioOptions = audioTracks.map((t: any) => ({
        label: t.label || t.language || 'Track ' + t.id,
        value: t
      }));
    } catch (e: any) {
      console.warn("[PlayerView] refresh audio:", e);
    }

    // Subtitle options
    try {
      const textTracks = this.player.getTextTracks() || [];
      this.subtitleOptions = [
        { label: 'Off', value: null },
        ...textTracks.map((t: any) => ({
          label: t.label || t.language || 'Track ' + t.id,
          value: t
        }))
      ];
    } catch (e: any) {
      console.warn("[PlayerView] refresh subtitle:", e);
    }

    // Clamp indices if options changed
    this.qualityIndex = Math.min(this.qualityIndex, this.qualityOptions.length - 1);
    this.audioIndex = Math.min(this.audioIndex, this.audioOptions.length - 1);
    this.subtitleIndex = Math.min(this.subtitleIndex, this.subtitleOptions.length - 1);

    this.updateSettingsDisplay();
  }

  private updateSettingsDisplay(): void {
    this.qualityValEl.textContent = this.qualityOptions[this.qualityIndex]?.label || 'Auto';
    this.audioValEl.textContent = this.audioOptions[this.audioIndex]?.label || '-';
    this.subtitleValEl.textContent = this.subtitleOptions[this.subtitleIndex]?.label || 'Off';
  }

  private toggleSettings(): void {
    if (this.settingsVisible) {
      this.hideSettings();
    } else {
      this.showSettings();
    }
  }

  private showSettings(): void {
    this.settingsVisible = true;
    this.settingsEl.classList.add("visible");
    this.settingsFocusRow = 0;
    this.focusSettingsRow(0);
    // Keep overlay visible while settings open, cancel auto-hide
    this.showOverlay();
    if (this.overlayTimer) clearTimeout(this.overlayTimer);
  }

  private hideSettings(): void {
    this.settingsVisible = false;
    this.settingsEl.classList.remove("visible");
    this.settingsRows.forEach(r => r.classList.remove("focused"));
  }

  private focusSettingsRow(index: number): void {
    this.settingsRows.forEach(r => r.classList.remove("focused"));
    this.settingsFocusRow = Math.max(0, Math.min(index, this.settingsRows.length - 1));
    this.settingsRows[this.settingsFocusRow]?.classList.add("focused");
  }

  private applyCurrentSelection(): void {
    const cat = this.settingsRows[this.settingsFocusRow]?.dataset?.cat;
    if (!cat || !this.player) return;

    switch (cat) {
      case 'quality': {
        const opt = this.qualityOptions[this.qualityIndex];
        if (opt.value === 'auto') {
          this.player.configureAbr(true);
        } else {
          this.player.configureAbr(false);
          const track = this.player.getVariantTracks().find((t: any) => t.height === opt.value);
          if (track) this.player.selectVariantTrack(track, true);
        }
        break;
      }
      case 'audio': {
        const opt = this.audioOptions[this.audioIndex];
        if (opt.value) this.player.selectAudioTrack(opt.value, true);
        break;
      }
      case 'subtitle': {
        const opt = this.subtitleOptions[this.subtitleIndex];
        if (opt.value) {
          this.player.selectTextTrack(opt.value);
          this.player.setTextTrackVisibility(true);
        } else {
          this.player.setTextTrackVisibility(false);
        }
        break;
      }
    }
    this.updateSettingsDisplay();
  }

  private cycleOption(direction: number): void {
    const cat = this.settingsRows[this.settingsFocusRow]?.dataset?.cat;
    if (!cat) return;

    switch (cat) {
      case 'quality':
        this.qualityIndex = (this.qualityIndex + direction + this.qualityOptions.length) % this.qualityOptions.length;
        break;
      case 'audio':
        if (this.audioOptions.length > 0)
          this.audioIndex = (this.audioIndex + direction + this.audioOptions.length) % this.audioOptions.length;
        break;
      case 'subtitle':
        this.subtitleIndex = (this.subtitleIndex + direction + this.subtitleOptions.length) % this.subtitleOptions.length;
        break;
    }

    this.applyCurrentSelection();
  }

  // ───────── Toolbar D-pad navigation ─────────

  private focusToolbarButton(index: number): void {
    this.toolbarButtons.forEach(b => b.classList.remove("focused"));
    this.toolbarFocusIndex = Math.max(-1, Math.min(index, this.toolbarButtons.length - 1));
    if (this.toolbarFocusIndex >= 0) {
      this.toolbarButtons[this.toolbarFocusIndex].classList.add("focused");
    }
  }

  // ───────── Events ─────────

  private keyHandler: ((e: KeyboardEvent) => void) | null = null;

  private bindEvents(): void {
    const toggleOverlay = () => {
      if (this.settingsVisible) {
        this.hideSettings();
        this.startOverlayTimer();
        return;
      }
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
        if (this.settingsVisible) {
          this.hideSettings();
          this.startOverlayTimer();
          return;
        }
        this.onBack?.();
        return;
      }

      // ── Settings navigation ──
      if (this.settingsVisible) {
        switch (e.key) {
          case "ArrowUp":
            e.preventDefault();
            this.focusSettingsRow(this.settingsFocusRow - 1);
            break;
          case "ArrowDown":
            e.preventDefault();
            this.focusSettingsRow(this.settingsFocusRow + 1);
            break;
          case "ArrowLeft":
            e.preventDefault();
            this.cycleOption(-1);
            break;
          case "ArrowRight":
            e.preventDefault();
            this.cycleOption(1);
            break;
          case "Enter":
            e.preventDefault();
            // Enter on a row: same as cycling right (quick-select)
            this.cycleOption(1);
            break;
          case "Backspace":
          case "Escape":
            e.preventDefault();
            this.hideSettings();
            this.startOverlayTimer();
            break;
        }
        return; // don't process other key events while in settings
      }

      // ── Normal player navigation ──
      switch (e.key) {
        case "Enter":
          e.preventDefault();
          if (this.toolbarFocusIndex >= 0) {
            // Click the focused toolbar button
            this.toolbarButtons[this.toolbarFocusIndex]?.click();
          } else {
            toggleOverlay();
          }
          break;
        case "ArrowLeft":
          e.preventDefault();
          if (this.toolbarFocusIndex >= 0) {
            const prev = this.toolbarFocusIndex - 1;
            this.focusToolbarButton(prev < 0 ? this.toolbarButtons.length - 1 : prev);
          } else {
            this.focusToolbarButton(this.toolbarButtons.length - 1);
          }
          break;
        case "ArrowRight":
          e.preventDefault();
          if (this.toolbarFocusIndex >= 0) {
            const next = this.toolbarFocusIndex + 1;
            this.focusToolbarButton(next >= this.toolbarButtons.length ? 0 : next);
          } else {
            this.focusToolbarButton(0);
          }
          break;
        case "ArrowUp":
        case "ArrowDown":
          e.preventDefault();
          if (this.overlay.style.opacity === "0" || this.overlay.style.opacity === "") {
            this.showOverlay();
            this.focusToolbarButton(1); // settings button (play=0, settings=1, back=2)
            this.startOverlayTimer();
          }
          break;
        case "Backspace":
        case "Escape":
          e.preventDefault();
          this.focusToolbarButton(-1);
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

    // Settings button
    const settingsBtn = document.getElementById("btn-settings");
    if (settingsBtn) {
      settingsBtn.addEventListener("click", () => this.toggleSettings());
    }
    this.video.addEventListener("timeupdate", () => this.updateTime());

    // Video click: toggle overlay or close settings when open
    this.video.addEventListener("click", () => {
      if (this.settingsVisible) {
        this.hideSettings();
        this.startOverlayTimer();
      } else {
        toggleOverlay();
      }
    });

    // Settings rows: click to cycle
    this.settingsRows.forEach((row) => {
      row.addEventListener("click", () => {
        if (!this.settingsVisible) return;
        const idx = this.settingsRows.indexOf(row);
        if (idx >= 0) {
          this.focusSettingsRow(idx);
          this.cycleOption(1);
        }
      });
    });
  }

  private showOverlay(): void {
    this.overlay.style.opacity = "1";
    this.overlay.style.pointerEvents = "auto";
    this.focusToolbarButton(-1);
  }

  private hideOverlay(): void {
    if (this.settingsVisible) return; // don't hide overlay while settings open
    this.overlay.style.opacity = "0";
    this.overlay.style.pointerEvents = "none";
    this.focusToolbarButton(-1);
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
