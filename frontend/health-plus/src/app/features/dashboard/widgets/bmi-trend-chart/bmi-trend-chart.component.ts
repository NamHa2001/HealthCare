import { Component, Input, OnChanges } from '@angular/core';
import { NgApexchartsModule } from 'ng-apexcharts';
import { ApexAxisChartSeries, ApexChart, ApexXAxis, ApexYAxis, ApexStroke, ApexAnnotations, ApexTooltip } from 'ng-apexcharts';
import { TrendPoint } from '../../../analytics/models/analytics.model';

@Component({
  selector: 'app-bmi-trend-chart',
  standalone: true,
  imports: [NgApexchartsModule],
  template: `
    <div class="sb-card">
      <div class="sb-card-header">
        <h2 class="sb-card-title">Xu hướng BMI (30 ngày)</h2>
      </div>
      @if (points.length === 0) {
        <div class="flex flex-col items-center justify-center py-10 text-[#858796]">
          <span class="text-sm">Chưa đủ dữ liệu</span>
        </div>
      } @else {
        <div class="p-3">
          <apx-chart [series]="series" [chart]="chart" [xaxis]="xaxis" [yaxis]="yaxis"
            [stroke]="stroke" [annotations]="annotations" [tooltip]="tooltip" />
        </div>
      }
    </div>
  `,
})
export class BmiTrendChartComponent implements OnChanges {
  @Input() points: TrendPoint[] = [];

  protected series: ApexAxisChartSeries = [];
  protected chart: ApexChart = { type: 'line', height: 200, toolbar: { show: false }, zoom: { enabled: false }, sparkline: { enabled: false } };
  protected xaxis: ApexXAxis = { type: 'category', labels: { rotate: -30, style: { fontSize: '10px' } } };
  protected yaxis: ApexYAxis = { min: 14, max: 40, labels: { formatter: (v: number) => v.toFixed(1) } };
  protected stroke: ApexStroke = { curve: 'smooth', width: 2, colors: ['#1cc88a'] };
  protected tooltip: ApexTooltip = { y: { formatter: (v: number) => v.toFixed(1) } };
  protected annotations: ApexAnnotations = {
    yaxis: [
      { y: 18.5, y2: 23, fillColor: '#1cc88a', opacity: 0.07, label: { text: 'Bình thường', style: { fontSize: '10px' } } },
      { y: 25, borderColor: '#e74a3b', label: { text: 'Béo phì', style: { color: '#e74a3b', fontSize: '10px' } } },
    ],
  };

  ngOnChanges(): void {
    this.series = [{
      name: 'BMI',
      data: this.points.map(p => ({ x: p.date, y: p.value })),
      color: '#1cc88a',
    }];
  }
}
