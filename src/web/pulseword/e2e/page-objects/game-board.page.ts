import { Page, Locator } from '@playwright/test';

export class GameBoardPage {
  private readonly page: Page;
  private readonly keyboardKey: (key: string) => Locator;
  private readonly tile: (row: number, col: number) => Locator;
  private readonly timer: Locator;
  private readonly guessCount: Locator;

  constructor(page: Page) {
    this.page = page;
    this.keyboardKey = (key: string) => page.locator(`button.key:has-text("${key}")`);
    this.tile = (row: number, col: number) => page.locator(`.row`).nth(row).locator(`.tile`).nth(col);
    this.timer = page.locator('.timer');
    this.guessCount = page.locator('.guess-count');
  }

  async enterLetter(letter: string) {
    await this.keyboardKey(letter.toUpperCase()).click();
  }

  async deleteLetter() {
    await this.page.keyboard.press('Backspace');
  }

  async submitGuess() {
    await this.page.keyboard.press('Enter');
  }

  async getTileColor(row: number, col: number): Promise<string> {
    const tile = this.tile(row, col);
    return await tile.evaluate((el) => getComputedStyle(el).backgroundColor);
  }

  async getTileText(row: number, col: number): Promise<string> {
    return (await this.tile(row, col).innerText()).trim();
  }

  async getKeyboardKeyColor(key: string): Promise<string> {
    const k = this.keyboardKey(key.toUpperCase());
    return await k.evaluate((el) => getComputedStyle(el).backgroundColor);
  }

  async getTimer(): Promise<string> {
    return (await this.timer.innerText()).trim();
  }

  async getGuessCount(): Promise<string> {
    return (await this.guessCount.innerText()).trim();
  }
}
