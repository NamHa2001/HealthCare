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
import { DoctorLink, DoctorSearchResult, LinkStatus } from '../models/doctor.model';

/**
 * Phía bệnh nhân: lời mời từ bác sĩ (chấp nhận với phạm vi tự chọn / từ chối),
 * liên kết đang hoạt động (xem consent, thu hồi), và mời bác sĩ từ danh bạ.
 */
@Component({
  selector: 'app-doctor-links',
  standalone: true,
  imports: [
    DatePipe, FormsModule,
    MatButtonModule, MatIconModule, MatProgressSpinnerModule, MatTooltipModule,
  ],
  templateUrl: './doctor-links.component.html',
})
export class DoctorLinksComponent implements OnInit {
  private readonly api = inject(ApiService);
  private readonly snackBar = inject(MatSnackBar);

  protected readonly scopeOptions = Object.entries(SCOPE_LABELS) as [ShareScope, string][];

  protected readonly loading = signal(true);
  protected readonly links = signal<DoctorLink[]>([]);
  protected readonly acting = signal(false);

  /** Link đang mở panel chấp nhận + scope đã tick */
  protected readonly acceptingId = signal<string | null>(null);
  protected readonly acceptScopes = signal<Set<ShareScope>>(new Set(['measurements', 'blood_pressure']));

  /** Tìm bác sĩ */
  protected searchTerm = '';
  protected readonly searchResults = signal<DoctorSearchResult[]>([]);
  protected readonly searching = signal(false);

  /** Bác sĩ đang được chọn để mời + scope cho lời mời */
  protected readonly invitingDoctorId = signal<string | null>(null);
  protected readonly inviteScopes = signal<Set<ShareScope>>(new Set(['measurements', 'blood_pressure']));

  ngOnInit(): void {
    this.load();
  }

  protected async load(): Promise<void> {
    this.loading.set(true);
    try {
      this.links.set(await firstValueFrom(this.api.get<DoctorLink[]>('DoctorLinks')));
    } finally {
      this.loading.set(false);
    }
  }

  protected pending(): DoctorLink[] {
    return this.links().filter(l => l.status === 'pending');
  }

  protected others(): DoctorLink[] {
    return this.links().filter(l => l.status !== 'pending');
  }

  protected toggleAcceptPanel(link: DoctorLink): void {
    this.acceptingId.set(this.acceptingId() === link.id ? null : link.id);
  }

  protected toggleScope(scope: ShareScope): void {
    const next = new Set(this.acceptScopes());
    if (next.has(scope)) next.delete(scope);
    else next.add(scope);
    this.acceptScopes.set(next);
  }

  protected async accept(link: DoctorLink): Promise<void> {
    if (this.acceptScopes().size === 0) {
      this.snackBar.open('Chọn ít nhất một phạm vi dữ liệu.', 'Đóng', { duration: 3000 });
      return;
    }
    this.acting.set(true);
    try {
      await firstValueFrom(this.api.post(`DoctorLinks/${link.id}/accept`, {
        consentScope: [...this.acceptScopes()],
      }));
      this.snackBar.open(`Đã liên kết với BS. ${link.doctorName}.`, undefined, { duration: 3000 });
      this.acceptingId.set(null);
      await this.load();
    } finally {
      this.acting.set(false);
    }
  }

  protected async reject(link: DoctorLink): Promise<void> {
    if (!confirm(`Từ chối lời mời của BS. ${link.doctorName}?`)) return;
    this.acting.set(true);
    try {
      await firstValueFrom(this.api.post(`DoctorLinks/${link.id}/reject`, {}));
      await this.load();
    } finally {
      this.acting.set(false);
    }
  }

  protected async revoke(link: DoctorLink): Promise<void> {
    if (!confirm(`Thu hồi quyền xem của BS. ${link.doctorName}? Bác sĩ sẽ mất quyền ngay lập tức.`)) return;
    this.acting.set(true);
    try {
      await firstValueFrom(this.api.delete(`DoctorLinks/${link.id}`));
      this.snackBar.open('Đã thu hồi liên kết.', undefined, { duration: 3000 });
      await this.load();
    } finally {
      this.acting.set(false);
    }
  }

  protected async search(): Promise<void> {
    this.searching.set(true);
    try {
      this.searchResults.set(await firstValueFrom(
        this.api.get<DoctorSearchResult[]>('Doctor/search', { q: this.searchTerm })));
    } finally {
      this.searching.set(false);
    }
  }

  protected toggleInvitePanel(doctor: DoctorSearchResult): void {
    this.invitingDoctorId.set(this.invitingDoctorId() === doctor.userId ? null : doctor.userId);
  }

  protected toggleInviteScope(scope: ShareScope): void {
    const next = new Set(this.inviteScopes());
    if (next.has(scope)) next.delete(scope);
    else next.add(scope);
    this.inviteScopes.set(next);
  }

  protected async inviteDoctor(doctor: DoctorSearchResult): Promise<void> {
    if (this.inviteScopes().size === 0) {
      this.snackBar.open('Chọn ít nhất một phạm vi dữ liệu.', 'Đóng', { duration: 3000 });
      return;
    }
    this.acting.set(true);
    try {
      const profile = await firstValueFrom(this.api.get<{ id: string }>('HealthProfiles/me'));
      await firstValueFrom(this.api.post('DoctorLinks', {
        doctorUserId: doctor.userId,
        healthProfileId: profile.id,
        consentScope: [...this.inviteScopes()],
      }));
      this.snackBar.open(`Đã gửi lời mời tới BS. ${doctor.fullName}.`, undefined, { duration: 3000 });
      this.searchResults.set([]);
      this.searchTerm = '';
      this.invitingDoctorId.set(null);
      await this.load();
    } catch {
      this.snackBar.open('Không gửi được lời mời — có thể đã tồn tại liên kết.', 'Đóng', { duration: 4000 });
    } finally {
      this.acting.set(false);
    }
  }

  protected scopeLabel(scope: string): string {
    return SCOPE_LABELS[scope as ShareScope] ?? scope;
  }

  protected statusInfo(status: LinkStatus): { label: string; cls: string } {
    switch (status) {
      case 'pending': return { label: 'Chờ phản hồi', cls: 'badge-warning' };
      case 'active': return { label: 'Đang liên kết', cls: 'badge-success' };
      case 'rejected': return { label: 'Đã từ chối', cls: 'badge-neutral' };
      default: return { label: 'Đã thu hồi', cls: 'badge-neutral' };
    }
  }
}
