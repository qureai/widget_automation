import { setWorldConstructor, World, IWorldOptions } from "@cucumber/cucumber";
import { ElectronApplication, Page, Locator, Browser } from "playwright";
import { ChildProcess } from "child_process";
import { getLocator, rawLocator } from "../../src/locators/registry";

export interface ICustomWorld extends World {
  electronApp: ElectronApplication | null;
  cdpBrowser: Browser | null;
  page: Page | null;
  widgetProcess: ChildProcess | null;
  readonly activePages: Page[];
  getLocator(name: string): Promise<ReturnType<Page["locator"]>>;
  rawLocator(name: string): Locator;
  takeScreenshot(name: string): Promise<void>;
}

class CustomWorld extends World implements ICustomWorld {
  electronApp: ElectronApplication | null = null;
  cdpBrowser: Browser | null = null;
  page: Page | null = null;
  widgetProcess: ChildProcess | null = null;

  constructor(options: IWorldOptions) {
    super(options);
  }

  /**
   * All candidate windows that could hold the element we're looking for.
   *
   * The widget runs MORE THAN ONE window: the collapsed icon window and, once an
   * accession is opened, a separate feedback-UI window. The page ordering returned
   * by Playwright is not stable, so we must NOT guess a single "active" page —
   * doing so caused random pass/fail depending on which window happened to be last.
   * Instead the registry searches every window and uses whichever one has the element.
   */
  get activePages(): Page[] {
    if (this.electronApp) {
      return this.electronApp.windows();
    }
    if (this.cdpBrowser) {
      const pages = this.cdpBrowser.contexts()[0]?.pages() ?? [];
      return pages.filter((p) => {
        const url = p.url();
        return !url.startsWith("devtools://") && url !== "about:blank";
      });
    }
    return this.page ? [this.page] : [];
  }

  /** Best single page — used for screenshots and rawLocator only. */
  private get activePage(): Page {
    const pages = this.activePages;
    if (pages.length > 0) return pages[pages.length - 1];
    if (!this.page) throw new Error("Page not initialized — app not launched");
    return this.page;
  }

  async getLocator(name: string) {
    return getLocator(name, () => this.activePages);
  }

  rawLocator(name: string): Locator {
    return rawLocator(name, this.activePage);
  }

  async takeScreenshot(name: string) {
    if (!this.electronApp && !this.cdpBrowser && !this.page) return;
    const ts = new Date().toISOString().replace(/[:.]/g, "-");
    await this.activePage.screenshot({
      path: `reports/screenshots/${name}-${ts}.png`,
      fullPage: true,
    });
  }
}

setWorldConstructor(CustomWorld);
