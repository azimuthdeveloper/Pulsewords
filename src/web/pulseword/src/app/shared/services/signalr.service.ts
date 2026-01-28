import { Injectable, signal } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { Subject } from 'rxjs';
import { LeaderboardEntryDto } from '../models/api.models';

@Injectable({
  providedIn: 'root'
})
export class SignalRService {
  private hubConnection: signalR.HubConnection | null = null;
  
  public leaderboardUpdate$ = new Subject<LeaderboardEntryDto[]>();
  public gameCompleted$ = new Subject<LeaderboardEntryDto>();
  public applauseReceived$ = new Subject<{ playerId: string, count: number }>();
  
  public isConnected = signal(false);

  constructor() {
    this.startConnection('/pulseHub');
  }

  private startConnection(hubUrl: string): void {
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(hubUrl)
      .withAutomaticReconnect()
      .build();

    this.hubConnection
      .start()
      .then(() => {
        console.log('Connection started');
        this.isConnected.set(true);
        this.registerHandlers();
      })
      .catch(err => console.log('Error while starting connection: ' + err));

    this.hubConnection.onclose(() => this.isConnected.set(false));
    this.hubConnection.onreconnecting(() => this.isConnected.set(false));
    this.hubConnection.onreconnected(() => this.isConnected.set(true));
  }

  private registerHandlers(): void {
    if (!this.hubConnection) return;

    this.hubConnection.on('LeaderboardUpdated', (data: LeaderboardEntryDto[]) => {
      this.leaderboardUpdate$.next(data);
    });

    this.hubConnection.on('GameCompleted', (data: LeaderboardEntryDto) => {
      this.gameCompleted$.next(data);
    });

    this.hubConnection.on('ApplauseReceived', (playerId: string, count: number) => {
      this.applauseReceived$.next({ playerId, count });
    });
  }

  public joinGroup(groupName: string): void {
    if (this.hubConnection && this.hubConnection.state === signalR.HubConnectionState.Connected) {
      this.hubConnection.invoke('JoinGroup', groupName)
        .catch(err => console.error(err));
    } else if (this.hubConnection) {
      // If not connected yet, wait and try again or use a promise
      this.hubConnection.onreconnected(() => {
        this.hubConnection?.invoke('JoinGroup', groupName);
      });
    }
  }

  public leaveGroup(groupName: string): void {
    if (this.hubConnection && this.isConnected()) {
      this.hubConnection.invoke('LeaveGroup', groupName)
        .catch(err => console.error(err));
    }
  }

  public sendApplause(date: string, playerId: string): void {
    if (this.hubConnection && this.isConnected()) {
      this.hubConnection.invoke('SendApplause', date, playerId)
        .catch(err => console.error(err));
    }
  }
}
