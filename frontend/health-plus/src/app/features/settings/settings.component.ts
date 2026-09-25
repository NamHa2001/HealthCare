import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatTabsModule } from '@angular/material/tabs';
import { firstValueFrom } from 'rxjs';
import { ApiService } from '../../core/services/api.service';
import { AuthStore } from '../../core/auth/auth.store';
import { User } from '../../core/auth/models/auth-response.model';
import { passwordStrengthValidator } from '../../shared/validators/password-strength.validator';
import { vietnamPhoneValidator } from '../../shared/validators/vietnam-phone.validator';

function confirmPasswordValidator(c: import('@angular/forms').AbstractControl) {
  const parent = c.parent;
  if (!parent) return null;
  return c.value === parent.get('newPassword')?.value ? null : { mismatch: true };
}

interface NotificationPreference {
  timezone: string;
  quietStartTime: string;
  quietEndTime: string;
  pushEnabled: boolean;
  emailEnabled: boolean;
  vaccineReminder: boolean;
  medicationReminder: boolean;
  followupReminder: boolean;
  healthAlert: boolean;
  doctorCriticalAlert: boolean;
  doctorDailyDigest: boolean;
}

@Component({
  selector: 'app-settings',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatTabsModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatSlideToggleModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
  ],
  templateUrl: './settings.component.html',
})
export class SettingsComponent implements OnInit {
  private readonly api       = inject(ApiService);
  private readonly authStore = inject(AuthStore);
  private readonly fb        = inject(FormBuilder);
  private readonly snack     = inject(MatSnackBar);

  protected readonly savingProfile  = signal(false);
  protected readonly savingPassword = signal(false);
  protected readonly savingNotif    = signal(false);
  protected readonly loadingNotif   = signal(false);
  protected readonly exportingData  = signal(false);
  protected readonly showCurrent    = signal(false);
  protected readonly showNew        = signal(false);
  protected readonly showConfirm    = signal(false);

  protected readonly isDoctor = this.authStore.isDoctor;

  protected readonly timezones = [
    { value: 'Asia/Ho_Chi_Minh', label: 'Hà Nội / TP.HCM (UTC+7)' },
    { value: 'Asia/Bangkok',     label: 'Bangkok (UTC+7)' },
    { value: 'Asia/Singapore',   label: 'Singapore (UTC+8)' },
    { value: 'Asia/Tokyo',       label: 'Tokyo (UTC+9)' },
    { value: 'Asia/Seoul',       label: 'Seoul (UTC+9)' },
    { value: 'UTC',              label: 'UTC (UTC+0)' },
  ];

  readonly profileForm = this.fb.group({
    firstName:   ['', [Validators.required, Validators.maxLength(100)]],
    lastName:    ['', [Validators.required, Validators.maxLength(100)]],
    phoneNumber: ['', [vietnamPhoneValidator()]],
  });

  readonly passwordForm = this.fb.group({
    currentPassword: ['', Validators.required],
    newPassword:     ['', [Validators.required, Validators.minLength(8), passwordStrengthValidator]],
    confirmPassword: ['', [Validators.required, confirmPasswordValidator]],
  });

  readonly notifForm = this.fb.group({
    timezone:           ['Asia/Ho_Chi_Minh', Validators.required],
    quietStartTime:     ['22:00', Validators.required],
    quietEndTime:       ['07:00', Validators.required],
    pushEnabled:        [true],
    emailEnabled:       [true],
    vaccineReminder:    [true],
    medicationReminder: [true],
    followupReminder:   [true],
    healthAlert:        [true],
    doctorCriticalAlert: [true],
    doctorDailyDigest:   [true],
  });

  async ngOnInit(): Promise<void> {
    const user = this.authStore.currentUser();
    if (user) {
      this.profileForm.patchValue({
        firstName:   user.firstName,
        lastName:    user.lastName,
        phoneNumber: user.phoneNumber ?? '',
      });
    }
    await this.loadNotifPreference();
  }

  private async loadNotifPreference(): Promise<void> {
    this.loadingNotif.set(true);
    try {
      const pref = await firstValueFrom(
        this.api.get<NotificationPreference>('notifications/preferences')
      );
      this.notifForm.patchValue(pref);
    } catch {
      // dùng defaults
    } finally {
      this.loadingNotif.set(false);
    }
  }

  protected saveProfile(): void {
    if (this.savingProfile()) return;
    this.profileForm.markAllAsTouched();
    if (this.profileForm.invalid) return;

    const { firstName, lastName, phoneNumber } = this.profileForm.getRawValue();
    this.savingProfile.set(true);

    this.api.put<User>('auth/profile', {
      firstName,
      lastName,
      phoneNumber: phoneNumber || null,
    }).subscribe({
      next: (updated) => {
        this.authStore.setUser(updated);
        this.savingProfile.set(false);
        this.snack.open('Cập nhật thông tin thành công!', 'Đóng', { duration: 3000 });
      },
      error: () => {
        this.savingProfile.set(false);
        this.snack.open('Có lỗi xảy ra, vui lòng thử lại.', 'Đóng', { duration: 3000 });
      },
    });
  }

  protected changePassword(): void {
    if (this.savingPassword()) return;
    this.passwordForm.markAllAsTouched();
    if (this.passwordForm.invalid) return;

    const { currentPassword, newPassword } = this.passwordForm.getRawValue();
    this.savingPassword.set(true);

    this.api.post<void>('auth/change-password', { currentPassword, newPassword }).subscribe({
      next: () => {
        this.passwordForm.reset();
        this.savingPassword.set(false);
        this.snack.open('Đổi mật khẩu thành công!', 'Đóng', { duration: 3000 });
      },
      error: (err) => {
        this.savingPassword.set(false);
        const msg = err?.error?.error?.message ?? 'Có lỗi xảy ra, vui lòng thử lại.';
        this.snack.open(msg, 'Đóng', { duration: 4000 });
      },
    });
  }

  protected async exportData(): Promise<void> {
    if (this.exportingData()) return;
    this.exportingData.set(true);
    try {
      const blob = await firstValueFrom(
        this.api.getBlob('auth/me/export')
      );
      const url = URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `healthplus-data-${new Date().toISOString().slice(0, 10)}.json`;
      a.click();
      URL.revokeObjectURL(url);
      this.snack.open('Đã tải xuống dữ liệu của bạn.', 'Đóng', { duration: 3000 });
    } catch {
      this.snack.open('Không thể xuất dữ liệu, vui lòng thử lại.', 'Đóng', { duration: 3000 });
    } finally {
      this.exportingData.set(false);
    }
  }

  protected saveNotifPreference(): void {
    if (this.savingNotif()) return;
    this.savingNotif.set(true);

    this.api.put<NotificationPreference>('notifications/preferences', this.notifForm.getRawValue()).subscribe({
      next: (updated) => {
        this.notifForm.patchValue(updated);
        this.savingNotif.set(false);
        this.snack.open('Đã lưu cài đặt thông báo!', 'Đóng', { duration: 3000 });
      },
      error: () => {
        this.savingNotif.set(false);
        this.snack.open('Có lỗi xảy ra, vui lòng thử lại.', 'Đóng', { duration: 3000 });
      },
    });
  }
}
