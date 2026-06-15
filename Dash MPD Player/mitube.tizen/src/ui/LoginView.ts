import { login, ping, setToken, ApiError } from "../api/client";
import { SessionStore } from "../services/SessionStore";

export class LoginView {
  private container: HTMLElement;
  private form: HTMLFormElement;
  private usernameInput: HTMLInputElement;
  private passwordInput: HTMLInputElement;
  private btn: HTMLButtonElement;
  private toastEl: HTMLElement;
  private toastTimer: number | null = null;
  private onSuccess: (() => void) | null = null;

  private focusIndex: number = 0;
  private fields: (HTMLElement)[] = [];
  private isEditMode: boolean = false;
  private canSubmit: boolean = false;
  private keyHandler: ((e: KeyboardEvent) => void) | null = null;
  private keySequence: number[] = [];
  private readonly SECRET_SEQUENCE = [49, 50, 51, 52, 53]; // 1,2,3,4,5

  constructor(containerId: string) {
    this.container = document.getElementById(containerId)!;
    this.form = document.getElementById("login-form") as HTMLFormElement;
    this.usernameInput = document.getElementById("login-username") as HTMLInputElement;
    this.passwordInput = document.getElementById("login-password") as HTMLInputElement;
    this.btn = document.getElementById("login-btn") as HTMLButtonElement;
    this.toastEl = document.getElementById("login-toast") as HTMLElement;

    this.fields = [this.usernameInput, this.passwordInput, this.btn];

    this.form.addEventListener("submit", (e) => { e.preventDefault(); this.doLogin(); });
    this.btn.addEventListener("click", () => this.doLogin());
    this.usernameInput.addEventListener("input", () => this.updateBtn());
    this.passwordInput.addEventListener("input", () => this.updateBtn());
  }

  setOnSuccess(cb: () => void): void {
    this.onSuccess = cb;
  }

  show(): void {
    this.container.classList.add("active");
    this.toastEl.classList.remove("show");

    this.usernameInput.value = SessionStore.username || "";
    this.passwordInput.value = "";
    this.updateBtn();

    this.focusIndex = 0;
    this.isEditMode = false;
    this.updateVisualFocus();

    // Use document capture (confirmed working from debug overlay)
    if (this.keyHandler) document.removeEventListener("keydown", this.keyHandler, true);
    this.keyHandler = (e: KeyboardEvent) => this.onKey(e);
    document.addEventListener("keydown", this.keyHandler, true);

    // Diagnostic: test GET connectivity
    this.testPing();
  }

  /** Quick connectivity check using XHR (proven working) */
  private async testPing(): Promise<void> {
    try {
      const text = await ping();
      this.appendDebug(`[Ping] OK: "${text}"`);
    } catch (e: any) {
      this.appendDebug(`[Ping] FAIL: ${e.message}`);
    }
  }

  hide(): void {
    this.container.classList.remove("active");
    if (this.keyHandler) {
      document.removeEventListener("keydown", this.keyHandler, true);
      this.keyHandler = null;
    }
  }

  private onKey(e: KeyboardEvent): void {
    // Secret sequence 1-2-3-4-5 while button has focus → toggle debug overlay
    this.checkSecretSequence(e.keyCode);

    // IME done/cancel: exit edit mode
    if (e.keyCode === 65376 || e.keyCode === 65385) {
      if (this.isEditMode) this.exitEditMode();
      return;
    }

    // Back: exit edit mode if editing
    if (e.keyCode === 10009) {
      if (this.isEditMode) {
        this.exitEditMode();
        return;
      }
      return;
    }

    // If in edit mode, let the input handle arrow keys natively (cursor movement)
    if (this.isEditMode) {
      if (e.keyCode === 13) {
        // Enter in edit mode: submit if password field, else move to next
        if (this.focusIndex === 1) {
          this.exitEditMode();
          this.doLogin();
        } else {
          // Move to next field
          this.exitEditMode();
          this.focusNext();
          this.enterEditMode();
        }
      }
      // For arrow keys in edit mode, let them pass through
      return;
    }

    // Navigation mode: handle arrow keys
    switch (e.keyCode) {
      case 38: // Up
      case 37: // Left
        this.focusPrev();
        break;
      case 40: // Down
      case 39: // Right
        this.focusNext();
        break;
      case 13: // Enter
        if (this.focusIndex === 2) {
          this.doLogin();
        } else {
          this.enterEditMode();
        }
        break;
    }
  }

  private focusNext(): void {
    this.focusIndex = (this.focusIndex + 1) % this.fields.length;
    this.updateVisualFocus();
  }

  private focusPrev(): void {
    this.focusIndex = (this.focusIndex - 1 + this.fields.length) % this.fields.length;
    this.updateVisualFocus();
  }

  private updateVisualFocus(): void {
    this.fields.forEach((f) => f.classList.remove("focused"));
    this.fields[this.focusIndex].classList.add("focused");
  }

  private enterEditMode(): void {
    if (this.focusIndex === 2) return; // button has no edit mode
    const input = this.fields[this.focusIndex] as HTMLInputElement;
    this.isEditMode = true;
    input.focus();
  }

  private exitEditMode(): void {
    this.isEditMode = false;
    if (document.activeElement instanceof HTMLElement) {
      document.activeElement.blur();
    }
    this.updateVisualFocus();
  }

  private updateBtn(): void {
    const u = this.usernameInput.value.trim();
    const p = this.passwordInput.value.trim();
    this.canSubmit = !!u && !!p;
    this.btn.classList.toggle("btn-inactive", !this.canSubmit);
  }

  private async doLogin(): Promise<void> {
    const username = this.usernameInput.value.trim();
    const password = this.passwordInput.value.trim();
    if (!username || !password) return;
    this.btn.classList.add("btn-inactive");
    this.btn.textContent = "Conectando...";
    this.showToast("Conectando...", "info");

    try {
      const res = await login({ username, password });
      setToken(res.token);
      SessionStore.token = res.token;
      SessionStore.username = username;
      SessionStore.displayName = res.displayName;
      this.onSuccess?.();
    } catch (e: any) {
      this.btn.textContent = "Ingresar";
      this.updateBtn();

      this.appendDebug(`[Login] ${e.name}: ${e.message}`);

      if (e instanceof ApiError) {
        this.showToast("Usuario o contraseña incorrectos", "error");
      } else {
        this.showToast(`Error: ${e.name}: ${e.message}`, "error");
      }
    }
  }

  /** Write line to debug overlay */
  private appendDebug(msg: string): void {
    try {
      const dbg = document.getElementById("debug-overlay");
      if (dbg) {
        const line = document.createElement("div");
        line.textContent = msg;
        dbg.appendChild(line);
      }
    } catch (_) { /* ignore */ }
  }

  /** Track 1-2-3-4-5 sequence while button has focus → toggle debug overlay */
  private checkSecretSequence(keyCode: number): void {
    if (this.focusIndex !== 2) {
      this.keySequence = [];
      return;
    }
    this.keySequence.push(keyCode);
    if (this.keySequence.length > this.SECRET_SEQUENCE.length) {
      this.keySequence.shift();
    }
    if (this.keySequence.length === this.SECRET_SEQUENCE.length &&
        this.keySequence.every((k, i) => k === this.SECRET_SEQUENCE[i])) {
      this.keySequence = [];
      this.toggleDebugOverlay();
    }
  }

  private toggleDebugOverlay(): void {
    const dbg = document.getElementById("debug-overlay");
    if (!dbg) return;
    const isHidden = dbg.style.display === "none" || dbg.style.display === "";
    dbg.style.display = isHidden ? "block" : "none";
  }

  private showToast(msg: string, type: "info" | "error"): void {
    if (this.toastTimer !== null) clearTimeout(this.toastTimer);
    this.toastEl.textContent = msg;
    this.toastEl.className = "login-toast " + type + " show";
    this.toastTimer = window.setTimeout(() => {
      this.toastEl.classList.remove("show");
      this.toastTimer = null;
    }, 4000);
  }
}
