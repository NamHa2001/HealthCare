import { Component, Input } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { HealthSummary } from '../../../analytics/models/analytics.model';

@Component({
  selector: 'app-stats-summary',
  standalone: true,
  imports: [DecimalPipe, RouterLink, MatIconModule],
  template: `
    <div class="grid grid-cols-2 sm:grid-cols-3 gap-3">

      <!-- Nhắc lịch sắp tới -->
      <a routerLink="/reminders" class="sb-card px-4 py-3 flex items-center gap-3 hover:shadow-md transition-shadow">
        <div class="w-10 h-10 rounded-full bg-[#f6c23e]/10 flex items-center justify-center shrink-0">
          <mat-icon class="text-[#f6c23e]" style="font-size:1.2rem;width:1.2rem;height:1.2rem">notifications</mat-icon>
        </div>
        <div class="min-w-0">
          <p class="text-lg font-bold text-[#5a5c69]">{{ summary?.upcomingRemindersCount ?? 0 }}</p>
          <p class="text-xs text-[#858796] truncate">Nhắc lịch 7 ngày</p>
        </div>
      </a>

      <!-- Cảnh báo chưa đọc -->
      <a routerLink="/health-records" class="sb-card px-4 py-3 flex items-center gap-3 hover:shadow-md transition-shadow">
        <div class="w-10 h-10 rounded-full bg-[#e74a3b]/10 flex items-center justify-center shrink-0">
          <mat-icon class="text-[#e74a3b]" style="font-size:1.2rem;width:1.2rem;height:1.2rem">warning</mat-icon>
        </div>
        <div class="min-w-0">
          <p class="text-lg font-bold text-[#5a5c69]">{{ summary?.activeAlertsCount ?? 0 }}</p>
          <p class="text-xs text-[#858796] truncate">Cảnh báo chưa đọc</p>
        </div>
      </a>

      <!-- Mũi tiêm đã thực hiện -->
      <a routerLink="/vaccines" class="sb-card px-4 py-3 flex items-center gap-3 hover:shadow-md transition-shadow">
        <div class="w-10 h-10 rounded-full bg-[#36b9cc]/10 flex items-center justify-center shrink-0">
          <mat-icon class="text-[#36b9cc]" style="font-size:1.2rem;width:1.2rem;height:1.2rem">vaccines</mat-icon>
        </div>
        <div class="min-w-0">
          <p class="text-lg font-bold text-[#5a5c69]">{{ summary?.vaccinesCompleted ?? 0 }}</p>
          <p class="text-xs text-[#858796] truncate">Mũi đã tiêm</p>
        </div>
      </a>

      <!-- Cân nặng -->
      <div class="sb-card px-4 py-3 flex items-center gap-3">
        <div class="w-10 h-10 rounded-full bg-[#1cc88a]/10 flex items-center justify-center shrink-0">
          <mat-icon class="text-[#1cc88a]" style="font-size:1.2rem;width:1.2rem;height:1.2rem">monitor_weight</mat-icon>
        </div>
        <div class="min-w-0">
          <p class="text-lg font-bold text-[#5a5c69]">
            {{ summary?.latestWeightKg != null ? (summary!.latestWeightKg | number:'1.1-1') + ' kg' : '—' }}
          </p>
          <p class="text-xs text-[#858796]">Cân nặng</p>
        </div>
      </div>

      <!-- Glucose -->
      <div class="sb-card px-4 py-3 flex items-center gap-3">
        <div class="w-10 h-10 rounded-full bg-[#4e73df]/10 flex items-center justify-center shrink-0">
          <mat-icon class="text-[#4e73df]" style="font-size:1.2rem;width:1.2rem;height:1.2rem">water_drop</mat-icon>
        </div>
        <div class="min-w-0">
          <p class="text-lg font-bold text-[#5a5c69]">
            {{ summary?.latestGlucose != null ? (summary!.latestGlucose | number:'1.1-1') + ' mmol/L' : '—' }}
          </p>
          <p class="text-xs text-[#858796]">Đường huyết</p>
        </div>
      </div>

      <!-- Tuân thủ thuốc -->
      <a routerLink="/medications" class="sb-card px-4 py-3 flex items-center gap-3 hover:shadow-md transition-shadow">
        <div class="w-10 h-10 rounded-full bg-[#f6c23e]/10 flex items-center justify-center shrink-0">
          <mat-icon class="text-[#f6c23e]" style="font-size:1.2rem;width:1.2rem;height:1.2rem">medication</mat-icon>
        </div>
        <div class="min-w-0">
          <p class="text-lg font-bold text-[#5a5c69]">
            {{ summary?.medicationComplianceRate != null ? (summary!.medicationComplianceRate | number:'1.0-0') + '%' : '—' }}
          </p>
          <p class="text-xs text-[#858796] truncate">Tuân thủ thuốc (7 ngày)</p>
        </div>
      </a>

    </div>
  `,
})
export class StatsSummaryComponent {
  @Input() summary: HealthSummary | null = null;
}
