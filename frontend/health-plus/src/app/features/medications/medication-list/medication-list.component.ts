import { Component, OnInit, inject, signal } from '@angular/core';
import { DatePipe, NgTemplateOutlet, SlicePipe } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTabsModule } from '@angular/material/tabs';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MedicationsStore } from '../medications.store';
import { Medication } from '../models/medication.model';

@Component({
  selector: 'app-medication-list',
  standalone: true,
  imports: [
    DatePipe, SlicePipe, NgTemplateOutlet, RouterLink,
    MatButtonModule, MatIconModule, MatProgressSpinnerModule,
    MatTabsModule, MatTooltipModule,
  ],
  templateUrl: './medication-list.component.html',
})
export class MedicationListComponent implements OnInit {
  protected readonly store = inject(MedicationsStore);
  private readonly router = inject(Router);

  protected readonly tabIndex = signal(0);

  ngOnInit(): void {
    this.store.loadMedications(true);
  }

  protected onTabChange(index: number): void {
    this.tabIndex.set(index);
    this.store.loadMedications(index === 0);
  }

  protected openSchedule(med: Medication, event: Event): void {
    event.stopPropagation();
    this.router.navigate(['/medications', med.id, 'schedule']);
  }

  protected openEdit(med: Medication, event: Event): void {
    event.stopPropagation();
    this.router.navigate(['/medications', med.id, 'edit']);
  }

  protected async delete(med: Medication, event: Event): Promise<void> {
    event.stopPropagation();
    if (confirm(`Xóa thuốc "${med.drugName}"?`)) {
      await this.store.deleteMedication(med.id);
    }
  }

  protected statusBadge(med: Medication): { label: string; cls: string } {
    if (med.isOngoing) return { label: 'Đang dùng', cls: 'badge-success' };
    if (!med.endDate) return { label: 'Không xác định', cls: 'badge-info' };
    return new Date(med.endDate) >= new Date()
      ? { label: 'Đang dùng', cls: 'badge-success' }
      : { label: 'Đã hoàn thành', cls: 'badge-neutral' };
  }
}
