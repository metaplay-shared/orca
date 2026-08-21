// Smoke test: does a production build of the dashboard actually start?
//
// This exists because of a real outage: after the SDK 37.1.0 upgrade the dashboard hung
// forever on "Initializing UI...". Nothing in the existing checks could see it -- the
// build succeeded, vue-tsc passed, and `pnpm run dev` worked, because vite's dep
// optimizer dedupes Vue in dev while a real build had bundled two copies of it. Two Vue
// copies means two reactivity systems, so App.vue's `viewState` computed never
// invalidated and the loading screen stayed up.
//
// So this test has to run against a *built* dashboard served by the game server, not
// against the dev server:
//
//   uv run tools/orca.py dashboard test
//
// which builds the dashboard, serves it through the game server on :5550 and points
// DASHBOARD_BASE_URL here. See defaultConfig for the port convention.
import { test, expect } from '@metaplay/playwright-config'

// Please note to import from '@metaplay/playwright-config' instead of '@playwright/test'

/**
 * Generous: a production bundle has to download, the client authenticates and the
 * initialization steps fetch static config from the game server before the shell mounts.
 */
const bootTimeoutMs = 60_000

test('a built dashboard finishes initializing and renders the shell', async ({ page }) => {
  await page.goto('/')

  // The shell is only mounted once `viewState` becomes 'loaded', which requires every
  // initialization step to have completed. Asserting on it therefore covers the whole
  // init sequence rather than any single step.
  await expect(page.getByTestId('sidebar')).toBeVisible({ timeout: bootTimeoutMs })

  // And the landing route rendered, so routing works after initialization.
  await expect(page.getByTestId('overview-card')).toBeVisible({ timeout: bootTimeoutMs })

  // Explicit regression guard for the outage above: the loading screen shows the current
  // step's display name, and the final step is called "Initializing UI".
  await expect(page.getByText('Initializing UI')).toHaveCount(0)
})
