import { test, expect } from '@playwright/test';

test.describe('Social Features', () => {
  test.beforeEach(async ({ page }) => {
    // Basic setup, maybe auth mock
  });

  test('Can follow another user', async ({ page }) => {
    await page.route('**/api/users/2/follow', async route => {
        await route.fulfill({ status: 200 });
    });
    await page.goto('/profile/2');
    const followBtn = page.getByRole('button', { name: 'Follow' });
    await followBtn.click();
    await expect(page.getByRole('button', { name: 'Unfollow' })).toBeVisible();
  });

  test('Can unfollow a followed user', async ({ page }) => {
    await page.route('**/api/users/2/unfollow', async route => {
        await route.fulfill({ status: 200 });
    });
    // Mock user profile as already followed
    await page.route('**/api/users/2', async route => {
        await route.fulfill({
            body: JSON.stringify({ id: '2', username: 'OtherUser', isFollowing: true })
        });
    });
    await page.goto('/profile/2');
    const unfollowBtn = page.getByRole('button', { name: 'Unfollow' });
    await unfollowBtn.click();
    await expect(page.getByRole('button', { name: 'Follow' })).toBeVisible();
  });

  test('Following list shows on profile', async ({ page }) => {
    await page.route('**/api/users/me/following', async route => {
        await route.fulfill({
            body: JSON.stringify([
                { id: '2', username: 'Friend1' },
                { id: '3', username: 'Friend2' }
            ])
        });
    });
    await page.goto('/profile/me');
    await page.getByRole('tab', { name: 'Following' }).click();
    const items = page.locator('[data-testid="following-item"]');
    await expect(items).toHaveCount(2);
    await expect(items.first()).toContainText('Friend1');
  });

  test('Followers list shows on profile', async ({ page }) => {
    await page.route('**/api/users/me/followers', async route => {
        await route.fulfill({
            body: JSON.stringify([
                { id: '4', username: 'Fan1' }
            ])
        });
    });
    await page.goto('/profile/me');
    await page.getByRole('tab', { name: 'Followers' }).click();
    const items = page.locator('[data-testid="follower-item"]');
    await expect(items).toHaveCount(1);
    await expect(items).toContainText('Fan1');
  });

  test('Activity feed shows friends completions', async ({ page }) => {
    await page.route('**/api/social/activity', async route => {
        await route.fulfill({
            body: JSON.stringify([
                { id: '101', userId: '2', username: 'Friend1', action: 'completed', gameName: 'Daily Word', timestamp: new Date().toISOString() }
            ])
        });
    });
    await page.goto('/');
    const feed = page.locator('[data-testid="activity-feed"]');
    await expect(feed).toBeVisible();
    await expect(feed.locator('[data-testid="activity-item"]')).toContainText('Friend1 completed Daily Word');
  });
});
