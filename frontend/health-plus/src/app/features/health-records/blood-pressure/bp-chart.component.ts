import { Component, Input, OnChanges, SimpleChanges } from '@angular/core';
import { NgApexchartsModule } from 'ng-apexcharts';
import {
  ApexAxisChartSeries, ApexChart, ApexXAxis, ApexYAxis,
  ApexTooltip, ApexStroke, ApexMarkers, ApexAnnotations,
} from 'ng-apexcharts';
import { BloodPressureLog } from '../models/blood-pressure-log.model';

@Component({
  selector: 'app-bp-chart',
  standalone: true,
  imports: [NgApexchartsModule],
  template: `
    <div class="bg-white rounded-xl border border-gray-100 p-4">
      <h3 class="text-sm font-medium text-gray-600 mb-3">Biểu đồ huyết áp (mmHg)</h3>
      <apx-chart
        [series]="series"
        [chart]="chartOptions"
        [xaxis]="xaxis"
        [yaxis]="yaxis"
        [stroke]="stroke"
        [markers]="markers"
        [annotations]="annotations"
        [tooltip]="tooltip" />
    </div>
  `,
})
export class BpChartComponent implements OnChanges {
  @Input() logs: BloodPressureLog[] = [];

  protected series: ApexAxisChartSeries = [];
  protected chartOptions: ApexChart = { type: 'line', height: 260, toolbar: { show: false }, zoom: { enabled: false } };
  protected xaxis: ApexXAxis = { type: 'datetime' };
  protected yaxis: ApexYAxis = { min: 40, max: 200 };
  protected stroke: ApexStroke = { curve: 'smooth', width: 2 };
  protected markers: ApexMarkers = { size: 4 };
  protected tooltip: ApexTooltip = { x: { format: 'dd/MM/yyyy HH:mm' } };
  protected annotations: ApexAnnotations = {
    yaxis: [
      { y: 140, borderColor: '#ef4444', label: { text: 'Ngưỡng tâm thu', style: { color: '#ef4444' } } },
      { y: 90,  borderColor: '#f97316', label: { text: 'Ngưỡng tâm trương', style: { color: '#f97316' } } },
    ],
  };

  ngOnChanges(_changes: SimpleChanges): void {
    const sorted = [...this.logs].sort(
      (a, b) => new Date(a.measuredAt).getTime() - new Date(b.measuredAt).getTime()
    );
    this.series = [
      {
        name: 'Tâm thu',
        data: sorted.map(l => ({ x: new Date(l.measuredAt).getTime(), y: l.systolic })),
      },
      {
        name: 'Tâm trương',
        data: sorted.map(l => ({ x: new Date(l.measuredAt).getTime(), y: l.diastolic })),
      },
    ];
  }
}
