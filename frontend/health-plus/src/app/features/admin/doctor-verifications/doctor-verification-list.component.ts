import { Component, OnInit, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { firstValueFrom } from 'rxjs';
import { ApiService } from '../../../core/services/api.service';
import {
  DoctorStatus,
  DoctorVerificationDetail,
  DoctorVerificationItem,
} from '../../doctor/models/doctor.model';

@Component({
  selector: 'app-doctor-verification-list',
  standalone: true,
  imports: [DatePipe, MatButtonModule, MatIconModule, MatProgressSpinnerModule, MatTooltipModule],
  templateUrl: './doctor-verification-list.component.html',
})
export class DoctorVerificationListComponent implements OnInit {
  private readonly api = inject(ApiService);
  private readonly snackBar = inject(MatSnackBar);

  protected readonly loading = signal(true);
  protected readonly items = signal<DoctorVerificationItem[]>([]);
  protected readonly filter = signal<DoctorStatus | null>('pending');
  protected readonly detail = signal<DoctorVerificationDetail | null>(null);
  protected readonly detailLoading = signal(false);
  protected readonly acting = signal(false);

  protected readonly filters: { label: string; value: DoctorStatus | null }[] = [
    { label: 'Chờ duyệt', value: 'pending' },
    { label: 'Đã duyệt', value: 'approved' },
    { label: 'Từ chối', value: 'rejected' },
    { label: 'Tạm ngưng', value: 'suspended' },
    { label: 'Tất cả', value: null },
  ];

  ngOnInit(): void {
    this.load();
  }

  protected async load(): Promise<void> {
    this.loading.set(true);
    try {
      const params: Record<string, string> = {};
      const f = this.filter();
      if (f) params['status'] = f;
      this.items.set(await firstValueFrom(
        this.api.get<DoctorVerificationItem[]>('Admin/doctor-verifications', params)));
    } finally {
      this.loading.set(false);
    }
  }

  protected applyFilter(status: DoctorStatus | null): void {
    this.filter.set(status);
    this.detail.set(null);
    this.load();
  }

  protected async openDetail(item: DoctorVerificationItem): Promise<void> {
    this.detailLoading.set(true);
    try {
      this.detail.set(await firstValueFrom(
        this.api.get<DoctorVerificationDetail>(`Admin/doctor-verifications/${item.id}`)));
    } finally {
      this.detailLoading.set(false);
    }
  }

  protected async approve(): Promise<void> {
    const d = this.detail();
    if (!d || !confirm(`Duyệt hồ sơ bác sĩ của ${d.fullName}?`)) return;
    this.acting.set(true);
    try {
      await firstValueFrom(this.api.post(`Admin/doctor-verifications/${d.id}/approve`, {}));
      this.snackBar.open(`Đã duyệt BS. ${d.fullName}.`, undefined, { duration: 3000 });
      this.detail.set(null);
      await this.load();
    } finally {
      this.acting.set(false);
    }
  }

  protected async reject(): Promise<void> {
    const d = this.detail();
    if (!d) return;
    const reason = prompt('Lý do từ chối (bắt buộc):');
    if (!reason?.trim()) return;
    this.acting.set(true);
    try {
      await firstValueFrom(this.api.post(`Admin/doctor-verifications/${d.id}/reject`, { reason }));
      this.snackBar.open('Đã từ chối hồ sơ.', undefined, { duration: 3000 });
      this.detail.set(null);
      await this.load();
    } finally {
      this.acting.set(false);
    }
  }

  protected async suspend(): Promise<void> {
    const d = this.detail();
    if (!d) return;
    const reason = prompt('Lý do tạm ngưng (bắt buộc):');
    if (!reason?.trim()) return;
    this.acting.set(true);
    try {
      await firstValueFrom(this.api.post(`Admin/doctor-verifications/${d.id}/suspend`, { reason }));
      this.snackBar.open('Đã tạm ngưng tài khoản bác sĩ.', undefined, { duration: 3000 });
      this.detail.set(null);
      await this.load();
    } finally {
      this.acting.set(false);
    }
  }

  protected statusInfo(status: DoctorStatus): { label: string; cls: string } {
    switch (status) {
      case 'pending': return { label: 'Chờ duyệt', cls: 'badge-warning' };
      case 'approved': return { label: 'Đã duyệt', cls: 'badge-success' };
      case 'suspended': return { label: 'Tạm ngưng', cls: 'badge-danger' };
      default: return { label: 'Từ chối', cls: 'badge-danger' };
    }
  }
}
