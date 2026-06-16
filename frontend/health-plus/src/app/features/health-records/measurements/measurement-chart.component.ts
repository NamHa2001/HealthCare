import { Component, Input, OnChanges, SimpleChanges } from '@angular/core';
import { NgApexchartsModule } from 'ng-apexcharts';
import {
  ApexAxisChartSeries, ApexChart, ApexXAxis, ApexYAxis,
  ApexTooltip, ApexStroke, ApexMarkers,
} from 'ng-apexcharts';
import { HealthMeasurement } from '../models/health-measurement.model';

const METRIC_LABELS: Record<string, { label: string; unit: string }> = {
  weightKg:        { label: 'Cân nặng', unit: 'kg' },
  heightCm:        { label: 'Chiều cao', unit: 'cm' },
  bmi:             { label: 'BMI', unit: 'kg/m²' },
  heartRateBpm:    { label: 'Nhịp tim', unit: 'bpm' },
  bodyTemperature: { label: 'Nhiệt độ', unit: '°C' },
  bloodGlucose:    { label: 'Đường huyết', unit: 'mmol/L' },
  spo2Percent:     { label: 'SpO₂', unit: '%' },
};

@Component({
  selector: 'app-measurement-chart',
  standalone: true,
  imports: [NgApexchartsModule],
  template: `
    <div class="bg-white rounded-xl border border-gray-100 p-4">
      <h3 class="text-sm font-medium text-gray-600 mb-3">
        {{ label }} ({{ unit }})
      </h3>
      <apx-chart
        [series]="series"
        [chart]="chartOptions"
        [xaxis]="xaxis"
        [yaxis]="yaxis"
        [stroke]="stroke"
        [markers]="markers"
        [tooltip]="tooltip" />
    </div>
  `,
})
export class MeasurementChartComponent implements OnChanges {
  @Input() measurements: HealthMeasurement[] = [];
  @Input() metric = 'weightKg';

  protected label = '';
  protected unit = '';
  protected series: ApexAxisChartSeries = [];
  protected chartOptions: ApexChart = { type: 'line', height: 250, toolbar: { show: false }, zoom: { enabled: false } };
  protected xaxis: ApexXAxis = { type: 'datetime' };
  protected yaxis: ApexYAxis = { labels: { formatter: v => v?.toFixed(1) ?? '' } };
  protected stroke: ApexStroke = { curve: 'smooth', width: 2 };
  protected markers: ApexMarkers = { size: 4 };
  protected tooltip: ApexTooltip = {
    x: { format: 'dd/MM/yyyy HH:mm' },
  };

  ngOnChanges(_changes: SimpleChanges): void {
    const meta = METRIC_LABELS[this.metric] ?? { label: this.metric, unit: '' };
    this.label = meta.label;
    this.unit = meta.unit;

    const points = this.measurements
      .filter(m => ((m as unknown) as Record<string, unknown>)[this.metric] != null)
      .map(m => ({
        x: new Date(m.measuredAt).getTime(),
        y: Number(((m as unknown) as Record<string, unknown>)[this.metric]),
      }))
      .sort((a, b) => a.x - b.x);

    this.series = [{ name: meta.label, data: points }];
  }
}
