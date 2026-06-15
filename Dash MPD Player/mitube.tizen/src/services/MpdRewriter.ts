const WIDEVINE_UUID = "edef8ba9-79d6-4ace-a3c8-27dcd51d21ed";
const PLAYREADY_UUID = "9a04f079-9840-4286-ab92-e65be0885f95";
const CLEARKEY_UUID = "e2719d58-a985-b3c9-781a-b030af78d30e";

export interface DrmInfo {
  scheme: string;
  licenseUri: string;
}

function rewrite(mpdXml: string, token: string): string {
  const proxyBase = `http://localhost:${location.port}`;
  return mpdXml.replace(
    /<BaseURL>([^<]+)<\/BaseURL>/g,
    (_match, url: string) => {
      const abs = resolveUrl(url, mpdXml);
      return `<BaseURL>${proxyBase}/segment/${token}/${encodeURIComponent(abs)}</BaseURL>`;
    }
  ).replace(
    /(?:SegmentURL\s*media="([^"]+)")|(?:media\s*="([^"]+)")/g,
    (_match, g1: string, g2: string) => {
      const url = g1 || g2;
      const abs = resolveUrl(url, mpdXml);
      const proxied = `${proxyBase}/segment/${token}/${encodeURIComponent(abs)}`;
      return g1
        ? `SegmentURL media="${proxied}"`
        : `media="${proxied}"`;
    }
  );
}

function detectDrm(mpdXml: string): DrmInfo | null {
  const lower = mpdXml.toLowerCase();
  const widevine = lower.includes(WIDEVINE_UUID);
  const playready = lower.includes(PLAYREADY_UUID);
  const clearkey = lower.includes(CLEARKEY_UUID);

  const laurlMatch = mpdXml.match(/<dashif:laurl[^>]*>([^<]+)<\/dashif:laurl>/i);
  const msproMatch = mpdXml.match(/<mspr:pro[^>]*>([^<]+)<\/mspr:pro>/i);

  if (widevine) {
    return { scheme: "widevine", licenseUri: laurlMatch?.[1] || "" };
  }
  if (playready) {
    const licenseUri = laurlMatch?.[1] || (msproMatch ? extractPlayReadyUrl(msproMatch[1]) : "");
    return { scheme: "playready", licenseUri };
  }
  if (clearkey) {
    return { scheme: "clearkey", licenseUri: "" };
  }
  return null;
}

function extractPlayReadyUrl(proData: string): string {
  try {
    const hex = atob(proData);
    const urlMatch = hex.match(/https?:\/\/[^\0"]+/);
    return urlMatch ? urlMatch[0] : "";
  } catch {
    return "";
  }
}

function resolveUrl(url: string, mpdXml: string): string {
  if (url.startsWith("http://") || url.startsWith("https://")) return url;
  const baseUrlMatch = mpdXml.match(/<BaseURL>([^<]+)<\/BaseURL>/);
  const base = baseUrlMatch ? baseUrlMatch[1].replace(/\/+$/, "") : "";
  return base ? `${base}/${url.replace(/^\//, "")}` : url;
}

export const MpdRewriter = { rewrite, detectDrm };
