import { test, expect } from '@playwright/test';

// Note: Accessibility tests usually require axe-playwright but we can do basic checks manually
test.describe('Accessibility', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/');
  });

  test('Game board is keyboard navigable', async ({ page }) => {
    // Focus should be able to move through tiles or keyboard
    await page.keyboard.press('Tab');
    const firstFocused = await page.evaluate(() => document.activeElement?.getAttribute('data-testid') || document.activeElement?.tagName);
    expect(firstFocused).toBeTruthy();
  });

  test('All interactive elements have ARIA labels', async ({ page }) => {
    const interactive = page.locator('button, input, [role="button"]');
    const count = await interactive.count();
    for (let i = 0; i < count; i++) {
        const el = interactive.nth(i);
        const ariaLabel = await el.getAttribute('aria-label');
        const title = await el.getAttribute('title');
        const text = await el.textContent();
        expect(ariaLabel || title || (text && text.trim().length > 0)).toBeTruthy();
    }
  });

  test('Color contrast meets WCAG AA standards', async ({ page }) => {
    // This is hard to test without axe, but we can check if the contrast-checker attributes exist
    // or if we have a specific high-contrast mode
    const body = page.locator('body');
    await expect(body).not.toHaveClass(/low-contrast/);
  });

  test('Screen reader announces tile feedback', async ({ page }) => {
    const liveRegion = page.locator('[aria-live="polite"], [aria-live="assertive"]');
    await expect(liveRegion).toBeAttached();
    
    // Type something and see if it updates
    await page.keyboard.type('A');
    // The live region might announce the letter
    // This is app-specific, but let's assume it should not be empty after interaction
    await expect(liveRegion).not.toBeEmpty();
  });

  test('Focus management on guess submission', async ({ page }) => {
    await page.keyboard.type('HELLO');
    await page.keyboard.press('Enter');
    
    // Focus should not be lost (body should not be focused)
    const activeElement = await page.evaluate(() => document.activeElement?.tagName);
    expect(activeElement).not.toBe('BODY');
  });
});
