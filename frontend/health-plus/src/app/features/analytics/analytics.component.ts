import { Component, OnInit, inject, signal } from '@angular/core';
import { DecimalPipe, NgClass, PercentPipe, SlicePipe } from '@angular/common';
import { MatButtonToggleModule } from '@angular/material/button-toggle';
import { MatIconModule } from '@angular/material/icon';
import { FormsModule } from '@angular/forms';
import { AnalyticsStore } from './analytics.store';
import { TrendPoint } from './models/analytics.model';
import { BmiTrendChartComponent } from '../dashboard/widgets/bmi-trend-chart/bmi-trend-chart.component';
import { BpTrendChartComponent } from '../dashboard/widgets/bp-trend-chart/bp-trend-chart.component';

@Component({
  selector: 'app-analytics',
  standalone: true,
  imports: [
    DecimalPipe,
    PercentPipe,
    NgClass,
    SlicePipe,
    FormsModule,
    MatButtonToggleModule,
    MatIconModule,
    BmiTrendChartComponent,
    BpTrendChartComponent,
  ],
  templateUrl: './analytics.component.html',
})
export class AnalyticsComponent implements OnInit {
  protected readonly store = inject(AnalyticsStore);
  protected readonly days = signal<number>(30);

  ngOnInit(): void {
    this.store.loadSummary();
    this.store.loadHealthScore();
    this.store.loadBmiTrend(this.days());
    this.store.loadBpTrend(this.days());
    this.store.loadGlucoseTrend(this.days());
    this.store.loadVaccineProgress();
    this.store.loadCompliance();
    this.store.loadVisitFrequency();
  }

  protected changeDays(d: number): void {
    this.days.set(d);
    this.store.loadBmiTrend(d);
    this.store.loadBpTrend(d);
    this.store.loadGlucoseTrend(d);
  }

  protected bmiClass(label: string | null): string {
    if (!label) return 'text-[#858796]';
    if (label.includes('Thiếu')) return 'text-[#4e73df]';
    if (label.includes('Bình')) return 'text-[#1cc88a]';
    if (label.includes('Thừa')) return 'text-[#f6c23e]';
    return 'text-[#e74a3b]';
  }

  protected bpClass(label: string | null): string {
    if (!label) return 'text-[#858796]';
    if (label.includes('Bình')) return 'text-[#1cc88a]';
    if (label.includes('Cao độ 1')) return 'text-[#f6c23e]';
    return 'text-[#e74a3b]';
  }

  protected glucoseClass(v: number | null): string {
    if (v === null) return 'text-[#858796]';
    return v > 7.0 ? 'text-[#e74a3b]' : 'text-[#1cc88a]';
  }

  protected maxCount(freq: { count: number }[]): number {
    return Math.max(...freq.map(f => f.count), 1);
  }

  protected gradeColor(grade: string): string {
    switch (grade) {
      case 'Xuất sắc':    return '#1cc88a';
      case 'Tốt':         return '#4e73df';
      case 'Trung bình':  return '#f6c23e';
      case 'Cần cải thiện': return '#fd7e14';
      default:            return '#e74a3b';
    }
  }

  protected glucosePolyline(pts: TrendPoint[]): string {
    if (pts.length < 2) return '';
    const values = pts.map(p => p.value);
    const max = Math.max(...values);
    const min = Math.min(...values);
    const range = max - min || 1;
    return pts.map((p, i) =>
      `${(i / (pts.length - 1)) * 400},${110 - ((p.value - min) / range) * 100}`
    ).join(' ');
  }

  protected glucoseCx(i: number, total: number): number {
    return total > 1 ? (i / (total - 1)) * 400 : 200;
  }

  protected glucoseCy(value: number, pts: TrendPoint[]): number {
    const values = pts.map(p => p.value);
    const max = Math.max(...values);
    const min = Math.min(...values);
    const range = max - min || 1;
    return 110 - ((value - min) / range) * 100;
  }
}
