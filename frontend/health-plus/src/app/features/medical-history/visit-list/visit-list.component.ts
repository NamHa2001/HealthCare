import { Component, OnInit, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MedicalHistoryStore } from '../medical-history.store';
import { MedicalVisitListItem } from '../models/medical-visit.model';

@Component({
  selector: 'app-visit-list',
  standalone: true,
  imports: [
    DatePipe, RouterLink,
    MatButtonModule, MatIconModule, MatProgressSpinnerModule, MatTooltipModule,
    MatFormFieldModule, MatSelectModule,
  ],
  templateUrl: './visit-list.component.html',
})
export class VisitListComponent implements OnInit {
  protected readonly store = inject(MedicalHistoryStore);
  private readonly router = inject(Router);

  protected readonly Math = Math;

  // Bộ lọc năm: 6 năm gần nhất; tháng 1-12.
  protected readonly years = signal<number[]>(
    Array.from({ length: 6 }, (_, i) => new Date().getFullYear() - i)
  );
  protected readonly months = signal<number[]>(
    Array.from({ length: 12 }, (_, i) => i + 1)
  );

  ngOnInit(): void {
    this.store.loadVisits({ page: 1 });
  }

  protected onYearChange(value: number | null): void {
    this.store.loadVisits({ page: 1, year: value, month: this.store.filterMonth() });
  }

  protected onMonthChange(value: number | null): void {
    this.store.loadVisits({ page: 1, year: this.store.filterYear(), month: value });
  }

  protected clearFilter(): void {
    this.store.loadVisits({ page: 1, year: null, month: null });
  }

  protected loadPage(page: number): void {
    this.store.loadVisits({ page });
  }

  protected openDetail(v: MedicalVisitListItem): void {
    this.router.navigate(['/medical-history/visits', v.id]);
  }

  protected delete(v: MedicalVisitListItem, event: Event): void {
    event.stopPropagation();
    if (confirm(`Xóa lần khám tại "${v.facilityName}" ngày ${new Date(v.visitDate).toLocaleDateString('vi-VN')}?`)) {
      this.store.deleteVisit(v.id);
    }
  }
}
