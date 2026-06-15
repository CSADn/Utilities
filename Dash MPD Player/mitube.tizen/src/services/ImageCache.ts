export class ImageCache {
  private static cache = new Map<string, string>();

  static getSync(url: string): string | undefined {
    return this.cache.get(url);
  }

  static async get(url: string): Promise<string> {
    const cached = this.cache.get(url);
    if (cached) return cached;
    const blobUrl = await this.fetchAsBlob(url);
    this.cache.set(url, blobUrl);
    return blobUrl;
  }

  private static fetchAsBlob(url: string): Promise<string> {
    return new Promise((resolve) => {
      const xhr = new XMLHttpRequest();
      xhr.open("GET", url, true);
      xhr.responseType = "blob";
      xhr.onreadystatechange = () => {
        if (xhr.readyState !== 4) return;
        if (xhr.status >= 200 && xhr.status < 300) {
          const blob = xhr.response as Blob;
          resolve(URL.createObjectURL(blob));
        } else {
          resolve(url);
        }
      };
      xhr.onerror = () => resolve(url);
      xhr.send();
    });
  }

  static clear(): void {
    for (const blobUrl of this.cache.values()) {
      if (blobUrl.startsWith("blob:")) URL.revokeObjectURL(blobUrl);
    }
    this.cache.clear();
  }
}
