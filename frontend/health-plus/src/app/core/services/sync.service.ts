import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { IndexedDbService, SyncQueueItem } from '../db/indexeddb.service';
import { OfflineService } from './offline.service';
import { ApiService } from './api.service';

export interface SyncOperation {
  operation: 'create' | 'update' | 'delete';
  entityType: string;
  entityId: string;
  payload: unknown;
  clientVersion: number;
  clientTimestamp: number;
}

export interface SyncChange {
  entityType: string;
  entityId: string;
  operation: string;
  data: unknown;
  serverTimestamp: string;
}

export interface PullSyncResponse {
  changes: SyncChange[];
  serverTimestamp: number;
}

@Injectable({ providedIn: 'root' })
export class SyncService {
  private readonly db = inject(IndexedDbService);
  private readonly http = inject(HttpClient);
  private readonly offline = inject(OfflineService);
  private readonly api = inject(ApiService);

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

  async pushToServer(operations: SyncOperation[]): Promise<void> {
    await firstValueFrom(this.api.post<void>('sync/push', { operations }));
  }

  async pullFromServer(since: number): Promise<PullSyncResponse> {
    return firstValueFrom(this.api.get<PullSyncResponse>('sync/pull', { since }));
  }

  private async replayRequest(item: SyncQueueItem): Promise<void> {
    try {
      const headers = new HttpHeaders({
        'Content-Type': 'application/json',
        Authorization: item.authToken,
      });

      await firstValueFrom(
        this.http.request(item.method, item.url, {
          headers,
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
