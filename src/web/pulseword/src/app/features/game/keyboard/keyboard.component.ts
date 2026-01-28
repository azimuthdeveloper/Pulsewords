import { Component, EventEmitter, Output, input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LetterState } from '../game.service';

@Component({
  selector: 'app-keyboard',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './keyboard.component.html',
  styleUrls: ['./keyboard.component.scss']
})
export class KeyboardComponent {
  @Output() keyPress = new EventEmitter<string>();
  
  letterStates = input<Record<string, LetterState>>({});

  rows = [
    ['Q', 'W', 'E', 'R', 'T', 'Y', 'U', 'I', 'O', 'P'],
    ['A', 'S', 'D', 'F', 'G', 'H', 'J', 'K', 'L'],
    ['ENTER', 'Z', 'X', 'C', 'V', 'B', 'N', 'M', 'BACKSPACE']
  ];

  onKeyClick(key: string) {
    this.keyPress.emit(key);
  }

  getKeyStateClass(key: string): string {
    const state = this.letterStates()[key];
    if (!state || state === 'unused') return '';
    return `key-${state}`;
  }

  getAriaLabel(key: string): string {
    if (key === 'ENTER') return 'Enter';
    if (key === 'BACKSPACE') return 'Backspace';
    const state = this.letterStates()[key];
    if (state && state !== 'unused') {
      return `${key}, ${state}`;
    }
    return key;
  }

  getAriaPressed(key: string): boolean | null {
    const state = this.letterStates()[key];
    return state && state !== 'unused' ? true : false;
  }
}
