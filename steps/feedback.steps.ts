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
