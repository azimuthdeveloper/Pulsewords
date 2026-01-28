import { Injectable, signal, computed, effect } from '@angular/core';
import { HttpClient } from '@angular/common/http';

export type LetterState = 'correct' | 'wrong-position' | 'incorrect' | 'unused';

export interface GameStatus {
  board: { letter: string; state: LetterState }[][];
  currentRow: number;
  isGameOver: boolean;
  won: boolean;
  message?: string;
  timeLeft: number;
}

@Injectable({
  providedIn: 'root'
})
export class GameService {
  // Signals for state management
  status = signal<GameStatus>({
    board: Array(6).fill(null).map(() => Array(5).fill({ letter: '', state: 'unused' })),
    currentRow: 0,
    isGameOver: false,
    won: false,
    timeLeft: 60
  });

  currentGuess = signal<string>('');
  
  // Track keyboard states
  keyboardStates = signal<Record<string, LetterState>>({});

  constructor(private http: HttpClient) {
    // Timer logic
    const timer = setInterval(() => {
      const current = this.status();
      if (current.isGameOver) {
        clearInterval(timer);
        return;
      }
      if (current.timeLeft > 0) {
        this.status.update(s => ({ ...s, timeLeft: s.timeLeft - 1 }));
      } else {
        this.status.update(s => ({ ...s, isGameOver: true, won: false, message: 'Time up!' }));
        clearInterval(timer);
      }
    }, 1000);
  }

  addLetter(letter: string) {
    if (this.status().isGameOver) return;
    if (this.currentGuess().length < 5) {
      this.currentGuess.update(g => g + letter.toUpperCase());
      this.updateBoardWithCurrentGuess();
    }
  }

  removeLetter() {
    if (this.status().isGameOver) return;
    if (this.currentGuess().length > 0) {
      this.currentGuess.update(g => g.slice(0, -1));
      this.updateBoardWithCurrentGuess();
    }
  }

  submitGuess() {
    if (this.status().isGameOver) return;
    if (this.currentGuess().length !== 5) return;

    // TODO: Connect to backend API. For now, mocking logic.
    const guess = this.currentGuess();
    const target = 'PULSE'; // Mock target word
    
    const result: { letter: string; state: LetterState }[] = [];
    const newKeyboardStates = { ...this.keyboardStates() };

    for (let i = 0; i < 5; i++) {
      let state: LetterState;
      if (guess[i] === target[i]) {
        state = 'correct';
      } else if (target.includes(guess[i])) {
        state = 'wrong-position';
      } else {
        state = 'incorrect';
      }
      result.push({ letter: guess[i], state });
      
      // Update keyboard state (priority: correct > wrong-position > incorrect)
      const currentKState = newKeyboardStates[guess[i]];
      if (state === 'correct' || (state === 'wrong-position' && currentKState !== 'correct') || (!currentKState)) {
        newKeyboardStates[guess[i]] = state;
      }
    }

    this.keyboardStates.set(newKeyboardStates);

    this.status.update(s => {
      const newBoard = [...s.board];
      newBoard[s.currentRow] = result;
      const won = guess === target;
      const isGameOver = won || s.currentRow === 5;
      return {
        ...s,
        board: newBoard,
        currentRow: s.currentRow + 1,
        won,
        isGameOver,
        message: won ? 'You Won!' : (isGameOver ? 'Game Over' : undefined)
      };
    });

    this.currentGuess.set('');
  }

  private updateBoardWithCurrentGuess() {
    this.status.update(s => {
      const newBoard = [...s.board];
      const guess = this.currentGuess();
      const newRow = Array(5).fill(null).map((_, i) => ({
        letter: guess[i] || '',
        state: 'unused' as LetterState
      }));
      newBoard[s.currentRow] = newRow;
      return { ...s, board: newBoard };
    });
  }
}
