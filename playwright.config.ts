import { defineConfig } from "@playwright/test";
import * as dotenv from "dotenv";

dotenv.config();

export default defineConfig({
  testDir: "./features",
  timeout: 60_000,
  retries: 1,
  use: {
    headless: false,
    screenshot: "only-on-failure",
    video: "retain-on-failure",
    trace: "on-first-retry",
  },
  outputDir: "reports/screenshots",
  reporter: [["list"], ["html", { outputFolder: "reports/allure-report" }]],
});
