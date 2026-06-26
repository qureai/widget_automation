import { When, Then } from "@cucumber/cucumber";
import { expect } from "@playwright/test";
import { execSync } from "child_process";
import * as path from "path";
import { ICustomWorld } from "../features/support/world";

const PSCONSOLE = path.resolve(__dirname, "../APIClient_sourcecode/PSConsole/bin/Debug/PSConsole.exe");

When("I open accession {string} on PowerScribe", function (accession: string) {
  execSync(`"${PSCONSOLE}" open --accession ${accession}`, { stdio: "inherit" });
});

Then("the thumbs up button should be visible", async function (this: ICustomWorld) {
  const button = await this.getLocator("thumbsUpButton");
  await expect(button).toBeVisible({ timeout: 15_000 });
});

Then("the thumbs down button should be visible", async function (this: ICustomWorld) {
  const button = await this.getLocator("thumbsDownButton");
  await expect(button).toBeVisible({ timeout: 15_000 });
});

Then("the thumbs up button should be in default state", async function (this: ICustomWorld) {
  const button = await this.getLocator("thumbsUpButtonDefaultState");
  await expect(button).toBeVisible({ timeout: 15_000 });
});

Then("the thumbs down button should be in default state", async function (this: ICustomWorld) {
  const button = await this.getLocator("thumbsDownButtonDefaultState");
  await expect(button).toBeVisible({ timeout: 15_000 });
});

When("I click the thumbs up button", async function (this: ICustomWorld) {
  const button = await this.getLocator("thumbsUpButton");
  await button.click();
});

When("I click the thumbs down button", async function (this: ICustomWorld) {
  const button = await this.getLocator("thumbsDownButton");
  await button.click();
});

Then("negative feedback modal should be displayed", async function (this: ICustomWorld) {
  // Clicking thumbs down spawns a SEPARATE Electron window whose DOM holds the
  // feedback modal — the "Give additional feedback" header lives only there.
  // getLocator polls across every window (re-fetching the window list each tick),
  // so the freshly-spawned window is found without guessing which page is active.
  const header = await this.getLocator("negativeFeedbackModalHeader");
  await expect(header).toBeVisible({ timeout: 15_000 });
});

Then("the thumbs down button should be in active state", async function (this: ICustomWorld) {
  const button = await this.getLocator("thumbsDownButtonPressedState");
  await expect(button).toBeVisible({ timeout: 15_000 });
});

// Maps a feedback-modal option label to its base locator key. The locators follow
// the pattern `<base>Option` / `<base>OptionActive` / `<base>OptionDefault`.
// "Other" is accepted as an alias for the actual button text "Others".
const FEEDBACK_OPTIONS: Record<string, string> = {
  "False Negative": "falseNegative",
  "False Positive": "falsePositive",
  "Performance lag": "performanceLag",
  "Connection error": "connectionError",
  "Other": "others",
  "Others": "others",
};

function optionBase(label: string): string {
  const base = FEEDBACK_OPTIONS[label.trim()];
  if (!base) throw new Error(`Unknown feedback option "${label}"`);
  return base;
}

// Regex (not a {string} expression) so the feature can read naturally without quotes,
// e.g. "Then able to click on False Negative" / "Then able to click on Submit button".
Then(/^able to click on (.+)$/, async function (this: ICustomWorld, label: string) {
  const trimmed = label.trim();
  const locatorName = /^submit\b/i.test(trimmed) ? "submitButton" : `${optionBase(trimmed)}Option`;
  const button = await this.getLocator(locatorName);
  await button.click();
});

// After clicking, the selected option turns teal (bg-teal-600).
Then("the {string} option should be selected", async function (this: ICustomWorld, label: string) {
  const button = await this.getLocator(`${optionBase(label)}OptionActive`);
  await expect(button).toBeVisible({ timeout: 15_000 });
});

// Before clicking, the option sits in its default dark state (bg-gray-900).
Then("the {string} option should be in default state", async function (this: ICustomWorld, label: string) {
  const button = await this.getLocator(`${optionBase(label)}OptionDefault`);
  await expect(button).toBeVisible({ timeout: 15_000 });
});

Then("the message should populate as submitted", async function (this: ICustomWorld) {
  // "Submitted!" flashes for ~1 second — Playwright's built-in retry (~100 ms intervals)
  // is far more reliable here than the registry's 500 ms polling loop.
  let lastError: Error | null = null;
  for (const page of this.activePages) {
    try {
      await expect(page.locator("//div[text()='Submitted!']")).toBeVisible({ timeout: 5_000 });
      return;
    } catch (e) {
      lastError = e as Error;
    }
  }
  throw lastError ?? new Error('"Submitted!" banner was not visible on any page within 5 s');
});

Then("the thumbs up button should be in active state", async function (this: ICustomWorld) {
  const button = await this.getLocator("thumbsUpButtonPressedState");
  await expect(button).toBeVisible({ timeout: 15_000 });
});

