import { Component, OnInit, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { VaccinesStore } from '../vaccines.store';
import { VaccineRecord } from '../models/vaccine.model';

@Component({
  selector: 'app-vaccine-list',
  standalone: true,
  imports: [
    DatePipe, RouterLink,
    MatButtonModule, MatIconModule, MatProgressSpinnerModule, MatTooltipModule,
  ],
  templateUrl: './vaccine-list.component.html',
})
export class VaccineListComponent implements OnInit {
  protected readonly store = inject(VaccinesStore);
  protected readonly downloading = signal(false);

  ngOnInit(): void {
    this.store.loadRecords();
  }

  protected async downloadPassport(): Promise<void> {
    this.downloading.set(true);
    try {
      await this.store.downloadPassport();
    } finally {
      this.downloading.set(false);
    }
  }

  protected statusBadge(r: VaccineRecord): { label: string; cls: string } {
    switch (r.status) {
      case 'overdue': return { label: 'Quá hạn', cls: 'badge-danger' };
      case 'upcoming': return { label: 'Sắp tiêm', cls: 'badge-warning' };
      case 'completed': return { label: 'Hoàn thành', cls: 'badge-success' };
      default: return { label: 'Đã lên lịch', cls: 'badge-info' };
    }
  }

  protected async delete(r: VaccineRecord, event: Event): Promise<void> {
    event.stopPropagation();
    if (confirm(`Xóa bản ghi tiêm "${r.vaccineName} mũi ${r.doseNumber}"?`)) {
      await this.store.deleteRecord(r.id);
    }
  }
}
