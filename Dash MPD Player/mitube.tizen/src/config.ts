export interface AppConfig {
  serverUrl: string;
}

const DEFAULT_URL = "http://192.168.0.173:5241";

export const CONFIG: AppConfig = { serverUrl: DEFAULT_URL };

export function loadConfig(): void {
  CONFIG.serverUrl = DEFAULT_URL;
}
