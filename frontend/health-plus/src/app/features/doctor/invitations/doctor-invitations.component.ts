import { Component, OnInit, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { firstValueFrom } from 'rxjs';
import { ApiService } from '../../../core/services/api.service';
import { SCOPE_LABELS, ShareScope } from '../../sharing/models/share.model';
import { LinkStatus, PatientLink } from '../models/doctor.model';

/** Phía bác sĩ: mời bệnh nhân qua email + quản lý liên kết đã gửi. */
@Component({
  selector: 'app-doctor-invitations',
  standalone: true,
  imports: [
    DatePipe, FormsModule,
    MatButtonModule, MatIconModule, MatProgressSpinnerModule, MatTooltipModule,
  ],
  templateUrl: './doctor-invitations.component.html',
})
export class DoctorInvitationsComponent implements OnInit {
  private readonly api = inject(ApiService);
  private readonly snackBar = inject(MatSnackBar);

  protected readonly loading = signal(true);
  protected readonly links = signal<PatientLink[]>([]);
  protected readonly acting = signal(false);
  protected inviteEmail = '';

  ngOnInit(): void {
    this.load();
  }

  protected async load(): Promise<void> {
    this.loading.set(true);
    try {
      this.links.set(await firstValueFrom(this.api.get<PatientLink[]>('Doctor/invitations')));
    } finally {
      this.loading.set(false);
    }
  }

  protected async invite(): Promise<void> {
    const email = this.inviteEmail.trim();
    if (!email) return;
    this.acting.set(true);
    try {
      await firstValueFrom(this.api.post('Doctor/invitations', { patientEmail: email }));
      this.snackBar.open(`Đã gửi lời mời tới ${email}.`, undefined, { duration: 3000 });
      this.inviteEmail = '';
      await this.load();
    } catch {
      this.snackBar.open(
        'Không gửi được: bệnh nhân chưa có tài khoản Health+, hoặc đã có liên kết.', 'Đóng', { duration: 5000 });
    } finally {
      this.acting.set(false);
    }
  }

  /** Bác sĩ chấp nhận lời mời do bệnh nhân khởi tạo (consent đã có sẵn). */
  protected async acceptPatientInvite(link: PatientLink): Promise<void> {
    if (!confirm(`Nhận theo dõi bệnh nhân ${link.patientName}?`)) return;
    this.acting.set(true);
    try {
      await firstValueFrom(this.api.post(`DoctorLinks/${link.id}/accept`, {}));
      this.snackBar.open(`Đã liên kết với bệnh nhân ${link.patientName}.`, undefined, { duration: 3000 });
      await this.load();
    } finally {
      this.acting.set(false);
    }
  }

  protected async rejectPatientInvite(link: PatientLink): Promise<void> {
    if (!confirm(`Từ chối lời mời của ${link.patientName}?`)) return;
    this.acting.set(true);
    try {
      await firstValueFrom(this.api.post(`DoctorLinks/${link.id}/reject`, {}));
      await this.load();
    } finally {
      this.acting.set(false);
    }
  }

  protected async revoke(link: PatientLink): Promise<void> {
    const msg = link.status === 'active'
      ? `Ngừng theo dõi bệnh nhân ${link.patientName}?`
      : `Hủy lời mời tới ${link.patientName}?`;
    if (!confirm(msg)) return;
    this.acting.set(true);
    try {
      await firstValueFrom(this.api.delete(`Doctor/invitations/${link.id}`));
      await this.load();
    } finally {
      this.acting.set(false);
    }
  }

  protected scopeLabel(scope: string): string {
    return SCOPE_LABELS[scope as ShareScope] ?? scope;
  }

  protected statusInfo(status: LinkStatus): { label: string; cls: string } {
    switch (status) {
      case 'pending': return { label: 'Chờ bệnh nhân đồng ý', cls: 'badge-warning' };
      case 'active': return { label: 'Đang theo dõi', cls: 'badge-success' };
      case 'rejected': return { label: 'Bị từ chối', cls: 'badge-neutral' };
      default: return { label: 'Đã thu hồi', cls: 'badge-neutral' };
    }
  }
}
