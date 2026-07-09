import { Component, OnInit, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { firstValueFrom } from 'rxjs';
import { ApiService } from '../../../core/services/api.service';
import {
  SCOPE_LABELS,
  SharedBloodPressure,
  SharedMeasurement,
  SharedMedication,
  SharedMeta,
  SharedProfile,
  SharedVaccine,
  SharedVisit,
  ShareScope,
} from '../models/share.model';

/**
 * Trang public — người được chia sẻ (bác sĩ) xem hồ sơ read-only qua token,
 * KHÔNG cần đăng nhập. Mọi lỗi (token sai/hết hạn/thu hồi) đều hiển thị như nhau.
 */
@Component({
  selector: 'app-shared-viewer',
  standalone: true,
  imports: [DatePipe, MatIconModule, MatProgressSpinnerModule],
  templateUrl: './shared-viewer.component.html',
})
export class SharedViewerComponent implements OnInit {
  private readonly api = inject(ApiService);
  private readonly route = inject(ActivatedRoute);

  protected readonly loading = signal(true);
  protected readonly notFound = signal(false);
  protected readonly meta = signal<SharedMeta | null>(null);

  protected readonly profile = signal<SharedProfile | null>(null);
  protected readonly measurements = signal<SharedMeasurement[]>([]);
  protected readonly bloodPressure = signal<SharedBloodPressure[]>([]);
  protected readonly visits = signal<SharedVisit[]>([]);
  protected readonly medications = signal<SharedMedication[]>([]);
  protected readonly vaccines = signal<SharedVaccine[]>([]);

  private token = '';

  async ngOnInit(): Promise<void> {
    this.token = this.route.snapshot.paramMap.get('token') ?? '';
    try {
      const meta = await firstValueFrom(this.api.get<SharedMeta>(`Shared/${this.token}`));
      this.meta.set(meta);
      await this.loadSections(meta.scope);
    } catch {
      this.notFound.set(true);
    } finally {
      this.loading.set(false);
    }
  }

  private async loadSections(scopes: ShareScope[]): Promise<void> {
    const jobs: Promise<unknown>[] = [];
    const load = <T>(path: string, sink: (v: T) => void) =>
      jobs.push(firstValueFrom(this.api.get<T>(path)).then(sink).catch(() => undefined));

    if (scopes.includes('profile'))
      load<SharedProfile>(`Shared/${this.token}/profile`, v => this.profile.set(v));
    if (scopes.includes('measurements'))
      load<SharedMeasurement[]>(`Shared/${this.token}/measurements`, v => this.measurements.set(v));
    if (scopes.includes('blood_pressure'))
      load<SharedBloodPressure[]>(`Shared/${this.token}/blood-pressure`, v => this.bloodPressure.set(v));
    if (scopes.includes('visits'))
      load<SharedVisit[]>(`Shared/${this.token}/medical-visits`, v => this.visits.set(v));
    if (scopes.includes('medications'))
      load<SharedMedication[]>(`Shared/${this.token}/medications`, v => this.medications.set(v));
    if (scopes.includes('vaccines'))
      load<SharedVaccine[]>(`Shared/${this.token}/vaccines`, v => this.vaccines.set(v));

    await Promise.all(jobs);
  }

  protected has(scope: ShareScope): boolean {
    return this.meta()?.scope.includes(scope) ?? false;
  }

  protected scopeLabel(scope: ShareScope): string {
    return SCOPE_LABELS[scope] ?? scope;
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
