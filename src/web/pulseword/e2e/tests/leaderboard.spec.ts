import { test, expect } from '@playwright/test';

test.describe('Leaderboard', () => {
  test.beforeEach(async ({ page }) => {
    await page.route('**/api/leaderboard', async route => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify([
          { userId: '1', username: 'FastPlayer', score: 1000, completionTimeSeconds: 30, applauseCount: 12 },
          { userId: '2', username: 'SteadyPlayer', score: 800, completionTimeSeconds: 45, applauseCount: 5 },
          { userId: 'me', username: 'CurrentUser', score: 750, completionTimeSeconds: 50, applauseCount: 2 }
        ])
      });
    });
    await page.goto('/leaderboard');
  });

  test('Leaderboard page displays entries', async ({ page }) => {
    const entries = page.locator('[data-testid="leaderboard-entry"]');
    await expect(entries).toHaveCount(3);
    await expect(entries.first()).toContainText('FastPlayer');
  });

  test('Entries are sorted by completion time', async ({ page }) => {
    const times = page.locator('[data-testid="entry-time"]');
    const time1 = await times.nth(0).textContent();
    const time2 = await times.nth(1).textContent();
    // Assuming format like "30s" or just "30"
    const t1 = parseInt(time1 || '0');
    const t2 = parseInt(time2 || '0');
    expect(t1).toBeLessThanOrEqual(t2);
  });

  test('Can applaud another player', async ({ page }) => {
    await page.route('**/api/leaderboard/1/applause', async route => {
      await route.fulfill({ status: 200 });
    });
    
    const firstEntry = page.locator('[data-testid="leaderboard-entry"]').first();
    const applaudButton = firstEntry.locator('[data-testid="applaud-button"]');
    await applaudButton.click();
    
    const applauseCount = firstEntry.locator('[data-testid="applause-count"]');
    await expect(applauseCount).toHaveText('13');
  });

  test('Applause count updates in real-time', async ({ page }) => {
    // Navigate to page first
    await page.goto('/leaderboard');
    
    // Simulate a real-time update (e.g., via a window message or direct SignalR mock if possible)
    await page.evaluate(() => {
        window.dispatchEvent(new CustomEvent('pulse-realtime-update', {
            detail: { type: 'applause', userId: '1', newCount: 20 }
        }));
    });
    
    const firstEntryApplause = page.locator('[data-testid="leaderboard-entry"]').first().locator('[data-testid="applause-count"]');
    await expect(firstEntryApplause).toHaveText('20');
  });

  test('Own entry is highlighted', async ({ page }) => {
    const myEntry = page.locator('[data-testid="leaderboard-entry"][data-is-me="true"]');
    await expect(myEntry).toBeVisible();
    await expect(myEntry).toHaveClass(/highlight-me/);
  });

  test('Can navigate to player profile from leaderboard', async ({ page }) => {
    await page.locator('text=FastPlayer').click();
    await expect(page).toHaveURL(/\/profile\/1/);
  });
});
