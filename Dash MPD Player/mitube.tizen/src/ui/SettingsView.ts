import { CONFIG } from "../config";
import { SessionStore } from "../services/SessionStore";
import { setToken } from "../api/client";

export class SettingsView {
  private container: HTMLElement;
  private serverEl: HTMLElement;
  private userEl: HTMLElement;
  private logoutBtn: HTMLElement;
  private closeBtn: HTMLElement;
  private onLogout: (() => void) | null = null;
  private onClose: (() => void) | null = null;

  constructor(containerId: string) {
    this.container = document.getElementById(containerId)!;
    this.serverEl = document.getElementById("settings-server")!;
    this.userEl = document.getElementById("settings-user")!;
    this.logoutBtn = document.getElementById("btn-logout")!;
    this.closeBtn = document.getElementById("btn-settings-close")!;
  }

  setOnLogout(cb: () => void): void {
    this.onLogout = cb;
  }

  setOnClose(cb: () => void): void {
    this.onClose = cb;
  }

  show(): void {
    this.container.classList.add("active");
    this.serverEl.textContent = CONFIG.serverUrl;
    this.userEl.textContent = SessionStore.displayName || SessionStore.username || "";
    this.logoutBtn.focus();
    this.bindKeyboard();
    this.logoutBtn.addEventListener("click", () => this.doLogout());
    this.closeBtn.addEventListener("click", () => this.onClose?.());
  }

  hide(): void {
    this.container.classList.remove("active");
  }

  private doLogout(): void {
    SessionStore.clear();
    setToken(null);
    this.onLogout?.();
  }

  private bindKeyboard(): void {
    const handler = (e: KeyboardEvent) => {
      if (!this.container.classList.contains("active")) return;
      if (e.key === "Backspace" || e.key === "Escape") {
        e.preventDefault();
        this.onClose?.();
      }
    };
    document.addEventListener("keydown", handler, { once: true });
    this.closeBtn.addEventListener("keydown", (e) => {
      if (e.key === "Enter") this.onClose?.();
    });
  }
}
