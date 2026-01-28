import { Page, Locator } from '@playwright/test';

export class LeaderboardPage {
  private readonly page: Page;
  private readonly entries: Locator;

  constructor(page: Page) {
    this.page = page;
    this.entries = page.locator('.leaderboard-entry');
  }

  async getEntries() {
    return await this.entries.all();
  }

  async getRank(playerName: string): Promise<number | null> {
    const entry = this.page.locator(`.leaderboard-entry:has-text("${playerName}")`);
    if (await entry.isVisible()) {
      const rankText = await entry.locator('.rank').innerText();
      return parseInt(rankText.replace('#', ''), 10);
    }
    return null;
  }

  async applaudPlayer(playerName: string) {
    const entry = this.page.locator(`.leaderboard-entry:has-text("${playerName}")`);
    await entry.locator('button.applaud').click();
  }
}
