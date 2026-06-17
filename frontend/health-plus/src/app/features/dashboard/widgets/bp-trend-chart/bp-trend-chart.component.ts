import { Component, Input, OnChanges } from '@angular/core';
import { NgApexchartsModule } from 'ng-apexcharts';
import { ApexAxisChartSeries, ApexChart, ApexXAxis, ApexYAxis, ApexStroke, ApexAnnotations, ApexTooltip, ApexMarkers } from 'ng-apexcharts';
import { BpTrendPoint } from '../../../analytics/models/analytics.model';

@Component({
  selector: 'app-bp-trend-chart',
  standalone: true,
  imports: [NgApexchartsModule],
  template: `
    <div class="sb-card">
      <div class="sb-card-header">
        <h2 class="sb-card-title">Xu hướng huyết áp (30 ngày)</h2>
      </div>
      @if (points.length === 0) {
        <div class="flex flex-col items-center justify-center py-10 text-[#858796]">
          <span class="text-sm">Chưa đủ dữ liệu</span>
        </div>
      } @else {
        <div class="p-3">
          <apx-chart [series]="series" [chart]="chart" [xaxis]="xaxis" [yaxis]="yaxis"
            [stroke]="stroke" [markers]="markers" [annotations]="annotations" [tooltip]="tooltip" />
        </div>
      }
    </div>
  `,
})
export class BpTrendChartComponent implements OnChanges {
  @Input() points: BpTrendPoint[] = [];

  protected series: ApexAxisChartSeries = [];
  protected chart: ApexChart = { type: 'line', height: 200, toolbar: { show: false }, zoom: { enabled: false } };
  protected xaxis: ApexXAxis = { type: 'category', labels: { rotate: -30, style: { fontSize: '10px' } } };
  protected yaxis: ApexYAxis = { min: 40, max: 200 };
  protected stroke: ApexStroke = { curve: 'smooth', width: 2 };
  protected markers: ApexMarkers = { size: 3 };
  protected tooltip: ApexTooltip = { shared: true };
  protected annotations: ApexAnnotations = {
    yaxis: [
      { y: 140, borderColor: '#e74a3b', label: { text: '140', style: { color: '#e74a3b', fontSize: '10px' } } },
      { y: 90, borderColor: '#f6c23e', label: { text: '90', style: { color: '#f6c23e', fontSize: '10px' } } },
    ],
  };

  ngOnChanges(): void {
    this.series = [
      {
        name: 'Tâm thu',
        data: this.points.map(p => ({ x: p.date, y: p.systolic })),
        color: '#e74a3b',
      },
      {
        name: 'Tâm trương',
        data: this.points.map(p => ({ x: p.date, y: p.diastolic })),
        color: '#4e73df',
      },
    ];
  }
}
