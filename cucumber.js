module.exports = {
  default: {
    require: [
      "features/support/world.ts",
      "features/support/hooks.ts",
      "steps/**/*.steps.ts",
    ],
    format: [
      "progress-bar",
      "json:reports/cucumber-report.json",
    ],
    retry: 1,
  },
};
