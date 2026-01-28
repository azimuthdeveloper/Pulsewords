import { Component, HostListener, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { KeyboardComponent } from './keyboard/keyboard.component';
import { GameService } from './game.service';

@Component({
  selector: 'app-game',
  standalone: true,
  imports: [CommonModule, MatDialogModule, KeyboardComponent],
  templateUrl: './game.component.html',
  styleUrls: ['./game.component.scss']
})
export class GameComponent {
  private gameService = inject(GameService);
  protected readonly window = window;
  
  status = this.gameService.status;
  keyboardStates = this.gameService.keyboardStates;

  @HostListener('window:keydown', ['$event'])
  handleKeyDown(event: KeyboardEvent) {
    if (this.status().isGameOver) return;
    
    const key = event.key.toUpperCase();
    if (key === 'ENTER') {
      this.gameService.submitGuess();
    } else if (key === 'BACKSPACE') {
      this.gameService.removeLetter();
    } else if (/^[A-Z]$/.test(key)) {
      this.gameService.addLetter(key);
    }
  }

  handleVirtualKey(key: string) {
    if (key === 'ENTER') {
      this.gameService.submitGuess();
    } else if (key === 'BACKSPACE') {
      this.gameService.removeLetter();
    } else {
      this.gameService.addLetter(key);
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
}
