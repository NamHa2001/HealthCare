import { Component, OnInit, inject, signal } from '@angular/core';
import { DatePipe, SlicePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MedicationsStore } from '../medications.store';

@Component({
  selector: 'app-medication-log',
  standalone: true,
  imports: [
    DatePipe, SlicePipe, RouterLink,
    MatButtonModule, MatFormFieldModule, MatIconModule,
    MatInputModule, MatProgressSpinnerModule, MatTooltipModule,
  ],
  templateUrl: './medication-log.component.html',
})
export class MedicationLogComponent implements OnInit {
  protected readonly store = inject(MedicationsStore);

  protected readonly skipReason = signal<Record<string, string | undefined>>({});
  protected readonly showSkipInput = signal<Record<string, boolean>>({});
  protected readonly acting = signal<Record<string, boolean>>({});

  ngOnInit(): void {
    this.store.loadTodayLogs();
  }

  protected onDateChange(event: Event): void {
    const value = (event.target as HTMLInputElement).value;
    if (value) this.store.loadTodayLogs(value);
  }

  protected async markTaken(logId: string): Promise<void> {
    this.acting.set({ ...this.acting(), [logId]: true });
    try {
      await this.store.markTaken(logId);
    } finally {
      this.acting.set({ ...this.acting(), [logId]: false });
    }
  }

  protected toggleSkipInput(logId: string): void {
    this.showSkipInput.set({ ...this.showSkipInput(), [logId]: !this.showSkipInput()[logId] });
  }

  protected async markSkipped(logId: string): Promise<void> {
    this.acting.set({ ...this.acting(), [logId]: true });
    try {
      const reason = this.skipReason()[logId] ?? undefined;
      await this.store.markSkipped(logId, reason);
      this.showSkipInput.set({ ...this.showSkipInput(), [logId]: false });
    } finally {
      this.acting.set({ ...this.acting(), [logId]: false });
    }
  }

  protected onSkipReasonChange(logId: string, value: string | undefined): void {
    this.skipReason.set({ ...this.skipReason(), [logId]: value });
  }

  protected statusInfo(status: string): { icon: string; cls: string; label: string } {
    switch (status) {
      case 'Taken': return { icon: 'check_circle', cls: 'text-[#1cc88a]', label: 'Đã uống' };
      case 'Skipped': return { icon: 'cancel', cls: 'text-[#e74a3b]', label: 'Bỏ qua' };
      default: return { icon: 'radio_button_unchecked', cls: 'text-[#858796]', label: 'Chờ uống' };
    }
  }
}
