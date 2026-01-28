import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTabsModule } from '@angular/material/tabs';
import { ProfileService } from './profile.service';
import { UserProfileDto } from '../../shared/models/api.models';
import { Observable, switchMap, tap } from 'rxjs';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatTabsModule
  ],
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.scss'
})
export class ProfileComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private profileService = inject(ProfileService);

  profile$!: Observable<UserProfileDto>;
  followers$!: Observable<UserProfileDto[]>;
  following$!: Observable<UserProfileDto[]>;
  isFollowing = false; // This should ideally come from the API

  ngOnInit(): void {
    this.profile$ = this.route.paramMap.pipe(
      switchMap(params => {
        const id = params.get('id')!;
        this.loadSocialData(id);
        return this.profileService.getProfile(id);
      })
    );
  }

  loadSocialData(id: string): void {
    this.followers$ = this.profileService.getFollowers(id);
    this.following$ = this.profileService.getFollowing(id);
    // TODO: Check if current user is following this user
  }

  toggleFollow(id: string): void {
    this.profileService.followUser(id).subscribe(() => {
      this.isFollowing = !this.isFollowing;
      this.loadSocialData(id);
    });
  }
}
