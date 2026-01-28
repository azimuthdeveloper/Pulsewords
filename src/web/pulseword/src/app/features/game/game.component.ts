import { Component, HostListener, inject, signal, computed, ElementRef, ViewChild, AfterViewInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { A11yModule } from '@angular/cdk/a11y';
import { KeyboardComponent } from './keyboard/keyboard.component';
import { GameService } from './game.service';
import { LiveAnnouncer } from '@angular/cdk/a11y';
import { AnnounceDirective } from '../../shared/directives/announce.directive';

@Component({
  selector: 'app-game',
  standalone: true,
  imports: [CommonModule, MatDialogModule, A11yModule, KeyboardComponent, AnnounceDirective],
  templateUrl: './game.component.html',
  styleUrls: ['./game.component.scss']
})
export class GameComponent implements AfterViewInit {
  private gameService = inject(GameService);
  private liveAnnouncer = inject(LiveAnnouncer);
  protected readonly window = window;
  
  @ViewChild('board') boardElement!: ElementRef;

  status = this.gameService.status;
  keyboardStates = this.gameService.keyboardStates;
  announcement = signal('');

  ngAfterViewInit() {
    this.boardElement.nativeElement.focus();
  }

  @HostListener('window:keydown', ['$event'])
  handleKeyDown(event: KeyboardEvent) {
    if (this.status().isGameOver) return;
    
    const key = event.key.toUpperCase();
    if (key === 'ENTER') {
      this.submitGuess();
    } else if (key === 'BACKSPACE') {
      this.gameService.removeLetter();
    } else if (/^[A-Z]$/.test(key)) {
      this.gameService.addLetter(key);
    }
  }

  handleVirtualKey(key: string) {
    if (key === 'ENTER') {
      this.submitGuess();
    } else if (key === 'BACKSPACE') {
      this.gameService.removeLetter();
    } else {
      this.gameService.addLetter(key);
    }
  }

  async submitGuess() {
    const previousRow = this.status().currentRow;
    this.gameService.submitGuess();
    
    // Announce feedback after submission
    if (this.status().currentRow > previousRow || this.status().isGameOver) {
      const rowData = this.status().board[previousRow];
      const feedback = rowData.map(t => `${t.letter}: ${t.state}`).join(', ');
      this.announcement.set(`Submitted guess. Results: ${feedback}`);
      
      if (this.status().isGameOver) {
        this.announcement.set(`Game over. ${this.status().message}`);
      }
    }
  }

  getTileClass(state: string): string {
    switch (state) {
      case 'correct': return 'feedback-correct';
      case 'wrong-position': return 'feedback-wrong-position';
      case 'incorrect': return 'feedback-incorrect';
      default: return '';
    }
  }

  getAriaLabel(tile: any, rowIndex: number, tileIndex: number): string {
    if (!tile.letter) return `Empty tile ${tileIndex + 1} in row ${rowIndex + 1}`;
    if (tile.state === 'empty' || tile.state === 'tbd') return `${tile.letter}`;
    return `${tile.letter} (${tile.state})`;
  }
}
