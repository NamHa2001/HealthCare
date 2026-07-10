import { Component, OnInit, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatTabsModule } from '@angular/material/tabs';
import { firstValueFrom } from 'rxjs';
import { ApiService } from '../../../core/services/api.service';
import {
  SharedBloodPressure,
  SharedMeasurement,
  SharedMedication,
  SharedVaccine,
  SharedVisit,
} from '../../sharing/models/share.model';
import { PatientAlert, PatientSummary } from '../models/doctor.model';

/** Bác sĩ xem chi tiết 1 bệnh nhân — tabs render theo đúng phạm vi consent. */
@Component({
  selector: 'app-patient-detail',
  standalone: true,
  imports: [
    DatePipe, RouterLink,
    MatButtonModule, MatIconModule, MatProgressSpinnerModule, MatTabsModule,
  ],
  templateUrl: './patient-detail.component.html',
})
export class PatientDetailComponent implements OnInit {
  private readonly api = inject(ApiService);
  private readonly route = inject(ActivatedRoute);
  private readonly snackBar = inject(MatSnackBar);

  protected readonly loading = signal(true);
  protected readonly forbidden = signal(false);
  protected readonly summary = signal<PatientSummary | null>(null);

  protected readonly measurements = signal<SharedMeasurement[]>([]);
  protected readonly bloodPressure = signal<SharedBloodPressure[]>([]);
  protected readonly visits = signal<SharedVisit[]>([]);
  protected readonly medications = signal<SharedMedication[]>([]);
  protected readonly vaccines = signal<SharedVaccine[]>([]);

  private profileId = '';

  async ngOnInit(): Promise<void> {
    this.profileId = this.route.snapshot.paramMap.get('profileId') ?? '';
    try {
      const summary = await firstValueFrom(
        this.api.get<PatientSummary>(`Doctor/patients/${this.profileId}/summary`));
      this.summary.set(summary);
      await this.loadSections(summary.consentScope);
    } catch {
      this.forbidden.set(true);
    } finally {
      this.loading.set(false);
    }
  }

  private async loadSections(scopes: string[]): Promise<void> {
    const jobs: Promise<unknown>[] = [];
    const load = <T>(path: string, sink: (v: T) => void) =>
      jobs.push(firstValueFrom(this.api.get<T>(path)).then(sink).catch(() => undefined));

    const base = `Doctor/patients/${this.profileId}`;
    if (scopes.includes('measurements'))
      load<SharedMeasurement[]>(`${base}/measurements`, v => this.measurements.set(v));
    if (scopes.includes('blood_pressure'))
      load<SharedBloodPressure[]>(`${base}/blood-pressure`, v => this.bloodPressure.set(v));
    if (scopes.includes('visits'))
      load<SharedVisit[]>(`${base}/medical-visits`, v => this.visits.set(v));
    if (scopes.includes('medications'))
      load<SharedMedication[]>(`${base}/medications`, v => this.medications.set(v));
    if (scopes.includes('vaccines'))
      load<SharedVaccine[]>(`${base}/vaccines`, v => this.vaccines.set(v));

    await Promise.all(jobs);
  }

  protected has(scope: string): boolean {
    return this.summary()?.consentScope.includes(scope) ?? false;
  }

  protected async acknowledge(alert: PatientAlert): Promise<void> {
    await firstValueFrom(this.api.post(
      `Doctor/patients/${this.profileId}/alerts/${alert.id}/acknowledge`, {}));
    this.summary.update(s => s === null ? s : {
      ...s,
      activeAlerts: s.activeAlerts.filter(a => a.id !== alert.id),
    });
    this.snackBar.open('Đã đánh dấu đã xem xét.', undefined, { duration: 2000 });
  }

  protected vaccineStatusLabel(status: string): string {
    switch (status) {
      case 'overdue': return 'Quá hạn';
      case 'upcoming': return 'Sắp tới';
      case 'scheduled': return 'Đã lên lịch';
      default: return 'Hoàn thành';
    }
  }
}
