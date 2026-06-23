import { When, Then } from "@cucumber/cucumber";
import { expect } from "@playwright/test";
import { ICustomWorld } from "../features/support/world";

When(
  "I select {string} from the theme dropdown",
  async function (this: ICustomWorld, theme: string) {
    const dropdown = await this.getLocator("themeDropdown");
    await dropdown.selectOption({ label: theme });
  }
);

When(
  "I click the save settings button",
  async function (this: ICustomWorld) {
    const saveBtn = await this.getLocator("saveSettingsButton");
    await saveBtn.click();
  }
);

When(
  "I update the display name to {string}",
  async function (this: ICustomWorld, name: string) {
    const input = await this.getLocator("displayNameInput");
    await input.fill(name);
  }
);

Then(
  "a success notification should appear",
  async function (this: ICustomWorld) {
    const notification = await this.getLocator("successNotification");
    await expect(notification).toBeVisible({ timeout: 5_000 });
  }
);

Then(
  "the application theme should be {string}",
  async function (this: ICustomWorld, theme: string) {
    const themeClass = theme.toLowerCase();
    await expect(this.rawLocator("bodyElement")).toHaveClass(
      new RegExp(`theme-${themeClass}`),
      { timeout: 5_000 }
    );
  }
);

Then(
  "the theme should still be {string}",
  async function (this: ICustomWorld, theme: string) {
    const themeClass = theme.toLowerCase();
    await expect(this.rawLocator("bodyElement")).toHaveClass(
      new RegExp(`theme-${themeClass}`),
      { timeout: 5_000 }
    );
  }
);
