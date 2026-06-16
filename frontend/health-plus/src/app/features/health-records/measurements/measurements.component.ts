import { Component, OnInit, inject, signal } from '@angular/core';
import { DatePipe, DecimalPipe } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatDialog } from '@angular/material/dialog';
import { HealthRecordsStore } from '../health-records.store';
import { BmiClassPipe } from '../../../shared/pipes/bmi-class.pipe';
import { MeasurementFormComponent } from './measurement-form.component';
import { MeasurementChartComponent } from './measurement-chart.component';
import { HealthMeasurement } from '../models/health-measurement.model';

@Component({
  selector: 'app-measurements',
  standalone: true,
  imports: [
    DatePipe, DecimalPipe,
    MatButtonModule, MatIconModule, MatProgressSpinnerModule, MatTooltipModule,
    BmiClassPipe, MeasurementChartComponent,
  ],
  templateUrl: './measurements.component.html',
})
export class MeasurementsComponent implements OnInit {
  protected readonly store = inject(HealthRecordsStore);
  private readonly dialog = inject(MatDialog);

  protected readonly showChart = signal(false);
  protected readonly chartMetric = signal<string>('weightKg');
  protected readonly Math = Math;

  ngOnInit(): void {
    this.store.loadMeasurements({ page: 1, pageSize: 20 });
  }

  protected openForm(): void {
    const ref = this.dialog.open(MeasurementFormComponent, { width: '480px' });
    ref.afterClosed().subscribe(saved => {
      if (saved) this.store.loadMeasurements({ page: 1 });
    });
  }

  protected toggleChart(metric: string): void {
    if (this.showChart() && this.chartMetric() === metric) {
      this.showChart.set(false);
    } else {
      this.chartMetric.set(metric);
      this.showChart.set(true);
    }
  }

  protected delete(m: HealthMeasurement): void {
    if (confirm(`Xóa bản ghi đo lúc ${new Date(m.measuredAt).toLocaleString('vi-VN')}?`)) {
      this.store.deleteMeasurement(m.id);
    }
  }

  protected loadPage(page: number): void {
    this.store.loadMeasurements({ page });
  }
}
