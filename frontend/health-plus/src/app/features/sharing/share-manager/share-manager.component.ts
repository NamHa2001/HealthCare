import { Component, OnInit, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatTooltipModule } from '@angular/material/tooltip';
import QRCode from 'qrcode';
import { SharingStore } from '../sharing.store';
import { SCOPE_LABELS, ShareGrant, ShareScope } from '../models/share.model';

@Component({
  selector: 'app-share-manager',
  standalone: true,
  imports: [
    DatePipe,
    MatButtonModule, MatIconModule, MatProgressSpinnerModule, MatTooltipModule,
  ],
  templateUrl: './share-manager.component.html',
})
export class ShareManagerComponent implements OnInit {
  protected readonly store = inject(SharingStore);
  private readonly snackBar = inject(MatSnackBar);

  protected readonly scopeOptions = Object.entries(SCOPE_LABELS) as [ShareScope, string][];
  protected readonly ttlOptions = [
    { label: '1 giờ', value: 1 },
    { label: '24 giờ', value: 24 },
    { label: '7 ngày', value: 168 },
  ];

  protected readonly selectedScopes = signal<Set<ShareScope>>(
    new Set(['profile', 'measurements', 'blood_pressure']));
  protected readonly selectedTtl = signal(24);
  protected readonly creating = signal(false);
  protected readonly qrDataUrl = signal<string | null>(null);

  ngOnInit(): void {
    this.store.clearLastCreated();
    this.store.load();
  }

  protected toggleScope(scope: ShareScope): void {
    const next = new Set(this.selectedScopes());
    if (next.has(scope)) next.delete(scope);
    else next.add(scope);
    this.selectedScopes.set(next);
  }

  protected shareUrl(token: string): string {
    return `${location.origin}/shared/${token}`;
  }

  protected async create(): Promise<void> {
    if (this.selectedScopes().size === 0) {
      this.snackBar.open('Chọn ít nhất một phạm vi dữ liệu.', 'Đóng', { duration: 3000 });
      return;
    }
    this.creating.set(true);
    try {
      const result = await this.store.create({
        scope: [...this.selectedScopes()],
        ttlHours: this.selectedTtl(),
      });
      this.qrDataUrl.set(await QRCode.toDataURL(this.shareUrl(result.token), { width: 220 }));
    } catch {
      this.snackBar.open('Không tạo được liên kết. Hồ sơ chỉ được tối đa 3 liên kết đang hoạt động.', 'Đóng', { duration: 5000 });
    } finally {
      this.creating.set(false);
    }
  }

  protected async copyLink(token: string): Promise<void> {
    await navigator.clipboard.writeText(this.shareUrl(token));
    this.snackBar.open('Đã sao chép liên kết.', undefined, { duration: 2000 });
  }

  protected async revoke(grant: ShareGrant): Promise<void> {
    if (!confirm('Thu hồi liên kết này? Người đang giữ link sẽ mất quyền xem ngay lập tức.')) return;
    await this.store.revoke(grant.id);
    if (this.store.lastCreated() === null) this.qrDataUrl.set(null);
    this.snackBar.open('Đã thu hồi liên kết.', undefined, { duration: 2000 });
  }

  protected scopeLabel(scope: ShareScope): string {
    return SCOPE_LABELS[scope] ?? scope;
  }

  protected statusOf(g: ShareGrant): { label: string; cls: string } {
    if (g.revokedAt) return { label: 'Đã thu hồi', cls: 'badge-neutral' };
    if (!g.isActive) return { label: 'Hết hạn', cls: 'badge-neutral' };
    return { label: 'Đang hoạt động', cls: 'badge-success' };
  }
}
