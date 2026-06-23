import { Given, When } from "@cucumber/cucumber";
import { ICustomWorld } from "../features/support/world";

Given("the application is launched", async function (this: ICustomWorld) {
  if (this.page) {
    await this.page.waitForLoadState("domcontentloaded");
  } else if (this.widgetProcess) {
    // CDP endpoint already confirmed ready by launchWidgetFromSource — no extra wait needed
  }
});

Given(
  "I am logged in as {string} with password {string}",
  async function (this: ICustomWorld, username: string, password: string) {
    const usernameInput = await this.getLocator("usernameInput");
    const passwordInput = await this.getLocator("passwordInput");
    const loginButton = await this.getLocator("loginButton");

    await usernameInput.fill(username);
    await passwordInput.fill(password);
    await loginButton.click();

    await this.rawLocator("dashboardHeader").waitFor({ state: "visible", timeout: 10_000 });
  }
);

Given("I navigate to the settings page", async function (this: ICustomWorld) {
  const nav = await this.getLocator("navMenu");
  await nav.getByText("Settings").click();
  await this.rawLocator("settingsPage").waitFor({ state: "visible", timeout: 10_000 });
});

When(
  "I click on {string} in the navigation menu",
  async function (this: ICustomWorld, menuItem: string) {
    const nav = await this.getLocator("navMenu");
    await nav.getByText(menuItem).click();
  }
);

When("the application restarts", async function (this: ICustomWorld) {
  // Close and re-launch for restart simulation
  const { launchApp, closeApp } = await import("../src/electron/launcher");
  await closeApp(this.electronApp);
  const { electronApp, page } = await launchApp();
  this.electronApp = electronApp;
  this.page = page;
});
