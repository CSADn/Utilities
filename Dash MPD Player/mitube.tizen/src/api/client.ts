import { CONFIG } from "../config";
import { ChannelGroup } from "../models/ChannelGroup";

export interface LoginRequest {
  username: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  expiresIn: number;
  username: string;
  displayName: string;
}

let authToken: string | null = null;

export function setToken(token: string | null): void {
  authToken = token;
}

export function getToken(): string | null {
  return authToken;
}

function baseUrl(): string {
  return CONFIG.serverUrl.replace(/\/+$/, "") + "/";
}

export class ApiError extends Error {
  constructor(public status: number, message: string) {
    super(message);
    this.name = "ApiError";
  }
}

function xhrRequest<T>(method: string, path: string, body?: string): Promise<T> {
  const url = baseUrl() + path.replace(/^\//, "");
  return new Promise((resolve, reject) => {
    const xhr = new XMLHttpRequest();
    xhr.open(method, url, true);
    xhr.timeout = 15000;
    xhr.setRequestHeader("Content-Type", "application/json");
    if (authToken) xhr.setRequestHeader("Authorization", `Bearer ${authToken}`);

    xhr.onreadystatechange = () => {
      if (xhr.readyState !== 4) return;
      const text = xhr.responseText || "";
      if (xhr.status >= 200 && xhr.status < 300) {
        if (!text) return reject(new Error("Empty response body"));
        try { resolve(JSON.parse(text) as T); }
        catch (e: any) { reject(new Error(`JSON parse error: ${text.substring(0, 100)}`)); }
      } else {
        reject(new ApiError(xhr.status, text.substring(0, 200) || `HTTP ${xhr.status}`));
      }
    };
    xhr.onerror = () => reject(new Error("XHR connection failed"));
    xhr.ontimeout = () => reject(new Error("XHR timeout"));

    xhr.send(body || undefined);
  });
}

/** Test connectivity: GET using XHR (proven working on Tizen Chromium 77) */
export async function ping(): Promise<string> {
  const url = baseUrl() + "api/auth/ping";
  return new Promise((resolve, reject) => {
    const xhr = new XMLHttpRequest();
    xhr.open("GET", url, true);
    xhr.timeout = 10000;
    xhr.onreadystatechange = () => {
      if (xhr.readyState !== 4) return;
      const text = xhr.responseText || "";
      if (xhr.status === 200 && text) return resolve(text);
      if (xhr.status === 200 && !text) return reject(new Error("Empty body"));
      reject(new Error(`HTTP ${xhr.status}: ${text.substring(0, 100)}`));
    };
    xhr.onerror = () => reject(new Error("XHR connection failed"));
    xhr.ontimeout = () => reject(new Error("XHR timeout"));
    xhr.send();
  });
}

export async function login(req: LoginRequest): Promise<LoginResponse> {
  return xhrRequest<LoginResponse>("POST", "api/auth/login", JSON.stringify(req));
}

export async function validateToken(): Promise<{ valid: boolean; username: string }> {
  return xhrRequest("GET", "api/auth/validate");
}

export async function getChannels(): Promise<ChannelGroup[]> {
  return xhrRequest<ChannelGroup[]>("GET", "api/channels");
}
