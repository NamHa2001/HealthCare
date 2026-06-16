import { inject } from '@angular/core';
import { patchState, signalStore, withMethods, withState } from '@ngrx/signals';
import { firstValueFrom } from 'rxjs';
import { ApiService } from '../../core/services/api.service';
import { AddBloodPressureRequest, BloodPressureLog } from './models/blood-pressure-log.model';
import { AddMeasurementRequest, HealthMeasurement } from './models/health-measurement.model';
import { HealthProfile, UpdateHealthProfileRequest } from './models/health-profile.model';

interface MeasurementsFilter {
  from?: string;
  to?: string;
  page?: number;
  pageSize?: number;
}

interface HealthRecordsState {
  profile: HealthProfile | null;
  profileLoading: boolean;

  measurements: HealthMeasurement[];
  measurementsTotal: number;
  measurementsPage: number;
  measurementsPageSize: number;
  measurementsLoading: boolean;

  bpLogs: BloodPressureLog[];
  bpTotal: number;
  bpPage: number;
  bpPageSize: number;
  bpLoading: boolean;
}

const initialState: HealthRecordsState = {
  profile: null,
  profileLoading: false,

  measurements: [],
  measurementsTotal: 0,
  measurementsPage: 1,
  measurementsPageSize: 20,
  measurementsLoading: false,

  bpLogs: [],
  bpTotal: 0,
  bpPage: 1,
  bpPageSize: 20,
  bpLoading: false,
};

export const HealthRecordsStore = signalStore(
  { providedIn: 'root' },
  withState(initialState),
  withMethods((store, api = inject(ApiService)) => ({

    async loadProfile(): Promise<void> {
      patchState(store, { profileLoading: true });
      try {
        const profile = await firstValueFrom(api.get<HealthProfile>('HealthProfiles/me'));
        patchState(store, { profile, profileLoading: false });
      } catch {
        patchState(store, { profileLoading: false });
      }
    },

    async updateProfile(request: UpdateHealthProfileRequest): Promise<void> {
      await firstValueFrom(api.put<void>('HealthProfiles/me', request));
      // Re-load to get server-computed fields
      patchState(store, { profileLoading: true });
      try {
        const profile = await firstValueFrom(api.get<HealthProfile>('HealthProfiles/me'));
        patchState(store, { profile, profileLoading: false });
      } catch {
        patchState(store, { profileLoading: false });
      }
    },

    async loadMeasurements(filter: MeasurementsFilter = {}): Promise<void> {
      patchState(store, { measurementsLoading: true });
      try {
        const result = await firstValueFrom(
          api.getPaged<HealthMeasurement>('Measurements', {
            from: filter.from,
            to: filter.to,
            page: filter.page ?? store.measurementsPage(),
            pageSize: filter.pageSize ?? store.measurementsPageSize(),
          })
        );
        patchState(store, {
          measurements: result.items,
          measurementsTotal: result.total,
          measurementsPage: result.page,
          measurementsPageSize: result.pageSize,
          measurementsLoading: false,
        });
      } catch {
        patchState(store, { measurementsLoading: false });
      }
    },

    async addMeasurement(request: AddMeasurementRequest): Promise<void> {
      await firstValueFrom(api.post<string>('Measurements', request));
      // Reload page 1 to show newest first
      patchState(store, { measurementsLoading: true });
      try {
        const result = await firstValueFrom(
          api.getPaged<HealthMeasurement>('Measurements', { page: 1, pageSize: store.measurementsPageSize() })
        );
        patchState(store, {
          measurements: result.items,
          measurementsTotal: result.total,
          measurementsPage: result.page,
          measurementsLoading: false,
        });
      } catch {
        patchState(store, { measurementsLoading: false });
      }
    },

    async deleteMeasurement(id: string): Promise<void> {
      await firstValueFrom(api.delete(`Measurements/${id}`));
      patchState(store, {
        measurements: store.measurements().filter(m => m.id !== id),
        measurementsTotal: store.measurementsTotal() - 1,
      });
    },

    async loadBpLogs(filter: MeasurementsFilter = {}): Promise<void> {
      patchState(store, { bpLoading: true });
      try {
        const result = await firstValueFrom(
          api.getPaged<BloodPressureLog>('BloodPressure', {
            from: filter.from,
            to: filter.to,
            page: filter.page ?? store.bpPage(),
            pageSize: filter.pageSize ?? store.bpPageSize(),
          })
        );
        patchState(store, {
          bpLogs: result.items,
          bpTotal: result.total,
          bpPage: result.page,
          bpPageSize: result.pageSize,
          bpLoading: false,
        });
      } catch {
        patchState(store, { bpLoading: false });
      }
    },

    async addBpLog(request: AddBloodPressureRequest): Promise<void> {
      await firstValueFrom(api.post<string>('BloodPressure', request));
      patchState(store, { bpLoading: true });
      try {
        const result = await firstValueFrom(
          api.getPaged<BloodPressureLog>('BloodPressure', { page: 1, pageSize: store.bpPageSize() })
        );
        patchState(store, {
          bpLogs: result.items,
          bpTotal: result.total,
          bpPage: result.page,
          bpLoading: false,
        });
      } catch {
        patchState(store, { bpLoading: false });
      }
    },

    async deleteBpLog(id: string): Promise<void> {
      await firstValueFrom(api.delete(`BloodPressure/${id}`));
      patchState(store, {
        bpLogs: store.bpLogs().filter(b => b.id !== id),
        bpTotal: store.bpTotal() - 1,
      });
    },

  })),
);
