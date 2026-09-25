import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { IndexedDbService, SyncQueueItem } from '../db/indexeddb.service';
import { OfflineService } from './offline.service';

/**
 * BUG-23: chỉ hàng đợi mutation offline (interceptor + processSyncQueue/replayRequest) thực sự
 * hoạt động. Giao thức sync 2 chiều thật qua Lamport clock (pushToServer/pullFromServer gọi
 * /sync/push, /sync/pull — backend đã dựng sẵn và có test) chưa từng được frontend gọi tới.
 * Sprint 7 Offline First vẫn "chưa bắt đầu" theo PROGRESS.md — đã xóa phần khung sườn dở dang
 * ở đây để tránh gây nhầm lẫn; xem SyncControllerTests.cs phía backend nếu làm tiếp sau này.
 */
@Injectable({ providedIn: 'root' })
export class SyncService {
  private readonly db = inject(IndexedDbService);
  private readonly http = inject(HttpClient);
  private readonly offline = inject(OfflineService);

  private syncing = false;

  constructor() {
    window.addEventListener('online', () => this.processSyncQueue());
  }

  async processSyncQueue(): Promise<void> {
    if (this.syncing || !this.offline.isOnline()) return;
    this.syncing = true;

    try {
      const items = await this.db.getPendingItems();
      for (const item of items) {
        await this.replayRequest(item);
      }
    } finally {
      this.syncing = false;
    }
  }

  private async replayRequest(item: SyncQueueItem): Promise<void> {
    try {
      // BUG-25: không cần tự set header Authorization — request này vẫn đi qua authInterceptor
      // như mọi request khác trong app, interceptor tự gắn access token mới nhất.
      await firstValueFrom(
        this.http.request(item.method, item.url, {
          headers: { 'Content-Type': 'application/json' },
          body: item.body ?? undefined,
        })
      );
      await this.db.dequeue(item.id!);
    } catch {
      if (item.retryCount >= 3) {
        await this.db.dequeue(item.id!);
      } else {
        await this.db.incrementRetry(item.id!);
      }
    }
  }
}
