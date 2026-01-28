import { Injectable, signal, computed, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { catchError, map, Observable, of, tap } from 'rxjs';
import { AuthResponse, LoginRequest, AnonymousRequest, UserInfo, TokenPayload } from './auth.models';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);

  private readonly ACCESS_TOKEN_KEY = 'access_token';
  private readonly REFRESH_TOKEN_KEY = 'refresh_token';

  private readonly userSignal = signal<UserInfo | null>(null);
  readonly currentUser = this.userSignal.asReadonly();
  readonly isAuthenticated = computed(() => !!this.userSignal());

  private refreshTimer?: any;

  constructor() {
    this.initializeAuth();
  }

  private initializeAuth(): void {
    const token = localStorage.getItem(this.ACCESS_TOKEN_KEY);
    if (token) {
      const payload = this.decodeToken(token);
      if (payload && payload.exp * 1000 > Date.now()) {
        this.setUserFromToken(payload);
        this.scheduleTokenRefresh(payload.exp);
      } else {
        this.logout();
      }
    }
  }

  login(request: LoginRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>('/api/auth/login', request).pipe(
      tap((response) => this.handleAuthResponse(response))
    );
  }

  loginAnonymous(request: AnonymousRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>('/api/auth/anonymous', request).pipe(
      tap((response) => this.handleAuthResponse(response))
    );
  }

  logout(): void {
    localStorage.removeItem(this.ACCESS_TOKEN_KEY);
    localStorage.removeItem(this.REFRESH_TOKEN_KEY);
    this.userSignal.set(null);
    this.clearRefreshTimer();
    this.router.navigate(['/login']);
  }

  refreshToken(): Observable<AuthResponse> {
    const refreshToken = localStorage.getItem(this.REFRESH_TOKEN_KEY);
    if (!refreshToken) {
      this.logout();
      return of(null as any);
    }

    return this.http.post<AuthResponse>('/api/auth/refresh', { refreshToken }).pipe(
      tap((response) => this.handleAuthResponse(response)),
      catchError((error) => {
        this.logout();
        throw error;
      })
    );
  }

  private handleAuthResponse(response: AuthResponse): void {
    localStorage.setItem(this.ACCESS_TOKEN_KEY, response.accessToken);
    localStorage.setItem(this.REFRESH_TOKEN_KEY, response.refreshToken);
    const payload = this.decodeToken(response.accessToken);
    if (payload) {
      this.setUserFromToken(payload);
      this.scheduleTokenRefresh(payload.exp);
    }
  }

  private setUserFromToken(payload: TokenPayload): void {
    this.userSignal.set({
      id: payload.sub,
      username: payload.username,
      isAnonymous: payload.isAnonymous,
      roles: []
    });
  }

  private scheduleTokenRefresh(expiry: number): void {
    this.clearRefreshTimer();
    const delay = (expiry * 1000 - Date.now()) - 60000; // Refresh 1 minute before expiry
    if (delay > 0) {
      this.refreshTimer = setTimeout(() => {
        this.refreshToken().subscribe();
      }, delay);
    } else {
      this.refreshToken().subscribe();
    }
  }

  private clearRefreshTimer(): void {
    if (this.refreshTimer) {
      clearTimeout(this.refreshTimer);
      this.refreshTimer = undefined;
    }
  }

  private decodeToken(token: string): TokenPayload | null {
    try {
      const base64Url = token.split('.')[1];
      const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
      const jsonPayload = decodeURIComponent(atob(base64).split('').map(function(c) {
        return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
      }).join(''));
      return JSON.parse(jsonPayload);
    } catch (e) {
      return null;
    }
  }

  getAccessToken(): string | null {
    return localStorage.getItem(this.ACCESS_TOKEN_KEY);
  }
}
