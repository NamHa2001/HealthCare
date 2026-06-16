import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { ApiService } from '../../../core/services/api.service';

@Component({
  selector: 'app-forgot-password',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    RouterLink,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
  ],
  templateUrl: './forgot-password.component.html',
})
export class ForgotPasswordComponent {
  private readonly api = inject(ApiService);
  private readonly fb = inject(FormBuilder);

  readonly isLoading = signal(false);
  readonly isSubmitted = signal(false);
  readonly submittedEmail = signal('');

  readonly form = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
  });

  onSubmit(): void {
    if (this.isLoading()) return;
    this.form.markAllAsTouched();
    if (this.form.invalid) return;

    const email = this.form.getRawValue().email!;
    this.isLoading.set(true);
    this.api.post<void>('Auth/forgot-password', { email }).subscribe({
      next: () => {
        this.submittedEmail.set(email);
        this.isLoading.set(false);
        this.isSubmitted.set(true);
      },
      error: () => this.isLoading.set(false),
    });
  }
}
