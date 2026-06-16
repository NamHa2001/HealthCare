import { Component, OnInit, computed, inject } from '@angular/core';
import { DatePipe, DecimalPipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { AuthStore } from '../../core/auth/auth.store';
import { HealthRecordsStore } from '../health-records/health-records.store';
import { MedicalHistoryStore } from '../medical-history/medical-history.store';
import { BmiClassPipe } from '../../shared/pipes/bmi-class.pipe';
import { BpClassPipe } from '../../shared/pipes/bp-class.pipe';

interface HealthAlert {
  icon: string;
  text: string;
  level: 'warning' | 'danger';
}

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    DatePipe, DecimalPipe, RouterLink,
    MatIconModule, MatButtonModule, MatProgressSpinnerModule,
    BmiClassPipe, BpClassPipe,
  ],
  templateUrl: './dashboard.component.html',
})
export class DashboardComponent implements OnInit {
  protected readonly auth = inject(AuthStore);
  protected readonly health = inject(HealthRecordsStore);
  protected readonly medical = inject(MedicalHistoryStore);

  // Lời chào theo thời điểm trong ngày
  protected readonly greeting = computed(() => {
    const h = new Date().getHours();
    if (h < 11) return 'Chào buổi sáng';
    if (h < 14) return 'Chào buổi trưa';
    if (h < 18) return 'Chào buổi chiều';
    return 'Chào buổi tối';
  });

  protected readonly firstName = computed(() => this.auth.currentUser()?.firstName ?? '');

  // Bản ghi mới nhất (store sắp xếp giảm dần theo thời gian đo)
  protected readonly latestMeasurement = computed(() => this.health.measurements()[0] ?? null);
  protected readonly latestBp = computed(() => this.health.bpLogs()[0] ?? null);

  protected readonly latestBmi = computed(() => this.latestMeasurement()?.bmi ?? null);

  // Chỉ số sức khỏe ước tính (0-100) từ BMI + huyết áp — dữ liệu sẵn có.
  protected readonly healthScore = computed<number | null>(() => {
    const bmi = this.latestBmi();
    const bp = this.latestBp();
    if (bmi == null && bp == null) return null;

    let score = 100;
    if (bmi != null) {
      if (bmi < 18.5) score -= 15;
      else if (bmi < 23) score -= 0;
      else if (bmi < 25) score -= 12;
      else score -= 25;
    }
    if (bp != null) {
      if (bp.systolic >= 140 || bp.diastolic >= 90) score -= 25;
      else if (bp.systolic >= 130 || bp.diastolic >= 80) score -= 12;
      else if (bp.systolic >= 120) score -= 5;
    }
    return Math.max(0, Math.min(100, score));
  });

  protected readonly scoreColor = computed(() => {
    const s = this.healthScore();
    if (s == null) return 'text-slate-400';
    if (s >= 80) return 'text-green-600';
    if (s >= 60) return 'text-amber-500';
    return 'text-red-600';
  });

  // Cảnh báo dựa trên ngưỡng SRS §3.2
  protected readonly alerts = computed<HealthAlert[]>(() => {
    const list: HealthAlert[] = [];
    const m = this.latestMeasurement();
    const bp = this.latestBp();

    if (bp) {
      if (bp.systolic >= 140 || bp.diastolic >= 90)
        list.push({ icon: 'monitor_heart', text: `Huyết áp cao: ${bp.systolic}/${bp.diastolic} mmHg`, level: 'danger' });
      else if (bp.systolic >= 130 || bp.diastolic >= 80)
        list.push({ icon: 'monitor_heart', text: `Tiền tăng huyết áp: ${bp.systolic}/${bp.diastolic} mmHg`, level: 'warning' });
    }
    if (m) {
      if (m.bmi != null && m.bmi >= 25)
        list.push({ icon: 'scale', text: `BMI cao: ${m.bmi.toFixed(1)} (béo phì)`, level: 'danger' });
      else if (m.bmi != null && m.bmi >= 23)
        list.push({ icon: 'scale', text: `BMI hơi cao: ${m.bmi.toFixed(1)} (thừa cân)`, level: 'warning' });
      if (m.spo2Percent != null && m.spo2Percent < 95)
        list.push({ icon: 'air', text: `SpO₂ thấp: ${m.spo2Percent}%`, level: 'danger' });
      if (m.heartRateBpm != null && (m.heartRateBpm < 60 || m.heartRateBpm > 100))
        list.push({ icon: 'cardiology', text: `Nhịp tim bất thường: ${m.heartRateBpm} bpm`, level: 'warning' });
      if (m.bodyTemperature != null && m.bodyTemperature > 37.5)
        list.push({ icon: 'thermostat', text: `Sốt: ${m.bodyTemperature}°C`, level: 'warning' });
    }
    return list;
  });

  ngOnInit(): void {
    this.health.loadProfile();
    this.health.loadMeasurements({ page: 1, pageSize: 5 });
    this.health.loadBpLogs({ page: 1, pageSize: 5 });
    this.medical.loadVisits({ page: 1, pageSize: 5 });
  }
}
