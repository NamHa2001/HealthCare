import { Injectable } from '@angular/core';
import Dexie, { Table } from 'dexie';

export interface SyncQueueItem {
  id?: number;
  method: 'POST' | 'PUT' | 'PATCH' | 'DELETE';
  url: string;
  body: unknown;
  authToken: string;
  createdAt: number;
  retryCount: number;
}

export interface CachedMeasurement {
  id: string;
  healthProfileId: string;
  measuredAt: string;
  weightKg: number | null;
  bmi: number | null;
  heartRateBpm: number | null;
  spo2Percent: number | null;
  bodyTemperature: number | null;
  bloodGlucose: number | null;
}

export interface CachedBpLog {
  id: string;
  healthProfileId: string;
  measuredAt: string;
  systolic: number;
  diastolic: number;
  pulse: number | null;
}

@Injectable({ providedIn: 'root' })
export class IndexedDbService extends Dexie {
  syncQueue!: Table<SyncQueueItem, number>;
  measurements!: Table<CachedMeasurement, string>;
  bpLogs!: Table<CachedBpLog, string>;

  constructor() {
    super('HealthPlusDB');
    this.version(1).stores({
      syncQueue: '++id, createdAt, method, url',
      measurements: 'id, healthProfileId, measuredAt',
      bpLogs: 'id, healthProfileId, measuredAt',
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
