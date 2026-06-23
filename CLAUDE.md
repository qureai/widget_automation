# Electron BDD Framework — Claude Code Instructions

## Project Overview
BDD automation framework for Electron desktop apps using Playwright + Cucumber + self-healing locators.

## Key Architecture Decisions
- **Locators live in `src/locators/locators.json`** — never hardcode selectors in step definitions.
- **All locator access goes through `src/locators/registry.ts`** — it handles fallback + healing.
- **App lifecycle (install + launch) is in `src/electron/launcher.ts`** — called only from `features/support/hooks.ts`.
- **Heal events are logged via `src/heal/healReport.ts`** — appends to `reports/heal-report.json`.

## Running Tests
```bash
npm test                          # all features
npm run test:login                # @login tagged scenarios only
npm run test:dashboard
npm run test:settings
npm run report:gen && npm run report:open   # generate + view Allure report
```

## Adding a New Feature
1. Create `features/<name>.feature` with Gherkin scenarios tagged `@<name>`.
2. Create `steps/<name>.steps.ts` importing `{ ICustomWorld }` from `features/support/world.ts`.
3. Add any new selectors to `src/locators/locators.json`.

## Adding Locators
Add an entry to `src/locators/locators.json`:
```json
"loginButton": { "css": "#btn-login", "text": "Login", "role": "button" }
```
`registry.ts` will try `css` → `text` → `role` in that order.

## Environment Variables (`.env`)
| Variable       | Purpose                          |
|----------------|----------------------------------|
| `APP_EXE_PATH` | Absolute path to installed `.exe`|
| `APP_MSI_PATH` | Relative path to `.msi` installer|

## Conventions
- Step files import world via `ICustomWorld`, not raw `Page`.
- Use `await world.getLocator("name")` to resolve any selector.
- Screenshots on failure are automatic via hooks — do not add manual screenshot calls.
- Keep `locators.json` keys camelCase and descriptive: `loginButton`, `usernameInput`.
