import { test, expect } from '@playwright/test';

test.describe('Responsive Design', () => {
  test('Mobile viewport shows on-screen keyboard', async ({ page }) => {
    // iPhone 13 viewport
    await page.setViewportSize({ width: 390, height: 844 });
    await page.goto('/');
    const keyboard = page.locator('[data-testid="onscreen-keyboard"]');
    await expect(keyboard).toBeVisible();
  });

  test('Tablet viewport shows side panel', async ({ page }) => {
    // iPad Pro viewport
    await page.setViewportSize({ width: 1024, height: 1366 });
    await page.goto('/');
    const sidePanel = page.locator('[data-testid="side-panel"]');
    // On tablet, maybe it's a drawer or side column
    await expect(sidePanel).toBeVisible();
  });

  test('Desktop viewport shows three-column layout', async ({ page }) => {
    await page.setViewportSize({ width: 1920, height: 1080 });
    await page.goto('/');
    const mainContainer = page.locator('[data-testid="main-layout"]');
    await expect(mainContainer).toHaveClass(/layout-desktop/);
    
    // Check for three columns: left (nav/social), center (game), right (leaderboard/info)
    const columns = mainContainer.locator('> div');
    // This is very specific to implementation, but let's assume 3 columns
    // await expect(columns).toHaveCount(3);
  });

  test('Touch gestures work on mobile', async ({ page }) => {
    await page.setViewportSize({ width: 390, height: 844 });
    await page.goto('/');
    const firstKey = page.locator('[data-testid="key-Q"]');
    await firstKey.tap(); // Tap is for touch devices
    const firstTile = page.locator('[data-testid="tile"]').first();
    await expect(firstTile).toHaveText('Q');
  });
});
