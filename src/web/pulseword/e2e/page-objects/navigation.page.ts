import { Page, Locator } from '@playwright/test';

export class NavigationPage {
  private readonly page: Page;
  private readonly gameLink: Locator;
  private readonly leaderboardLink: Locator;
  private readonly profileLink: Locator;
  private readonly loginButton: Locator;
  private readonly logoutButton: Locator;

  constructor(page: Page) {
    this.page = page;
    this.gameLink = page.locator('nav a[href="/game"]');
    this.leaderboardLink = page.locator('nav a[href="/leaderboard"]');
    this.profileLink = page.locator('nav a[href="/profile"]');
    this.loginButton = page.locator('button.login');
    this.logoutButton = page.locator('button.logout');
  }

  async goToGame() {
    await this.gameLink.click();
  }

  async goToLeaderboard() {
    await this.leaderboardLink.click();
  }

  async goToProfile() {
    await this.profileLink.click();
  }

  async login(username: string, password: string) {
    await this.loginButton.click();
    await this.page.fill('input[name="username"]', username);
    await this.page.fill('input[name="password"]', password);
    await this.page.click('button[type="submit"]');
  }

  async logout() {
    await this.logoutButton.click();
  }
}
