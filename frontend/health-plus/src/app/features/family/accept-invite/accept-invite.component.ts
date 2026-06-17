import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { firstValueFrom } from 'rxjs';
import { ApiService } from '../../../core/services/api.service';

@Component({
  selector: 'app-accept-invite',
  standalone: true,
  imports: [MatButtonModule, MatIconModule, MatProgressSpinnerModule],
  template: `
    <div class="min-h-screen flex items-center justify-center bg-gray-50 p-4">
      <div class="sb-card p-8 max-w-md w-full text-center space-y-4">

        @if (loading()) {
          <mat-spinner diameter="48" class="mx-auto" />
          <p class="text-[#858796]">Đang xử lý lời mời...</p>
        }

        @if (!loading() && success()) {
          <mat-icon class="text-[#1cc88a] text-6xl">check_circle</mat-icon>
          <h2 class="text-xl font-bold text-[#3a3b45]">Tham gia thành công!</h2>
          <p class="text-[#858796]">Bạn đã gia nhập nhóm gia đình. Hãy vào trang Gia đình để xem hồ sơ.</p>
          <button mat-raised-button color="primary" (click)="goToFamily()">
            <mat-icon>group</mat-icon>
            Xem nhóm gia đình
          </button>
        }

        @if (!loading() && !success() && error()) {
          <mat-icon class="text-[#e74a3b] text-6xl">error_outline</mat-icon>
          <h2 class="text-xl font-bold text-[#3a3b45]">Không thể chấp nhận lời mời</h2>
          <p class="text-[#858796]">{{ error() }}</p>
          <button mat-stroked-button (click)="router.navigate(['/family'])">Về trang gia đình</button>
        }

      </div>
    </div>
  `,
})
export class AcceptInviteComponent implements OnInit {
  protected readonly loading = signal(true);
  protected readonly success = signal(false);
  protected readonly error   = signal<string | null>(null);

  protected readonly router = inject(Router);
  private  readonly route  = inject(ActivatedRoute);
  private  readonly api    = inject(ApiService);

  async ngOnInit(): Promise<void> {
    const token = this.route.snapshot.queryParamMap.get('token');
    if (!token) {
      this.error.set('Liên kết không hợp lệ.');
      this.loading.set(false);
      return;
    }

    try {
      await firstValueFrom(this.api.post<void>('family/invite/accept', { token }));
      this.success.set(true);
    } catch (err: any) {
      const msg = err?.error?.error?.message ?? 'Lời mời đã hết hạn hoặc không hợp lệ.';
      this.error.set(msg);
    } finally {
      this.loading.set(false);
    }
  }

  protected goToFamily(): void {
    this.router.navigate(['/family']);
  }
}
