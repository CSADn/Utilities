// Module-level diagnostic: confirms bundle.js executed + base URL info
(() => {
  const dbg = document.getElementById("debug-overlay");
  if (dbg) {
    const line = document.createElement("div");
    line.textContent = `[Bundle] OK href=${location.href} base=${document.baseURI || "?"}`;
    dbg.appendChild(line);
  }

  // webpack: set public path to the directory of this script + "dist/"
  const scripts = document.getElementsByTagName("script");
  for (let i = 0; i < scripts.length; i++) {
    const s = scripts[i];
    if (s.src && s.src.includes("bundle.js")) {
      const dir = s.src.substring(0, s.src.lastIndexOf("/") + 1);
      (window as any).__webpack_public_path__ = dir;
      break;
    }
  }
})();

import { loadConfig } from "./config";
import { setToken, validateToken } from "./api/client";
import { SessionStore } from "./services/SessionStore";
import { LoginView } from "./ui/LoginView";
import { BrowseView } from "./ui/BrowseView";
import { PlayerView } from "./ui/PlayerView";
import { SettingsView } from "./ui/SettingsView";

const VIEWS = {
  login: "login-view",
  browse: "browse-view",
  player: "player-view",
  settings: "settings-view",
} as const;

type ViewId = (typeof VIEWS)[keyof typeof VIEWS];

function showView(id: ViewId): void {
  document.querySelectorAll(".view").forEach((el) => el.classList.remove("active"));
  document.getElementById(id)?.classList.add("active");
}

async function main(): Promise<void> {
  await loadConfig();

  const loginView = new LoginView(VIEWS.login);
  const browseView = new BrowseView(VIEWS.browse);
  const playerView = new PlayerView(VIEWS.player);
  const settingsView = new SettingsView(VIEWS.settings);

  function goToLogin(): void {
    loginView.show();
    showView(VIEWS.login);
  }

  function goToBrowse(restore?: boolean): void {
    browseView.show(restore);
    showView(VIEWS.browse);
    setTimeout(() => browseView.focus(), 200);
  }

  function goToPlayer(channel: any): void {
    browseView.saveState();
    playerView.show(channel);
    showView(VIEWS.player);
  }

  function goToSettings(): void {
    settingsView.show();
    showView(VIEWS.settings);
  }

  loginView.setOnSuccess(() => goToBrowse());

  browseView.setOnChannelSelect((ch) => goToPlayer(ch));
  browseView.setOnSettings(() => goToSettings());

  playerView.setOnBack(() => goToBrowse(true));

  settingsView.setOnLogout(() => goToLogin());
  settingsView.setOnClose(() => goToBrowse());

  // Session check
  if (SessionStore.isLoggedIn) {
    const token = SessionStore.token!;
    setToken(token);
    try {
      await validateToken();
      goToBrowse();
      return;
    } catch {
      SessionStore.clear();
      setToken(null);
    }
  }

  goToLogin();
}

document.addEventListener("DOMContentLoaded", () => {
  main().catch((e) => {
    console.error("[App] Fatal:", e);
    const dbg = document.getElementById("debug-overlay");
    if (dbg) {
      const line = document.createElement("div");
      line.className = "error";
      line.textContent = "[App] Fatal: " + (e.message || e);
      dbg.appendChild(line);
    }
  });
});
