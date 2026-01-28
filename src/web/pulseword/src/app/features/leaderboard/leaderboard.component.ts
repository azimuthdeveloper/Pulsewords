import { Component, OnInit, OnDestroy, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatCardModule } from '@angular/material/card';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatNativeDateModule } from '@angular/material/core';
import { FormsModule } from '@angular/forms';
import { LeaderboardService } from './leaderboard.service';
import { LeaderboardEntryDto } from '../../shared/models/api.models';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-leaderboard',
  standalone: true,
  imports: [
    CommonModule,
    MatTableModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatCardModule,
    MatDatepickerModule,
    MatFormFieldModule,
    MatInputModule,
    MatNativeDateModule,
    FormsModule
  ],
  templateUrl: './leaderboard.component.html',
  styleUrls: ['./leaderboard.component.scss']
})
export class LeaderboardComponent implements OnInit, OnDestroy {
  private leaderboardService = inject(LeaderboardService);
  
  leaderboardData = signal<LeaderboardEntryDto[]>([]);
  isLoading = signal(true);
  selectedDate = signal<Date>(new Date());
  
  displayedColumns: string[] = ['rank', 'player', 'time', 'guesses', 'score', 'applause'];
  
  private subscriptions = new Subscription();

  ngOnInit(): void {
    this.loadLeaderboard();
    
    this.subscriptions.add(
      this.leaderboardService.getApplauseUpdates().subscribe(update => {
        this.leaderboardData.update(data => 
          data.map(entry => 
            entry.playerId === update.playerId 
              ? { ...entry, applauseCount: update.count } 
              : entry
          )
        );
      })
    );
  }

  ngOnDestroy(): void {
    this.subscriptions.unsubscribe();
    this.leaderboardService.stopListening(this.formatDate(this.selectedDate()));
  }

  loadLeaderboard(): void {
    const dateStr = this.formatDate(this.selectedDate());
    this.isLoading.set(true);
    
    this.leaderboardService.getLeaderboard(dateStr).subscribe({
      next: (data) => {
        this.leaderboardData.set(data);
        this.isLoading.set(false);
        this.leaderboardService.listenToUpdates(dateStr).subscribe(updatedData => {
           this.leaderboardData.set(updatedData);
        });
      },
      error: (err) => {
        console.error('Error loading leaderboard', err);
        this.isLoading.set(false);
      }
    });
  }

  onDateChange(event: any): void {
    const oldDateStr = this.formatDate(this.selectedDate());
    this.leaderboardService.stopListening(oldDateStr);
    
    this.selectedDate.set(event.value);
    this.loadLeaderboard();
  }

  sendApplause(entry: LeaderboardEntryDto): void {
    const dateStr = this.formatDate(this.selectedDate());
    this.leaderboardService.sendApplause(dateStr, entry.playerId);
  }

  private formatDate(date: Date): string {
    return date.toISOString().split('T')[0];
  }

  getRankClass(rank: number): string {
    if (rank === 1) return 'rank-gold';
    if (rank === 2) return 'rank-silver';
    if (rank === 3) return 'rank-bronze';
    return '';
  }
}
