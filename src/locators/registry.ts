import { Page, Locator } from "playwright";
import * as fs from "fs";
import * as path from "path";

const locatorsDir = path.resolve(__dirname);
const locatorsMap: Record<string, Record<string, string>> = {};

for (const file of fs.readdirSync(locatorsDir)) {
  if (file.endsWith(".locators.json")) {
    // eslint-disable-next-line @typescript-eslint/no-var-requires
    const data: Record<string, Record<string, string>> = require(path.join(locatorsDir, file));
    Object.assign(locatorsMap, data);
  }
}

export function rawLocator(name: string, page: Page): Locator {
  const entry = locatorsMap[name];
  if (!entry) throw new Error(`Locator "${name}" not found in any *.locators.json`);
  const selector = entry["css"];
  if (!selector) throw new Error(`Locator "${name}" has no css selector defined`);
  return page.locator(selector);
}

/**
 * Is the element actually clickable, or is something layered on top of it?
 *
 * `count() > 0` only proves the element is in the DOM, and `toBeVisible()` passes
 * even for an element fully covered by an overlay — so neither distinguishes the
 * real, interactive copy of a button from one buried under the collapsed widget
 * handle on another window. We replicate Playwright's own hit-test: a click lands
 * only if the topmost element at the click point IS the target or a descendant of
 * it. Returns false (never throws) if the element is detached, zero-size, or
 * scrolled out of view — callers fall back to the present-but-obstructed match.
 */
async function isClickable(locator: Locator): Promise<boolean> {
  try {
    return await locator.first().evaluate((el) => {
      const rect = (el as HTMLElement).getBoundingClientRect();
      if (rect.width === 0 || rect.height === 0) return false;
      const cx = rect.left + rect.width / 2;
      const cy = rect.top + rect.height / 2;
      const top = document.elementFromPoint(cx, cy);
      // Clickable when the hit target is the element itself or something inside it
      // (e.g. an icon span); an overlay or ancestor on top means interception.
      return !!top && (top === el || el.contains(top));
    });
  } catch {
    return false;
  }
}

function strategiesFor(entry: Record<string, string>, page: Page): Array<{ key: string; build: () => Locator }> {
  return [
    entry["css"]         ? { key: "css",         build: () => page.locator(entry["css"]) }                                        : null,
    entry["text"]        ? { key: "text",        build: () => page.getByText(entry["text"], { exact: false }) }                   : null,
    entry["placeholder"] ? { key: "placeholder", build: () => page.getByPlaceholder(entry["placeholder"]) }                       : null,
    entry["role"]        ? { key: "role",        build: () => page.getByRole(entry["role"] as Parameters<Page["getByRole"]>[0]) } : null,
  ].filter(Boolean) as Array<{ key: string; build: () => Locator }>;
}

/**
 * Resolve a locator by searching across ALL candidate windows.
 *
 * The widget spawns multiple windows (collapsed icon window + feedback-UI window),
 * and their ordering is not stable. Rather than guess which single page is "active"
 * — which caused random pass/fail — we poll every window each tick and return the
 * locator from whichever DOM actually contains the element. Deterministic regardless
 * of window count or ordering.
 */
export async function getLocator(name: string, pagesFn: () => Page[]): Promise<Locator> {
  const entry = locatorsMap[name];
  if (!entry) throw new Error(`Locator "${name}" not found in any *.locators.json`);

  const TIMEOUT = 60_000;
  const INTERVAL = 500;
  const deadline = Date.now() + TIMEOUT;

  while (Date.now() < deadline) {
    const pages = pagesFn();

    // Within a single tick, scan every window/strategy. Prefer a copy of the
    // element that is actually clickable (not buried under an overlay); fall
    // back to the first present-but-obstructed match so visibility-only
    // assertions still resolve immediately instead of waiting out the timeout.
    let present: { locator: Locator; info: string } | null = null;

    for (const page of pages) {
      for (const { key, build } of strategiesFor(entry, page)) {
        try {
          const locator = build();
          if ((await locator.count()) > 0) {
            if (await isClickable(locator)) {
              console.log(`[registry] "${name}" found (clickable) via ${key} on page: ${page.url()}`);
              return locator;
            }
            if (!present) present = { locator, info: `${key} on page: ${page.url()}` };
          }
        } catch {
          // page may have navigated/closed mid-call — ignore and continue
        }
      }
    }

    if (present) {
      console.log(`[registry] "${name}" present but obstructed on every window; returning ${present.info}`);
      return present.locator;
    }

    await new Promise((r) => setTimeout(r, INTERVAL));
  }

  const urls = pagesFn().map((p) => p.url()).join(", ") || "(no pages)";
  throw new Error(
    `Could not locate element "${name}" within ${TIMEOUT / 1000}s across any window.\nSearched pages: ${urls}`
  );
}
