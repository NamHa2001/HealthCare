import { Component, OnInit, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { AdminStore } from '../admin.store';
import { EmptyStateComponent } from '../../../shared/components/empty-state/empty-state.component';

const RESOURCES = ['', 'Auth', 'HealthProfile', 'Measurement', 'BloodPressureLog', 'Vaccine', 'Medication', 'MedicalVisit', 'Reminder', 'Admin'];

@Component({
  selector: 'app-audit-log-list',
  standalone: true,
  imports: [DatePipe, MatIconModule, MatProgressSpinnerModule, EmptyStateComponent],
  templateUrl: './audit-log-list.component.html',
})
export class AuditLogListComponent implements OnInit {
  protected readonly store = inject(AdminStore);
  readonly resources = RESOURCES;
  filterResource = signal('');

  ngOnInit(): void {
    this.store.loadAuditLogs(undefined, 1);
  }

  onResourceChange(value: string): void {
    this.filterResource.set(value);
    this.store.loadAuditLogs(value || undefined, 1);
  }

  onPageChange(delta: number): void {
    const next = this.store.auditLogsPage() + delta;
    this.store.loadAuditLogs(this.filterResource() || undefined, next);
  }

  get totalPages(): number {
    return Math.max(1, Math.ceil(this.store.auditLogsTotal() / 30));
  }

  eventTypeClass(eventType: string): string {
    if (eventType.includes('Login') || eventType.includes('Register')) return 'badge-info';
    if (eventType.includes('Delete') || eventType.includes('Failed')) return 'badge-danger';
    if (eventType.includes('Create')) return 'badge-success';
    if (eventType.includes('Update')) return 'badge-warning';
    return 'badge-neutral';
  }
}
