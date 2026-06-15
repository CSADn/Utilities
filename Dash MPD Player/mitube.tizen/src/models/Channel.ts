export interface DrmInfo {
  scheme: string;
  licenseUri: string;
}

export interface Channel {
  name: string;
  url: string;
  type: string;
  drm_license_uri?: string;
  icono?: string;
  headers?: Record<string, string>;
}

/** Parse Clearkey license URI format: https://server/?keyid=<uuid>&key=<hex> */
export function parseClearkeyLicense(drmLicenseUri: string): { keyId: string; key: string } | null {
  if (!drmLicenseUri) return null;
  try {
    const u = new URL(drmLicenseUri);
    const keyId = u.searchParams.get("keyid") || "";
    const key = u.searchParams.get("key") || "";
    if (keyId && key) return { keyId: keyId.replace(/-/g, ""), key };
  } catch {
    // fallback: colon-separated format
    const parts = drmLicenseUri.split(":");
    if (parts.length === 2) return { keyId: parts[0].replace(/-/g, ""), key: parts[1] };
  }
  return null;
}
