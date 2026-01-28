import { test, expect } from '@playwright/test';

test.describe('Real-time Updates', () => {
  test('SignalR connection established on page load', async ({ page }) => {
    // Monitor websocket connections
    const wsPromise = page.waitForEvent('websocket');
    await page.goto('/');
    const ws = await wsPromise;
    expect(ws.url()).toContain('/hubs/pulse');
  });

  test('Leaderboard updates when another player completes', async ({ page }) => {
    await page.goto('/leaderboard');
    
    // Trigger a mock real-time event
    await page.evaluate(() => {
        // Assuming there's a global hook for testing or we trigger via event
        window.dispatchEvent(new CustomEvent('pulse-realtime-update', {
            detail: { 
                type: 'PlayerCompleted', 
                data: { userId: '99', username: 'Speedy', completionTimeSeconds: 20 } 
            }
        }));
    });
    
    await expect(page.locator('text=Speedy')).toBeVisible();
    await expect(page.locator('text=20s')).toBeVisible();
  });

  test('Applause notifications appear in real-time', async ({ page }) => {
    await page.goto('/');
    
    await page.evaluate(() => {
        window.dispatchEvent(new CustomEvent('pulse-realtime-update', {
            detail: { 
                type: 'ApplauseReceived', 
                data: { fromUsername: 'Fan1337' } 
            }
        }));
    });
    
    const notification = page.locator('[data-testid="applause-notification"]');
    await expect(notification).toBeVisible();
    await expect(notification).toContainText('Fan1337 applauded you!');
  });
});
