import { Then, When } from "@cucumber/cucumber";
import { expect } from "@playwright/test";
import { ICustomWorld } from "../features/support/world";

Then(
  "the dashboard header should be visible",
  async function (this: ICustomWorld) {
    const header = await this.getLocator("dashboardHeader");
    await expect(header).toBeVisible();
  }
);

Then(
  "the metrics panel should be visible",
  async function (this: ICustomWorld) {
    const panel = await this.getLocator("metricsPanel");
    await expect(panel).toBeVisible();
  }
);

Then("the activity feed should be visible", async function (this: ICustomWorld) {
  const feed = await this.getLocator("activityFeed");
  await expect(feed).toBeVisible();
});

When("I click the refresh button", async function (this: ICustomWorld) {
  const btn = await this.getLocator("refreshButton");
  await btn.click();
});

Then(
  "the metrics panel should display updated data",
  async function (this: ICustomWorld) {
    const panel = await this.getLocator("metricsPanel");
    await expect(panel).toBeVisible();
    await this.rawLocator("loadingSpinner").waitFor({ state: "hidden", timeout: 10_000 }).catch(() => {});
  }
);

Then(
  "I should be on the reports page",
  async function (this: ICustomWorld) {
    await this.rawLocator("reportsContainer").waitFor({ state: "visible", timeout: 10_000 });
  }
);
