import { Page } from "playwright";

export interface DomSnapshot {
  timestamp: string;
  url: string;
  elements: ElementInfo[];
}

export interface ElementInfo {
  tag: string;
  id?: string;
  classes: string[];
  text?: string;
  role?: string;
  placeholder?: string;
  selector: string;
}

export async function takeDomSnapshot(page: Page): Promise<DomSnapshot> {
  const elements = await page.evaluate(() => {
    const results: ElementInfo[] = [];
    const interactable = document.querySelectorAll(
      "input, button, select, textarea, a, [role], [id], [data-testid]"
    );

    interactable.forEach((el) => {
      const tag = el.tagName.toLowerCase();
      const id = el.id || undefined;
      const classes = Array.from(el.classList);
      const text = el.textContent?.trim().slice(0, 100) || undefined;
      const role = el.getAttribute("role") || undefined;
      const placeholder = (el as HTMLInputElement).placeholder || undefined;

      let selector = tag;
      if (id) selector = `#${id}`;
      else if (classes.length) selector = `${tag}.${classes[0]}`;

      results.push({ tag, id, classes, text, role, placeholder, selector });
    });

    return results;
  });

  return {
    timestamp: new Date().toISOString(),
    url: page.url(),
    elements,
  };
}

export function diffSnapshots(before: DomSnapshot, after: DomSnapshot): {
  added: ElementInfo[];
  removed: ElementInfo[];
} {
  const beforeSelectors = new Set(before.elements.map((e) => e.selector));
  const afterSelectors = new Set(after.elements.map((e) => e.selector));

  return {
    added: after.elements.filter((e) => !beforeSelectors.has(e.selector)),
    removed: before.elements.filter((e) => !afterSelectors.has(e.selector)),
  };
}
