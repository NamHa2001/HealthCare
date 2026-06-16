import { inject } from '@angular/core';
import { patchState, signalStore, withMethods, withState } from '@ngrx/signals';
import { firstValueFrom } from 'rxjs';
import { ApiService } from '../../core/services/api.service';
import {
  AddScheduleRequest,
  ComplianceReport,
  CreateMedicationRequest,
  Medication,
  MedicationLog,
  MedicationSchedule,
  OcrResult,
  UpdateMedicationRequest,
} from './models/medication.model';

interface MedicationsState {
  medications: Medication[];
  loading: boolean;
  activeOnly: boolean;

  todayLogs: MedicationLog[];
  logsLoading: boolean;
  logsDate: string; // 'yyyy-MM-dd'

  compliance: ComplianceReport[];
  complianceLoading: boolean;

  ocrResult: OcrResult | null;
  ocrLoading: boolean;
}

const todayIso = (): string => new Date().toISOString().substring(0, 10);

const initialState: MedicationsState = {
  medications: [],
  loading: false,
  activeOnly: true,

  todayLogs: [],
  logsLoading: false,
  logsDate: todayIso(),

  compliance: [],
  complianceLoading: false,

  ocrResult: null,
  ocrLoading: false,
};

export const MedicationsStore = signalStore(
  { providedIn: 'root' },
  withState(initialState),
  withMethods((store, api = inject(ApiService)) => ({

    async loadMedications(activeOnly?: boolean): Promise<void> {
      const flag = activeOnly !== undefined ? activeOnly : store.activeOnly();
      patchState(store, { loading: true, activeOnly: flag });
      try {
        const meds = await firstValueFrom(
          api.get<Medication[]>('Medications', { activeOnly: flag })
        );
        patchState(store, { medications: meds, loading: false });
      } catch {
        patchState(store, { loading: false });
      }
    },

    async createMedication(request: CreateMedicationRequest): Promise<Medication> {
      const med = await firstValueFrom(api.post<Medication>('Medications', request));
      patchState(store, { medications: [med, ...store.medications()] });
      return med;
    },

    async updateMedication(id: string, request: UpdateMedicationRequest): Promise<Medication> {
      const med = await firstValueFrom(api.put<Medication>(`Medications/${id}`, request));
      patchState(store, {
        medications: store.medications().map(m => m.id === id ? med : m),
      });
      return med;
    },

    async deleteMedication(id: string): Promise<void> {
      await firstValueFrom(api.delete(`Medications/${id}`));
      patchState(store, { medications: store.medications().filter(m => m.id !== id) });
    },

    async addSchedule(medicationId: string, request: AddScheduleRequest): Promise<MedicationSchedule> {
      const schedule = await firstValueFrom(
        api.post<MedicationSchedule>(`Medications/${medicationId}/schedules`, request)
      );
      patchState(store, {
        medications: store.medications().map(m =>
          m.id === medicationId
            ? { ...m, schedules: [...m.schedules, schedule] }
            : m
        ),
      });
      return schedule;
    },

    async deleteSchedule(medicationId: string, scheduleId: string): Promise<void> {
      await firstValueFrom(api.delete(`Medications/schedules/${scheduleId}`));
      patchState(store, {
        medications: store.medications().map(m =>
          m.id === medicationId
            ? { ...m, schedules: m.schedules.filter(s => s.id !== scheduleId) }
            : m
        ),
      });
    },

    async loadTodayLogs(date?: string): Promise<void> {
      const d = date ?? store.logsDate();
      patchState(store, { logsLoading: true, logsDate: d });
      try {
        const logs = await firstValueFrom(
          api.get<MedicationLog[]>('Medications/logs', { date: d })
        );
        patchState(store, { todayLogs: logs, logsLoading: false });
      } catch {
        patchState(store, { logsLoading: false });
      }
    },

    async markTaken(logId: string): Promise<void> {
      await firstValueFrom(api.post<void>(`Medications/logs/${logId}/taken`, {}));
      patchState(store, {
        todayLogs: store.todayLogs().map(l =>
          l.id === logId ? { ...l, status: 'Taken' as const, takenAt: new Date().toISOString() } : l
        ),
      });
    },

    async markSkipped(logId: string, skipReason?: string): Promise<void> {
      await firstValueFrom(api.post<void>(`Medications/logs/${logId}/skipped`, { skipReason }));
      patchState(store, {
        todayLogs: store.todayLogs().map(l =>
          l.id === logId ? { ...l, status: 'Skipped' as const, skipReason } : l
        ),
      });
    },

    async loadCompliance(weeksBack = 4): Promise<void> {
      patchState(store, { complianceLoading: true });
      try {
        const data = await firstValueFrom(
          api.get<ComplianceReport[]>('Medications/compliance', { weeksBack })
        );
        patchState(store, { compliance: data, complianceLoading: false });
      } catch {
        patchState(store, { complianceLoading: false });
      }
    },

    async triggerOcr(documentId: string): Promise<OcrResult> {
      patchState(store, { ocrLoading: true, ocrResult: null });
      try {
        const result = await firstValueFrom(
          api.post<OcrResult>(`Documents/${documentId}/ocr`, {})
        );
        patchState(store, { ocrResult: result, ocrLoading: false });
        return result;
      } catch (err) {
        patchState(store, { ocrLoading: false });
        throw err;
      }
    },

    async getOcrResult(documentId: string): Promise<OcrResult> {
      const result = await firstValueFrom(
        api.get<OcrResult>(`Documents/${documentId}/ocr`)
      );
      patchState(store, { ocrResult: result });
      return result;
    },

    clearOcrResult(): void {
      patchState(store, { ocrResult: null });
    },
  }))
);
