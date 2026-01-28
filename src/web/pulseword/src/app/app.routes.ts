import { Routes } from '@angular/router';
import { GameComponent } from './features/game/game.component';

export const routes: Routes = [
  { path: '', component: GameComponent },
  { path: 'game', component: GameComponent },
  { 
    path: 'leaderboard', 
    loadComponent: () => import('./features/leaderboard/leaderboard.component').then(m => m.LeaderboardComponent) 
  },
  {
    path: 'profile/:id',
    loadComponent: () => import('./features/profile/profile.component').then(m => m.ProfileComponent)
  },
  { path: '**', redirectTo: '' }
];
