import { Component, OnInit, inject, signal } from '@angular/core';
import { AbstractControl, FormBuilder, ReactiveFormsModule, ValidatorFn, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { AuthService } from '../../../core/auth/auth.service';
import { RegisterRequest } from '../../../core/auth/models/register-request.model';
import { passwordStrengthValidator } from '../../../shared/validators/password-strength.validator';
import { vietnamPhoneValidator } from '../../../shared/validators/vietnam-phone.validator';

function confirmPasswordValidator(passwordKey: string): ValidatorFn {
  return (control: AbstractControl) => {
    const password = control.parent?.get(passwordKey)?.value;
    return control.value === password ? null : { passwordMismatch: true };
  };
}

@Component({
  selector: 'app-register',
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
  templateUrl: './register.component.html',
})
export class RegisterComponent implements OnInit {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly fb = inject(FormBuilder);

  readonly step = signal<1 | 2>(1);
  readonly isLoading = signal(false);
  readonly showPassword = signal(false);
  readonly showConfirmPassword = signal(false);
  readonly registeredEmail = signal('');

  readonly form = this.fb.group({
    firstName: ['', [Validators.required, Validators.maxLength(100)]],
    lastName: ['', [Validators.required, Validators.maxLength(100)]],
    email: ['', [Validators.required, Validators.email, Validators.maxLength(256)]],
    phoneNumber: ['' as string | null, [vietnamPhoneValidator()]],
    password: ['', [Validators.required, passwordStrengthValidator()]],
    confirmPassword: ['', [Validators.required, confirmPasswordValidator('password')]],
  });

  ngOnInit(): void {
    // Khi password thay đổi, re-validate confirmPassword
    this.form.get('password')!.valueChanges.subscribe(() => {
      this.form.get('confirmPassword')!.updateValueAndValidity({ emitEvent: false });
    });
  }

  onSubmit(): void {
    if (this.isLoading()) return;
    this.form.markAllAsTouched();
    if (this.form.invalid) return;

    this.isLoading.set(true);
    const raw = this.form.getRawValue();
    const request: RegisterRequest = {
      email: raw.email!,
      password: raw.password!,
      firstName: raw.firstName!,
      lastName: raw.lastName!,
      phoneNumber: raw.phoneNumber || null,
    };

    this.authService.register(request).subscribe({
      next: () => {
        this.registeredEmail.set(request.email);
        this.isLoading.set(false);
        this.step.set(2);
      },
      error: () => this.isLoading.set(false),
    });
  }

  goToVerifyEmail(): void {
    this.router.navigate(['/auth/verify-email']);
  }

  togglePassword(): void {
    this.showPassword.update(v => !v);
  }

  toggleConfirmPassword(): void {
    this.showConfirmPassword.update(v => !v);
  }
}
