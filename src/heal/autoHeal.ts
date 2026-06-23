import { Page } from "playwright";
import { takeDomSnapshot } from "./domDiff";

interface LocatorEntry {
  css?: string;
  text?: string;
  placeholder?: string;
  role?: string;
  [key: string]: string | undefined;
}

/**
 * Attempts to find a best-match selector for a broken locator by
 * scanning the live DOM for elements whose attributes partially match
 * the recorded locator hints.
 */
export async function findBestMatch(
  name: string,
  entry: LocatorEntry,
  page: Page
): Promise<string | null> {
  const snapshot = await takeDomSnapshot(page);

  // Score each DOM element against the hints we have
  let bestSelector: string | null = null;
  let bestScore = 0;

  for (const el of snapshot.elements) {
    let score = 0;

    if (entry.text && el.text?.toLowerCase().includes(entry.text.toLowerCase())) score += 3;
    if (entry.role && el.role === entry.role) score += 2;
    if (entry.placeholder && el.placeholder === entry.placeholder) score += 2;
    if (entry.css) {
      // Check if tag or a class fragment matches the css hint
      const cssLower = entry.css.toLowerCase();
      if (cssLower.includes(el.tag)) score += 1;
      for (const cls of el.classes) {
        if (cssLower.includes(cls)) score += 1;
      }
    }

    if (score > bestScore) {
      bestScore = score;
      bestSelector = el.selector;
    }
  }

  if (bestScore >= 2) {
    console.warn(`[autoHeal] "${name}" healed → "${bestSelector}" (score ${bestScore})`);
    return bestSelector;
  }

  return null;
}
