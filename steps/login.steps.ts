import { Given, When, Then } from "@cucumber/cucumber";
import { expect } from "@playwright/test";
import { execSync } from "child_process";
import * as path from "path";
import { ICustomWorld } from "../features/support/world";

const PSCONSOLE = path.resolve(__dirname, "../APIClient_sourcecode/PSConsole/bin/Debug/PSConsole.exe");

Given("PowerScribe is connected", async function () {
  execSync(`"${PSCONSOLE}" connect`, { stdio: "inherit" });

  // Wait until PSConsole reports a connected (non-Disconnected) state before
  // proceeding. The widget needs time to establish its channel to the PowerScribe
  // server after a fresh start — firing login too soon causes "Login timed out".
  const TIMEOUT = 30_000;
  const INTERVAL = 2_000;
  const deadline = Date.now() + TIMEOUT;
  while (Date.now() < deadline) {
    await new Promise((r) => setTimeout(r, INTERVAL));
    try {
      const status = execSync(`"${PSCONSOLE}" status`, { encoding: "utf8" });
      if (!status.includes("Disconnected")) return;
    } catch {}
  }
  throw new Error("PowerScribe connection timed out after 30 s — status never left Disconnected");
});

When(
  "I login to PowerScribe with username {string} and password {string}",
  async function (username: string, password: string) {
    execSync(`"${PSCONSOLE}" login --user ${username} --password ${password}`, {
      stdio: "inherit",
    });

    // Poll PSConsole status every 2 s instead of a fixed 60 s sleep.
    // Exits as soon as the session is LoggedIn; throws if 60 s elapses.
    const TIMEOUT = 60_000;
    const INTERVAL = 2_000;
    const deadline = Date.now() + TIMEOUT;
    while (Date.now() < deadline) {
      await new Promise((r) => setTimeout(r, INTERVAL));
      try {
        const status = execSync(`"${PSCONSOLE}" status`, { encoding: "utf8" });
        if (status.includes("LoggedIn")) return;
      } catch {}
    }
    throw new Error("PowerScribe login timed out after 60 s — status never reached LoggedIn");
  }
);

When(
  "I enter username {string} and password {string}",
  async function (this: ICustomWorld, username: string, password: string) {
    const usernameInput = await this.getLocator("usernameInput");
    const passwordInput = await this.getLocator("passwordInput");
    await usernameInput.fill(username);
    await passwordInput.fill(password);
  }
);

When("I click the login button", async function (this: ICustomWorld) {
  const loginButton = await this.getLocator("loginButton");
  await loginButton.click();
});

Then(
  "disconnect button should not be visibled on widget",
  async function (this: ICustomWorld) {
    await expect(this.rawLocator("disconnectButton")).not.toBeVisible({ timeout: 10_000 });
  }
);

Then(
  "I should be redirected to the dashboard",
  async function (this: ICustomWorld) {
    if (this.page) {
      const header = await this.getLocator("dashboardHeader");
      await expect(header).toBeVisible({ timeout: 10_000 });
    } else {
      const output = execSync(`"${PSCONSOLE}" status`, { encoding: "utf8" });
      if (!output.includes("LoggedIn")) {
        throw new Error(`Expected PowerScribe session to be LoggedIn.\n${output}`);
      }
    }
  }
);

Then(
  "I should see an error message {string}",
  async function (this: ICustomWorld, message: string) {
    const errorEl = await this.getLocator("loginErrorMessage");
    await expect(errorEl).toBeVisible();
    await expect(errorEl).toContainText(message);
  }
);

Then(
  "I should see a validation error on the login form",
  async function (this: ICustomWorld) {
    const validationError = await this.getLocator("loginValidationError");
    await expect(validationError).toBeVisible();
  }
);
