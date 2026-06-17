import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';
import { RemindersStore } from '../reminders.store';

@Component({
  selector: 'app-reminder-form',
  standalone: true,
  imports: [
    ReactiveFormsModule, RouterLink,
    MatButtonModule, MatFormFieldModule, MatIconModule,
    MatInputModule, MatProgressSpinnerModule, MatSelectModule,
  ],
  templateUrl: './reminder-form.component.html',
})
export class ReminderFormComponent {
  protected readonly store = inject(RemindersStore);
  private readonly router = inject(Router);
  private readonly fb = inject(FormBuilder);

  protected readonly saving = signal(false);
  protected readonly errorMsg = signal<string | null>(null);

  private readonly localNow = new Date(Date.now() - new Date().getTimezoneOffset() * 60000)
    .toISOString()
    .substring(0, 16);

  protected readonly form = this.fb.nonNullable.group({
    type: ['custom', Validators.required],
    title: ['', Validators.required],
    body: [''],
    remindAt: [this.localNow, Validators.required],
  });

  protected readonly types = [
    { value: 'vaccine',     label: 'Vắc-xin' },
    { value: 'medication',  label: 'Thuốc' },
    { value: 'appointment', label: 'Lịch khám' },
    { value: 'custom',      label: 'Tùy chỉnh' },
  ];

  protected async submit(): Promise<void> {
    if (this.form.invalid) return;
    this.saving.set(true);
    this.errorMsg.set(null);

    const raw = this.form.getRawValue();

    try {
      await this.store.createReminder({
        type: raw.type as any,
        title: raw.title,
        body: raw.body || null,
        remindAt: new Date(raw.remindAt).toISOString(),
      });
      await this.router.navigate(['/reminders']);
    } catch {
      this.errorMsg.set('Có lỗi xảy ra, vui lòng thử lại.');
    } finally {
      this.saving.set(false);
    }
  }
}
