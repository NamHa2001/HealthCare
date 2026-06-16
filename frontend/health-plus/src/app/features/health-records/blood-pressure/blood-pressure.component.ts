import { Component, OnInit, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatDialog } from '@angular/material/dialog';
import { HealthRecordsStore } from '../health-records.store';
import { BpClassPipe } from '../../../shared/pipes/bp-class.pipe';
import { BpFormComponent } from './bp-form.component';
import { BpChartComponent } from './bp-chart.component';
import { BloodPressureLog } from '../models/blood-pressure-log.model';

@Component({
  selector: 'app-blood-pressure',
  standalone: true,
  imports: [
    DatePipe,
    MatButtonModule, MatIconModule, MatProgressSpinnerModule, MatTooltipModule,
    BpClassPipe, BpChartComponent,
  ],
  templateUrl: './blood-pressure.component.html',
})
export class BloodPressureComponent implements OnInit {
  protected readonly store = inject(HealthRecordsStore);
  private readonly dialog = inject(MatDialog);

  protected readonly showChart = signal(false);
  protected readonly Math = Math;

  ngOnInit(): void {
    this.store.loadBpLogs({ page: 1, pageSize: 20 });
  }

  protected openForm(): void {
    const ref = this.dialog.open(BpFormComponent, { width: '440px' });
    ref.afterClosed().subscribe(saved => {
      if (saved) this.store.loadBpLogs({ page: 1 });
    });
  }

  protected delete(log: BloodPressureLog): void {
    if (confirm(`Xóa bản ghi huyết áp ${log.systolic}/${log.diastolic} lúc ${new Date(log.measuredAt).toLocaleString('vi-VN')}?`)) {
      this.store.deleteBpLog(log.id);
    }
  }

  protected loadPage(page: number): void {
    this.store.loadBpLogs({ page });
  }

  protected armLabel(arm: string): string {
    return arm === 'right' ? 'Tay phải' : 'Tay trái';
  }

  protected positionLabel(pos: string): string {
    const map: Record<string, string> = { sitting: 'Ngồi', lying: 'Nằm', standing: 'Đứng' };
    return map[pos] ?? pos;
  }
}
