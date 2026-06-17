import { inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { patchState, signalStore, withMethods, withState } from '@ngrx/signals';
import { firstValueFrom } from 'rxjs';
import { ApiService } from '../../core/services/api.service';
import { environment } from '../../../environments/environment';
import { CreateVaccineRecordRequest, VaccineCatalog, VaccineProgress, VaccineRecord } from './models/vaccine.model';

interface VaccinesState {
  records: VaccineRecord[];
  catalog: VaccineCatalog[];
  progress: VaccineProgress[];
  loading: boolean;
  error: string | null;
}

export const VaccinesStore = signalStore(
  { providedIn: 'root' },
  withState<VaccinesState>({
    records: [],
    catalog: [],
    progress: [],
    loading: false,
    error: null,
  }),
  withMethods((store, api = inject(ApiService), http = inject(HttpClient)) => ({
    async loadRecords(): Promise<void> {
      patchState(store, { loading: true, error: null });
      try {
        const res = await firstValueFrom(api.get<VaccineRecord[]>('vaccines'));
        patchState(store, { records: res, loading: false });
      } catch (e: any) {
        patchState(store, { error: e.message, loading: false });
      }
    },

    async loadCatalog(): Promise<void> {
      patchState(store, { loading: true });
      try {
        const res = await firstValueFrom(api.get<VaccineCatalog[]>('vaccines/catalog'));
        patchState(store, { catalog: res, loading: false });
      } catch (e: any) {
        patchState(store, { error: e.message, loading: false });
      }
    },

    async loadProgress(): Promise<void> {
      patchState(store, { loading: true });
      try {
        const res = await firstValueFrom(api.get<VaccineProgress[]>('vaccines/progress'));
        patchState(store, { progress: res, loading: false });
      } catch (e: any) {
        patchState(store, { error: e.message, loading: false });
      }
    },

    async createRecord(request: CreateVaccineRecordRequest): Promise<VaccineRecord> {
      const result = await firstValueFrom(api.post<VaccineRecord>('vaccines', request));
      patchState(store, { records: [result, ...store.records()] });
      return result;
    },

    async deleteRecord(id: string): Promise<void> {
      await firstValueFrom(api.delete(`vaccines/${id}`));
      patchState(store, { records: store.records().filter(r => r.id !== id) });
    },

    async downloadPassport(): Promise<void> {
      const blob = await firstValueFrom(
        http.get(`${environment.apiUrl}/vaccines/passport`, { responseType: 'blob' })
      );
      const url = URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `vaccine-passport-${new Date().toISOString().slice(0, 10)}.pdf`;
      a.click();
      URL.revokeObjectURL(url);
    },
  }))
);
