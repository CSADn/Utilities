import { getChannels, ApiError } from "../api/client";
import { SessionStore } from "../services/SessionStore";
import { ImageCache } from "../services/ImageCache";
import { Channel } from "../models/Channel";
import { ChannelGroup } from "../models/ChannelGroup";

interface FocusableItem {
  element: HTMLElement;
  row: number;
  col: number;
  channel?: Channel;
}

export class BrowseView {
  private container: HTMLElement;
  private categoryContainer: HTMLElement;
  private loadingText: HTMLElement;
  private errorText: HTMLElement;
  private countEl: HTMLElement;
  private userEl: HTMLElement;
  private settingsBtn: HTMLElement;

  private groups: ChannelGroup[] = [];
  private focusables: FocusableItem[] = [];
  private currentRow = 0;
  private currentCol = 0;
  private onChannelSelect: ((ch: Channel) => void) | null = null;
  private onSettings: (() => void) | null = null;
  private keyboardHandler: ((e: KeyboardEvent) => void) | null = null;
  private savedRow = 0;
  private savedCol = 0;

  constructor(containerId: string) {
    this.container = document.getElementById(containerId)!;
    this.categoryContainer = document.getElementById("category-container")!;
    this.loadingText = document.getElementById("loading-text")!;
    this.errorText = document.getElementById("error-text")!;
    this.countEl = document.getElementById("toolbar-count")!;
    this.userEl = document.getElementById("toolbar-user")!;
    this.settingsBtn = document.getElementById("toolbar-settings")!;
  }

  setOnChannelSelect(cb: (ch: Channel) => void): void {
    this.onChannelSelect = cb;
  }

  setOnSettings(cb: () => void): void {
    this.onSettings = cb;
  }

  saveState(): void {
    this.savedRow = this.currentRow;
    this.savedCol = this.currentCol;
  }

  show(restore?: boolean): void {
    this.container.classList.add("active");
    this.userEl.textContent = SessionStore.displayName || SessionStore.username || "";
    this.loadChannels(restore);
    this.bindKeyboard();
    this.settingsBtn.addEventListener("click", () => this.onSettings?.());
  }

  hide(): void {
    this.container.classList.remove("active");
    if (this.keyboardHandler) {
      document.removeEventListener("keydown", this.keyboardHandler);
      this.keyboardHandler = null;
    }
  }

  focus(): void {
    this.focusCurrent();
  }

  private async loadChannels(restore?: boolean): Promise<void> {
    this.loadingText.style.display = "block";
    this.errorText.style.display = "none";
    this.categoryContainer.innerHTML = "";
    try {
      this.groups = await getChannels();
      this.renderCategories(restore);
      this.loadingText.style.display = "none";
    } catch (e: any) {
      this.loadingText.style.display = "none";
      if (e instanceof ApiError && e.status === 401) {
        SessionStore.clear();
        window.location.reload();
        return;
      }
      this.errorText.textContent = "No se pudieron cargar los canales";
      this.errorText.style.display = "block";
    }
  }

  private renderCategories(restore?: boolean): void {
    this.categoryContainer.innerHTML = "";
    this.focusables = [];

    this.groups.forEach((group, rowIdx) => {
      const section = document.createElement("div");
      section.className = "category-section";

      const header = document.createElement("h2");
      header.className = "category-header";
      header.textContent = group.name;
      section.appendChild(header);

      const row = document.createElement("div");
      row.className = "horizontal-row";
      row.id = `row-${rowIdx}`;

      group.samples.forEach((channel, colIdx) => {
        const card = document.createElement("div");
        card.className = "channel-card";
        card.tabIndex = -1;
        card.setAttribute("data-row", String(rowIdx));
        card.setAttribute("data-col", String(colIdx));

        const icon = document.createElement("img");
        icon.className = "channel-icon";
        icon.alt = channel.name;
        icon.loading = "lazy";
        const fallbackSrc = "assets/icons/placeholder.png";
        icon.onerror = () => {
          if (icon.src !== fallbackSrc && !icon.src.startsWith("blob:")) {
            icon.src = fallbackSrc;
          } else {
            icon.onerror = null;
          }
        };

        if (channel.icono && !channel.icono.startsWith("assets/")) {
          const cached = ImageCache.getSync(channel.icono);
          if (cached) {
            icon.src = cached;
          } else {
            icon.src = "assets/icons/placeholder.png";
            ImageCache.get(channel.icono).then((blobUrl) => {
              if (icon.isConnected) icon.src = blobUrl;
            });
          }
        } else {
          icon.src = channel.icono || "assets/icons/placeholder.png";
        }

        const name = document.createElement("span");
        name.className = "channel-name";
        name.textContent = channel.name;

        card.appendChild(icon);
        card.appendChild(name);

        card.addEventListener("click", () => this.onChannelSelect?.(channel));
        card.addEventListener("focus", () => {
          this.currentRow = rowIdx;
          this.currentCol = colIdx;
          this.scrollToCard(rowIdx, colIdx);
        });

        this.focusables.push({ element: card, row: rowIdx, col: colIdx, channel });
        row.appendChild(card);
      });

      section.appendChild(row);
      this.categoryContainer.appendChild(section);
    });

    const total = this.groups.reduce((sum, g) => sum + g.samples.length, 0);
    this.countEl.textContent = `${total} canales`;

    if (this.focusables.length > 0) {
      if (restore) {
        setTimeout(() => {
          const item = this.focusables.find(f => f.row === this.savedRow && f.col === this.savedCol);
          if (item) {
            this.currentRow = this.savedRow;
            this.currentCol = this.savedCol;
            item.element.focus();
            this.scrollToCard(item.row, item.col);
          } else {
            this.focusables[0].element.focus();
          }
        }, 150);
      } else {
        setTimeout(() => this.focusables[0].element.focus(), 150);
      }
    }
  }

  private scrollToCard(row: number, col: number): void {
    const rowEl = document.getElementById(`row-${row}`);
    if (!rowEl) return;
    const cards = rowEl.querySelectorAll(".channel-card");
    const target = cards[col] as HTMLElement | undefined;
    if (target) {
      target.scrollIntoView({ behavior: "smooth", block: "nearest", inline: "center" });
    }
  }

  private focusCurrent(): void {
    const item = this.focusables.find(f => f.row === this.currentRow && f.col === this.currentCol);
    item?.element.focus();
    if (item) this.scrollToCard(item.row, item.col);
  }

  private bindKeyboard(): void {
    if (this.keyboardHandler) {
      document.removeEventListener("keydown", this.keyboardHandler);
    }
    this.keyboardHandler = (e: KeyboardEvent) => {
      if (!this.container.classList.contains("active")) return;
      const rows = this.groups.length;
      const cols = this.groups[this.currentRow]?.samples.length || 0;

      switch (e.key) {
        case "ArrowUp":
          e.preventDefault();
          if (this.currentRow > 0) { this.currentRow--; this.currentCol = Math.min(this.currentCol, this.groups[this.currentRow]?.samples.length - 1 || 0); this.focusCurrent(); }
          break;
        case "ArrowDown":
          e.preventDefault();
          if (this.currentRow < rows - 1) { this.currentRow++; this.currentCol = Math.min(this.currentCol, this.groups[this.currentRow]?.samples.length - 1 || 0); this.focusCurrent(); }
          break;
        case "ArrowLeft":
          e.preventDefault();
          if (this.currentCol > 0) { this.currentCol--; this.focusCurrent(); }
          break;
        case "ArrowRight":
          e.preventDefault();
          if (this.currentCol < cols - 1) { this.currentCol++; this.focusCurrent(); }
          break;
        case "Enter":
          e.preventDefault();
          const item = this.focusables.find(f => f.row === this.currentRow && f.col === this.currentCol);
          if (item?.channel) this.onChannelSelect?.(item.channel);
          break;
      }
    };
    document.addEventListener("keydown", this.keyboardHandler);
  }
}
