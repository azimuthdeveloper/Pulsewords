import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { LeaderboardEntryDto } from '../../shared/models/api.models';
import { SignalRService } from '../../shared/services/signalr.service';
import { API_BASE_URL } from '../../app.config';

@Injectable({
  providedIn: 'root'
})
export class LeaderboardService {
  private http = inject(HttpClient);
  private signalRService = inject(SignalRService);
  private apiBaseUrl = inject(API_BASE_URL);
  private apiUrl = `${this.apiBaseUrl}/leaderboard`;

  getLeaderboard(date: string): Observable<LeaderboardEntryDto[]> {
    return this.http.get<LeaderboardEntryDto[]>(`${this.apiUrl}/${date}`);
  }

  listenToUpdates(date: string): Observable<LeaderboardEntryDto[]> {
    this.signalRService.joinGroup(`leaderboard-${date}`);
    return this.signalRService.leaderboardUpdate$;
  }

  stopListening(date: string): void {
    this.signalRService.leaveGroup(`leaderboard-${date}`);
  }

  sendApplause(date: string, playerId: string): void {
    this.signalRService.sendApplause(date, playerId);
  }

  getApplauseUpdates(): Observable<{ playerId: string, count: number }> {
    return this.signalRService.applauseReceived$;
  }
}
