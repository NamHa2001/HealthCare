import { inject } from '@angular/core';
import { patchState, signalStore, withMethods, withState } from '@ngrx/signals';
import { firstValueFrom } from 'rxjs';
import { ApiService } from '../../core/services/api.service';
import { CreateReminderRequest, Reminder, ReminderStatus } from './models/reminder.model';

interface RemindersState {
  reminders: Reminder[];
  loading: boolean;
  filterStatus: ReminderStatus | null;
}

const initialState: RemindersState = {
  reminders: [],
  loading: false,
  filterStatus: null,
};

export const RemindersStore = signalStore(
  { providedIn: 'root' },
  withState(initialState),
  withMethods((store, api = inject(ApiService)) => ({

    async loadReminders(status?: ReminderStatus | null): Promise<void> {
      const s = status !== undefined ? status : store.filterStatus();
      patchState(store, { loading: true, filterStatus: s ?? null });
      try {
        const params: Record<string, string> = {};
        if (s) params['status'] = s;
        const data = await firstValueFrom(api.get<Reminder[]>('Reminders', params));
        patchState(store, { reminders: data, loading: false });
      } catch {
        patchState(store, { loading: false });
      }
    },

    async createReminder(request: CreateReminderRequest): Promise<Reminder> {
      const r = await firstValueFrom(api.post<Reminder>('Reminders', request));
      patchState(store, { reminders: [r, ...store.reminders()] });
      return r;
    },

    async cancelReminder(id: string): Promise<void> {
      await firstValueFrom(api.delete(`Reminders/${id}`));
      patchState(store, {
        reminders: store.reminders().map(r =>
          r.id === id ? { ...r, status: 'cancelled' as const } : r
        ),
      });
    },

    setFilter(status: ReminderStatus | null): void {
      patchState(store, { filterStatus: status });
    },
  }))
);
