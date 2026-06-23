import { Before, After, BeforeAll, AfterAll, Status, setDefaultTimeout } from "@cucumber/cucumber";

setDefaultTimeout(120_000);
import { ICustomWorld } from "./world";
import { ensureInstalled, launchApp, closeApp, launchWidgetFromSource, closeWidgetCDP, logoutPowerScribe, terminatePowerScribe } from "../../src/electron/launcher";

BeforeAll(async function () {
  // Install the MSI once per test run if the exe is not already present.
  // This prompts UAC elevation — approve it when the dialog appears.
  await ensureInstalled();
});

Before({ tags: "not @login and not @feedback" }, async function (this: ICustomWorld) {
  const { electronApp, page } = await launchApp();
  this.electronApp = electronApp;
  this.page = page;
});

Before({ tags: "@login or @feedback" }, async function (this: ICustomWorld) {
  const { browser, page, proc } = await launchWidgetFromSource();
  this.cdpBrowser = browser;
  this.page = page;
  this.widgetProcess = proc;
});

After(async function (this: ICustomWorld, scenario) {
  if (scenario.result?.status === Status.FAILED) {
    try {
      await this.takeScreenshot(`FAILED-${scenario.pickle.name.replace(/[^a-z0-9]/gi, "_")}`);
    } catch {
      // Widget window may already be closed — screenshot not critical, skip silently
    }
  }

  if (scenario.willBeRetried) {
    // Full teardown before retry — terminate so the next attempt starts completely fresh.
    terminatePowerScribe();
  } else {
    logoutPowerScribe();
  }

  await closeApp(this.electronApp);
  await closeWidgetCDP(this.cdpBrowser, this.widgetProcess);
  this.electronApp = null;
  this.cdpBrowser = null;
  this.page = null;
  this.widgetProcess = null;
});

AfterAll(async function () {
  terminatePowerScribe();
});
