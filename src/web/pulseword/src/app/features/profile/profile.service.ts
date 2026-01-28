import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { UserProfileDto } from '../../shared/models/api.models';

@Injectable({
  providedIn: 'root'
})
export class ProfileService {
  private http = inject(HttpClient);
  private apiUrl = '/api/users';

  getProfile(id: string): Observable<UserProfileDto> {
    return this.http.get<UserProfileDto>(`${this.apiUrl}/${id}/profile`);
  }

  followUser(id: string): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${id}/follow`, {});
  }

  getFollowers(id: string): Observable<UserProfileDto[]> {
    return this.http.get<UserProfileDto[]>(`${this.apiUrl}/${id}/followers`);
  }

  getFollowing(id: string): Observable<UserProfileDto[]> {
    return this.http.get<UserProfileDto[]>(`${this.apiUrl}/${id}/following`);
  }
}
