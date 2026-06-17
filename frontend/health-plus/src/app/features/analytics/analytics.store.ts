import { inject } from '@angular/core';
import { patchState, signalStore, withMethods, withState } from '@ngrx/signals';
import { firstValueFrom } from 'rxjs';
import { ApiService } from '../../core/services/api.service';
import {
  BpTrendPoint, ComplianceWeek, HealthScore, HealthSummary,
  TrendPoint, VaccineProgress, VisitFrequency,
} from './models/analytics.model';

interface AnalyticsState {
  summary:          HealthSummary | null;
  healthScore:      HealthScore | null;
  bmiTrend:         TrendPoint[];
  bpTrend:          BpTrendPoint[];
  glucoseTrend:     TrendPoint[];
  vaccineProgress:  VaccineProgress[];
  compliance:       ComplianceWeek[];
  visitFrequency:   VisitFrequency[];
  loading:          boolean;
}

export const AnalyticsStore = signalStore(
  { providedIn: 'root' },
  withState<AnalyticsState>({
    summary:         null,
    healthScore:     null,
    bmiTrend:        [],
    bpTrend:         [],
    glucoseTrend:    [],
    vaccineProgress: [],
    compliance:      [],
    visitFrequency:  [],
    loading:         false,
  }),
  withMethods((store, api = inject(ApiService)) => ({
    async loadSummary(): Promise<void> {
      patchState(store, { loading: true });
      try {
        const data = await firstValueFrom(api.get<HealthSummary>('analytics/summary'));
        patchState(store, { summary: data, loading: false });
      } catch {
        patchState(store, { loading: false });
      }
    },

    async loadHealthScore(): Promise<void> {
      try {
        const data = await firstValueFrom(api.get<HealthScore>('analytics/health-score'));
        patchState(store, { healthScore: data });
      } catch { /* silent */ }
    },

    async loadBmiTrend(days = 30): Promise<void> {
      try {
        const data = await firstValueFrom(api.get<TrendPoint[]>('analytics/bmi-trend', { days }));
        patchState(store, { bmiTrend: data });
      } catch { /* silent */ }
    },

    async loadBpTrend(days = 30): Promise<void> {
      try {
        const data = await firstValueFrom(api.get<BpTrendPoint[]>('analytics/bp-trend', { days }));
        patchState(store, { bpTrend: data });
      } catch { /* silent */ }
    },

    async loadGlucoseTrend(days = 30): Promise<void> {
      try {
        const data = await firstValueFrom(api.get<TrendPoint[]>('analytics/glucose-trend', { days }));
        patchState(store, { glucoseTrend: data });
      } catch { /* silent */ }
    },

    async loadVaccineProgress(): Promise<void> {
      try {
        const data = await firstValueFrom(api.get<VaccineProgress[]>('analytics/vaccine-progress'));
        patchState(store, { vaccineProgress: data });
      } catch { /* silent */ }
    },

    async loadCompliance(weeks = 8): Promise<void> {
      try {
        const data = await firstValueFrom(api.get<ComplianceWeek[]>('analytics/medication-compliance', { weeks }));
        patchState(store, { compliance: data });
      } catch { /* silent */ }
    },

    async loadVisitFrequency(months = 6): Promise<void> {
      try {
        const data = await firstValueFrom(api.get<VisitFrequency[]>('analytics/visit-frequency', { months }));
        patchState(store, { visitFrequency: data });
      } catch { /* silent */ }
    },
  }))
);
