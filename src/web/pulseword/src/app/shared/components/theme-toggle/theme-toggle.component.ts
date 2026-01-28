import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { ThemeService, Theme } from '../../../core/theme/theme.service';

@Component({
  selector: 'app-theme-toggle',
  standalone: true,
  imports: [CommonModule, MatButtonModule, MatIconModule, MatTooltipModule],
  template: `
    <button mat-icon-button (click)="toggleTheme()" [matTooltip]="getTooltip()" aria-label="Toggle theme">
      <mat-icon>{{ getIcon() }}</mat-icon>
    </button>
  `,
  styles: [`
    :host {
      display: block;
    }
  `]
})
export class ThemeToggleComponent {
  constructor(private themeService: ThemeService) {}

  toggleTheme() {
    this.themeService.toggleTheme();
  }

  getIcon(): string {
    const theme = this.themeService.getTheme();
    switch (theme) {
      case 'light': return 'light_mode';
      case 'dark': return 'dark_mode';
      case 'high-contrast': return 'contrast';
      default: return 'light_mode';
    }
  }

  getTooltip(): string {
    const theme = this.themeService.getTheme();
    switch (theme) {
      case 'light': return 'Switch to Dark Mode';
      case 'dark': return 'Switch to High Contrast Mode';
      case 'high-contrast': return 'Switch to Light Mode';
      default: return 'Toggle Theme';
    }
  }
}
