# Prompt: Add a new BDD feature

Use this prompt to scaffold a new feature end-to-end.

---

Add a new BDD feature called `<FEATURE_NAME>` to the widget-automation project.

1. Create `features/<FEATURE_NAME>.feature` with 3 realistic Gherkin scenarios tagged `@<FEATURE_NAME>`. Use the Background pattern if multiple scenarios share setup.
2. Create `steps/<FEATURE_NAME>.steps.ts` with all step definitions. Use `this.getLocator(name)` for all element access — never hardcode selectors.
3. Add any new locators to `src/locators/locators.json` using the `{ css, text, role }` shape.
4. Do NOT modify `hooks.ts` or `world.ts` unless the feature requires a new lifecycle action.

Follow the existing conventions in `steps/login.steps.ts` and `features/login.feature`.
