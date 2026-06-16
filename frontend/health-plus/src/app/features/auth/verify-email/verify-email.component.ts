import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { ApiService } from '../../../core/services/api.service';

@Component({
  selector: 'app-verify-email',
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
  templateUrl: './verify-email.component.html',
})
export class VerifyEmailComponent implements OnInit {
  private readonly api = inject(ApiService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly fb = inject(FormBuilder);

  readonly isLoading = signal(false);
  readonly isSuccess = signal(false);

  readonly form = this.fb.group({
    // Token là Guid.ToString("N") — 32 ký tự hex không dấu gạch
    token: ['', [Validators.required, Validators.minLength(32), Validators.maxLength(32), Validators.pattern(/^[0-9a-fA-F]{32}$/)]],
  });

  ngOnInit(): void {
    const token = this.route.snapshot.queryParamMap.get('token');
    if (token) {
      this.form.patchValue({ token });
      this.doVerify(token);
    }
  }

  onSubmit(): void {
    if (this.isLoading() || this.isSuccess()) return;
    this.form.markAllAsTouched();
    if (this.form.invalid) return;
    this.doVerify(this.form.getRawValue().token!);
  }

  private doVerify(token: string): void {
    this.isLoading.set(true);
    this.api.post<void>('Auth/verify-email', { token }).subscribe({
      next: () => {
        this.isLoading.set(false);
        this.isSuccess.set(true);
      },
      error: () => this.isLoading.set(false),
    });
  }
}
