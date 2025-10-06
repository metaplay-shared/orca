// This file is part of Metaplay SDK which is released under the Metaplay SDK License.
import { test, expect } from '@metaplay/playwright-config'

// Please note to import from '@metaplay/playwright-config' instead of '@playwright/test'

test('trivial example', async ({ page, freshTestPlayer }) => {
  await page.goto(`/players/${freshTestPlayer}`)
  await expect(page.getByTestId('player-overview-card')).toBeVisible()
})
