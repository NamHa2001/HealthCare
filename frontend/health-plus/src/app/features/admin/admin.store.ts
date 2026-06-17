import { inject } from '@angular/core';
import { patchState, signalStore, withMethods, withState } from '@ngrx/signals';
import { firstValueFrom } from 'rxjs';
import { ApiService } from '../../core/services/api.service';
import { PagedResult } from '../../core/models/paged-result.model';
import { AdminUser, AuditLogItem, SystemStats } from './models/admin.model';

interface AdminState {
  stats: SystemStats | null;
  users: AdminUser[];
  usersTotal: number;
  usersPage: number;
  auditLogs: AuditLogItem[];
  auditLogsTotal: number;
  auditLogsPage: number;
  loading: boolean;
}

export const AdminStore = signalStore(
  { providedIn: 'root' },
  withState<AdminState>({
    stats: null,
    users: [],
    usersTotal: 0,
    usersPage: 1,
    auditLogs: [],
    auditLogsTotal: 0,
    auditLogsPage: 1,
    loading: false,
  }),
  withMethods((store, api = inject(ApiService)) => ({

    async loadStats(): Promise<void> {
      try {
        const data = await firstValueFrom(api.get<SystemStats>('admin/stats'));
        patchState(store, { stats: data });
      } catch { /* silent */ }
    },

    async loadUsers(search?: string, isActive?: boolean, page = 1): Promise<void> {
      patchState(store, { loading: true });
      try {
        const params: Record<string, string | number | boolean> = { page, pageSize: 20 };
        if (search) params['search'] = search;
        if (isActive !== undefined) params['isActive'] = isActive;
        const res = await firstValueFrom(api.getPaged<AdminUser>('admin/users', params));
        patchState(store, { users: res.items, usersTotal: res.total, usersPage: page, loading: false });
      } catch {
        patchState(store, { loading: false });
      }
    },

    async toggleUserActive(id: string): Promise<void> {
      await firstValueFrom(api.put(`admin/users/${id}/toggle-active`, {}));
      patchState(store, {
        users: store.users().map(u => u.id === id ? { ...u, isActive: !u.isActive } : u),
      });
    },

    async loadAuditLogs(resource?: string, page = 1): Promise<void> {
      patchState(store, { loading: true });
      try {
        const params: Record<string, string | number> = { page, pageSize: 30 };
        if (resource) params['resource'] = resource;
        const res = await firstValueFrom(api.getPaged<AuditLogItem>('admin/audit-logs', params));
        patchState(store, { auditLogs: res.items, auditLogsTotal: res.total, auditLogsPage: page, loading: false });
      } catch {
        patchState(store, { loading: false });
      }
    },
  }))
);
