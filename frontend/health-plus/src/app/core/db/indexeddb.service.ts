import { Injectable } from '@angular/core';
import Dexie, { Table } from 'dexie';

export interface SyncQueueItem {
  id?: number;
  method: 'POST' | 'PUT' | 'PATCH' | 'DELETE';
  url: string;
  body: unknown;
  createdAt: number;
  retryCount: number;
}

// BUG-23: bảng cache đọc offline (measurements/bpLogs) từng khai báo sẵn nhưng không component/store
// nào ghi hay đọc — dọn theo quyết định giữ nguyên "Offline First = chưa bắt đầu" (PROGRESS.md).
// Chỉ giữ syncQueue — phần hàng đợi mutation offline đang thực sự hoạt động.
@Injectable({ providedIn: 'root' })
export class IndexedDbService extends Dexie {
  syncQueue!: Table<SyncQueueItem, number>;

  constructor() {
    super('HealthPlusDB');
    this.version(1).stores({
      syncQueue: '++id, createdAt, method, url',
    });
  }

  async enqueue(item: Omit<SyncQueueItem, 'id' | 'createdAt' | 'retryCount'>): Promise<number> {
    return this.syncQueue.add({
      ...item,
      createdAt: Date.now(),
      retryCount: 0,
    });
  }

  async dequeue(id: number): Promise<void> {
    await this.syncQueue.delete(id);
  }

  async getPendingItems(): Promise<SyncQueueItem[]> {
    return this.syncQueue.orderBy('createdAt').toArray();
  }

  async incrementRetry(id: number): Promise<void> {
    await this.syncQueue
      .where('id').equals(id)
      .modify(item => { item.retryCount += 1; });
  }

  async clearSyncQueue(): Promise<void> {
    await this.syncQueue.clear();
  }
}
