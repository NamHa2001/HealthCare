import { Component, OnInit, inject, signal } from '@angular/core';
import { AbstractControl, FormBuilder, ReactiveFormsModule, ValidatorFn, Validators } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { ApiService } from '../../../core/services/api.service';
import { passwordStrengthValidator } from '../../../shared/validators/password-strength.validator';

function confirmPasswordValidator(passwordKey: string): ValidatorFn {
  return (control: AbstractControl) => {
    const password = control.parent?.get(passwordKey)?.value;
    return control.value === password ? null : { passwordMismatch: true };
  };
}

@Component({
  selector: 'app-reset-password',
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
  templateUrl: './reset-password.component.html',
})
export class ResetPasswordComponent implements OnInit {
  private readonly api = inject(ApiService);
  private readonly route = inject(ActivatedRoute);
  private readonly fb = inject(FormBuilder);

  readonly isLoading = signal(false);
  readonly isSuccess = signal(false);
  readonly showPassword = signal(false);
  readonly showConfirmPassword = signal(false);

  readonly form = this.fb.group({
    token: ['', [Validators.required, Validators.minLength(32), Validators.maxLength(32), Validators.pattern(/^[0-9a-fA-F]{32}$/)]],
    newPassword: ['', [Validators.required, passwordStrengthValidator()]],
    confirmPassword: ['', [Validators.required, confirmPasswordValidator('newPassword')]],
  });

  ngOnInit(): void {
    const token = this.route.snapshot.queryParamMap.get('token');
    if (token) {
      this.form.patchValue({ token });
    }

    this.form.get('newPassword')!.valueChanges.subscribe(() => {
      this.form.get('confirmPassword')!.updateValueAndValidity({ emitEvent: false });
    });
  }

  onSubmit(): void {
    if (this.isLoading()) return;
    this.form.markAllAsTouched();
    if (this.form.invalid) return;

    const { token, newPassword } = this.form.getRawValue();
    this.isLoading.set(true);
    this.api.post<void>('Auth/reset-password', { token, newPassword }).subscribe({
      next: () => {
        this.isLoading.set(false);
        this.isSuccess.set(true);
      },
      error: () => this.isLoading.set(false),
    });
  }

  togglePassword(): void {
    this.showPassword.update(v => !v);
  }

  toggleConfirmPassword(): void {
    this.showConfirmPassword.update(v => !v);
  }
}
