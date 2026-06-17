import { Component, OnInit, inject } from '@angular/core';
import { DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatChipsModule } from '@angular/material/chips';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { RemindersStore } from '../reminders.store';
import { Reminder, ReminderStatus } from '../models/reminder.model';

@Component({
  selector: 'app-reminder-list',
  standalone: true,
  imports: [
    DatePipe, RouterLink,
    MatButtonModule, MatChipsModule, MatIconModule,
    MatProgressSpinnerModule, MatTooltipModule,
  ],
  templateUrl: './reminder-list.component.html',
})
export class ReminderListComponent implements OnInit {
  protected readonly store = inject(RemindersStore);

  protected readonly filters: { label: string; value: ReminderStatus | null }[] = [
    { label: 'Tất cả', value: null },
    { label: 'Chờ gửi', value: 'pending' },
    { label: 'Đã gửi', value: 'sent' },
    { label: 'Lỗi', value: 'failed' },
    { label: 'Đã hủy', value: 'cancelled' },
  ];

  ngOnInit(): void {
    this.store.loadReminders(null);
  }

  protected applyFilter(status: ReminderStatus | null): void {
    this.store.setFilter(status);
    this.store.loadReminders(status);
  }

  protected async cancel(r: Reminder, event: Event): Promise<void> {
    event.stopPropagation();
    if (!confirm(`Hủy nhắc lịch "${r.title}"?`)) return;
    await this.store.cancelReminder(r.id);
  }

  protected statusInfo(status: ReminderStatus): { label: string; cls: string; icon: string } {
    switch (status) {
      case 'pending':  return { label: 'Chờ gửi',  cls: 'badge-warning', icon: 'schedule' };
      case 'sent':     return { label: 'Đã gửi',   cls: 'badge-success', icon: 'check_circle' };
      case 'failed':   return { label: 'Lỗi',      cls: 'badge-danger',  icon: 'error' };
      case 'cancelled':return { label: 'Đã hủy',   cls: 'badge-neutral',   icon: 'cancel' };
    }
  }

  protected typeIcon(type: string): string {
    switch (type) {
      case 'vaccine':     return 'vaccines';
      case 'medication':  return 'medication';
      case 'appointment': return 'event';
      default:            return 'notifications';
    }
  }
}
