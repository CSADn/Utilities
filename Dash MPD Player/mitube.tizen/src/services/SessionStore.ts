const KEY_TOKEN = "mitube_token";
const KEY_USERNAME = "mitube_username";
const KEY_DISPLAY_NAME = "mitube_display_name";

export const SessionStore = {
  get token(): string | null {
    return localStorage.getItem(KEY_TOKEN);
  },
  set token(v: string | null) {
    if (v) localStorage.setItem(KEY_TOKEN, v);
    else localStorage.removeItem(KEY_TOKEN);
  },

  get username(): string | null {
    return localStorage.getItem(KEY_USERNAME);
  },
  set username(v: string | null) {
    if (v) localStorage.setItem(KEY_USERNAME, v);
    else localStorage.removeItem(KEY_USERNAME);
  },

  get displayName(): string | null {
    return localStorage.getItem(KEY_DISPLAY_NAME);
  },
  set displayName(v: string | null) {
    if (v) localStorage.setItem(KEY_DISPLAY_NAME, v);
    else localStorage.removeItem(KEY_DISPLAY_NAME);
  },

  get isLoggedIn(): boolean {
    return this.token !== null;
  },

  clear(): void {
    localStorage.removeItem(KEY_TOKEN);
    localStorage.removeItem(KEY_USERNAME);
    localStorage.removeItem(KEY_DISPLAY_NAME);
  },
};
