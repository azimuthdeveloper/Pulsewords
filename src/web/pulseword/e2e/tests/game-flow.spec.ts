import { test, expect } from '@playwright/test';

test.describe('Game Flow', () => {
  test.beforeEach(async ({ page }) => {
    // Mock the daily game API
    await page.route('**/api/daily-games/current', async route => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          id: '123',
          wordLength: 5,
          maxGuesses: 6,
          expirationTime: new Date(Date.now() + 600000).toISOString()
        })
      });
    });
    await page.goto('/');
  });

  test('Page loads with empty game board (6 rows, 5 columns)', async ({ page }) => {
    const rows = page.locator('[data-testid="game-row"]');
    await expect(rows).toHaveCount(6);
    const tiles = page.locator('[data-testid="tile"]');
    await expect(tiles).toHaveCount(30);
  });

  test('Typing letters fills tiles from left to right', async ({ page }) => {
    await page.keyboard.type('HELLO');
    const firstRowTiles = page.locator('[data-testid="game-row"]').first().locator('[data-testid="tile"]');
    await expect(firstRowTiles.nth(0)).toHaveText('H');
    await expect(firstRowTiles.nth(1)).toHaveText('E');
    await expect(firstRowTiles.nth(2)).toHaveText('L');
    await expect(firstRowTiles.nth(3)).toHaveText('L');
    await expect(firstRowTiles.nth(4)).toHaveText('O');
  });

  test('Backspace removes last letter', async ({ page }) => {
    await page.keyboard.type('HEL');
    await page.keyboard.press('Backspace');
    const firstRowTiles = page.locator('[data-testid="game-row"]').first().locator('[data-testid="tile"]');
    await expect(firstRowTiles.nth(0)).toHaveText('H');
    await expect(firstRowTiles.nth(1)).toHaveText('E');
    await expect(firstRowTiles.nth(2)).toBeEmpty();
  });

  test('Cannot type more than 5 letters per row', async ({ page }) => {
    await page.keyboard.type('HELLOOO');
    const firstRowTiles = page.locator('[data-testid="game-row"]').first().locator('[data-testid="tile"]');
    await expect(firstRowTiles).toHaveCount(5);
    await expect(firstRowTiles.nth(4)).toHaveText('O');
  });

  test('Enter key submits guess when row is complete', async ({ page }) => {
    await page.route('**/api/games/*/guess', async route => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          isCorrect: false,
          letterFeedbacks: [
            { letter: 'H', feedback: 2 }, // Correct
            { letter: 'E', feedback: 2 },
            { letter: 'L', feedback: 2 },
            { letter: 'L', feedback: 2 },
            { letter: 'O', feedback: 0 }  // Incorrect
          ]
        })
      });
    });

    await page.keyboard.type('HELLO');
    await page.keyboard.press('Enter');
    
    // Expect second row to become active
    const secondRow = page.locator('[data-testid="game-row"]').nth(1);
    await expect(secondRow).toHaveAttribute('data-active', 'true');
  });

  test('Cannot submit incomplete guess (less than 5 letters)', async ({ page }) => {
    await page.keyboard.type('HELL');
    await page.keyboard.press('Enter');
    
    const firstRow = page.locator('[data-testid="game-row"]').first();
    await expect(firstRow).toHaveAttribute('data-active', 'true');
    await expect(page.locator('text=Not enough letters')).toBeVisible();
  });

  test('Correct letter in correct position shows green', async ({ page }) => {
    await page.route('**/api/games/*/guess', async route => {
      await route.fulfill({
        body: JSON.stringify({
          letterFeedbacks: [
            { letter: 'H', feedback: 2 },
            { letter: 'A', feedback: 0 },
            { letter: 'B', feedback: 0 },
            { letter: 'C', feedback: 0 },
            { letter: 'D', feedback: 0 }
          ]
        })
      });
    });
    await page.keyboard.type('HABCD');
    await page.keyboard.press('Enter');
    const tile = page.locator('[data-testid="tile"]').first();
    await expect(tile).toHaveClass(/feedback-correct/);
  });

  test('Correct letter in wrong position shows yellow', async ({ page }) => {
    await page.route('**/api/games/*/guess', async route => {
      await route.fulfill({
        body: JSON.stringify({
          letterFeedbacks: [
            { letter: 'H', feedback: 1 },
            { letter: 'A', feedback: 0 },
            { letter: 'B', feedback: 0 },
            { letter: 'C', feedback: 0 },
            { letter: 'D', feedback: 0 }
          ]
        })
      });
    });
    await page.keyboard.type('HABCD');
    await page.keyboard.press('Enter');
    const tile = page.locator('[data-testid="tile"]').first();
    await expect(tile).toHaveClass(/feedback-wrong-position/);
  });

  test('Incorrect letter shows gray', async ({ page }) => {
    await page.route('**/api/games/*/guess', async route => {
      await route.fulfill({
        body: JSON.stringify({
          letterFeedbacks: [
            { letter: 'H', feedback: 0 },
            { letter: 'A', feedback: 0 },
            { letter: 'B', feedback: 0 },
            { letter: 'C', feedback: 0 },
            { letter: 'D', feedback: 0 }
          ]
        })
      });
    });
    await page.keyboard.type('HABCD');
    await page.keyboard.press('Enter');
    const tile = page.locator('[data-testid="tile"]').first();
    await expect(tile).toHaveClass(/feedback-incorrect/);
  });

  test('Keyboard updates to reflect guessed letters', async ({ page }) => {
    await page.route('**/api/games/*/guess', async route => {
      await route.fulfill({
        body: JSON.stringify({
          letterFeedbacks: [{ letter: 'H', feedback: 2 }]
        })
      });
    });
    await page.keyboard.type('HXXXX');
    await page.keyboard.press('Enter');
    const key = page.locator('[data-testid="key-H"]');
    await expect(key).toHaveClass(/key-correct/);
  });

  test('Winning game shows success message', async ({ page }) => {
    await page.route('**/api/games/*/guess', async route => {
      await route.fulfill({
        body: JSON.stringify({ isCorrect: true, letterFeedbacks: [] })
      });
    });
    await page.keyboard.type('HELLO');
    await page.keyboard.press('Enter');
    await expect(page.locator('text=Impressive!')).toBeVisible();
  });

  test('Losing after 6 guesses shows failure message', async ({ page }) => {
    await page.route('**/api/games/*/guess', async route => {
      await route.fulfill({
        body: JSON.stringify({ isCorrect: false, letterFeedbacks: [] })
      });
    });
    for (let i = 0; i < 6; i++) {
        await page.keyboard.type('WRONG');
        await page.keyboard.press('Enter');
    }
    await expect(page.locator('text=Game Over')).toBeVisible();
  });

  test('Timer counts down during gameplay', async ({ page }) => {
    const timer = page.locator('[data-testid="timer"]');
    const initialTimeText = await timer.textContent();
    // Wait for a second
    await page.waitForTimeout(1100);
    const newTimeText = await timer.textContent();
    expect(newTimeText).not.toBe(initialTimeText);
  });

  test('Game ends when timer expires', async ({ page }) => {
    // Mock the game to have a very short expiration
    await page.route('**/api/daily-games/current', async route => {
        await route.fulfill({
          body: JSON.stringify({
            id: '123',
            wordLength: 5,
            maxGuesses: 6,
            expirationTime: new Date(Date.now() + 2000).toISOString()
          })
        });
      });
    await page.goto('/');
    await page.waitForTimeout(2500);
    await expect(page.locator('text=Time is up!')).toBeVisible();
  });
});
