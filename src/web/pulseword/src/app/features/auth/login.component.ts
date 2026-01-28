import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTabsModule } from '@angular/material/tabs';
import { MatDividerModule } from '@angular/material/divider';
import { AuthService } from '../../core/auth/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatTabsModule,
    MatDividerModule
  ],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss'
})
export class LoginComponent {
  private readonly fb = inject(FormBuilder);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  loginForm = this.fb.group({
    username: ['', [Validators.required]],
    password: ['', [Validators.required]]
  });

  anonymousForm = this.fb.group({
    displayName: ['', [Validators.required, Validators.minLength(3)]]
  });

  error = signal<string | null>(null);
  isLoading = signal(false);

  onSubmit(): void {
    if (this.loginForm.valid) {
      this.isLoading.set(true);
      this.error.set(null);
      
      const { username, password } = this.loginForm.value;
      this.authService.login({ username: username!, password: password! }).subscribe({
        next: () => this.handleSuccess(),
        error: (err) => {
          this.error.set(err.error?.message || 'Login failed. Please check your credentials.');
          this.isLoading.set(false);
        }
      });
    }
  }

  onAnonymousSubmit(): void {
    if (this.anonymousForm.valid) {
      this.isLoading.set(true);
      this.error.set(null);
      
      const { displayName } = this.anonymousForm.value;
      this.authService.loginAnonymous({ displayName: displayName! }).subscribe({
        next: () => this.handleSuccess(),
        error: (err) => {
          this.error.set(err.error?.message || 'Anonymous login failed.');
          this.isLoading.set(false);
        }
      });
    }
  }

  private handleSuccess(): void {
    const returnUrl = this.route.snapshot.queryParams['returnUrl'] || '/';
    this.router.navigateByUrl(returnUrl);
  }
}
