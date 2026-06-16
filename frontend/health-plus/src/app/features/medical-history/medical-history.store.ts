import { inject } from '@angular/core';
import { patchState, signalStore, withMethods, withState } from '@ngrx/signals';
import { firstValueFrom, map } from 'rxjs';
import { HttpClient } from '@angular/common/http';
import { ApiService } from '../../core/services/api.service';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../../core/models/api-response.model';
import {
  CreateMedicalVisitRequest,
  MedicalDocument,
  MedicalVisit,
  MedicalVisitListItem,
  UpdateMedicalVisitRequest,
} from './models/medical-visit.model';

interface VisitsFilter {
  page?: number;
  pageSize?: number;
  year?: number | null;
  month?: number | null;
}

interface MedicalHistoryState {
  visits: MedicalVisitListItem[];
  visitsTotal: number;
  visitsPage: number;
  visitsPageSize: number;
  visitsLoading: boolean;
  filterYear: number | null;
  filterMonth: number | null;

  currentVisit: MedicalVisit | null;
  currentVisitLoading: boolean;
}

const initialState: MedicalHistoryState = {
  visits: [],
  visitsTotal: 0,
  visitsPage: 1,
  visitsPageSize: 10,
  visitsLoading: false,
  filterYear: null,
  filterMonth: null,

  currentVisit: null,
  currentVisitLoading: false,
};

export const MedicalHistoryStore = signalStore(
  { providedIn: 'root' },
  withState(initialState),
  withMethods((store, api = inject(ApiService), http = inject(HttpClient)) => ({

    async loadVisits(filter: VisitsFilter = {}): Promise<void> {
      patchState(store, { visitsLoading: true });
      const year = filter.year !== undefined ? filter.year : store.filterYear();
      const month = filter.month !== undefined ? filter.month : store.filterMonth();
      try {
        const result = await firstValueFrom(
          api.getPaged<MedicalVisitListItem>('MedicalVisits', {
            page: filter.page ?? store.visitsPage(),
            pageSize: filter.pageSize ?? store.visitsPageSize(),
            year,
            month,
          })
        );
        patchState(store, {
          visits: result.items,
          visitsTotal: result.total,
          visitsPage: result.page,
          visitsPageSize: result.pageSize,
          filterYear: year,
          filterMonth: month,
          visitsLoading: false,
        });
      } catch {
        patchState(store, { visitsLoading: false });
      }
    },

    async loadVisitById(id: string): Promise<void> {
      patchState(store, { currentVisit: null, currentVisitLoading: true });
      try {
        const visit = await firstValueFrom(api.get<MedicalVisit>(`MedicalVisits/${id}`));
        patchState(store, { currentVisit: visit, currentVisitLoading: false });
      } catch {
        patchState(store, { currentVisitLoading: false });
      }
    },

    async createVisit(request: CreateMedicalVisitRequest): Promise<MedicalVisit> {
      const created = await firstValueFrom(api.post<MedicalVisit>('MedicalVisits', request));
      return created;
    },

    async updateVisit(id: string, request: UpdateMedicalVisitRequest): Promise<MedicalVisit> {
      const updated = await firstValueFrom(api.put<MedicalVisit>(`MedicalVisits/${id}`, request));
      patchState(store, { currentVisit: updated });
      return updated;
    },

    async deleteVisit(id: string): Promise<void> {
      await firstValueFrom(api.delete(`MedicalVisits/${id}`));
      patchState(store, {
        visits: store.visits().filter(v => v.id !== id),
        visitsTotal: Math.max(0, store.visitsTotal() - 1),
      });
    },

    /** Upload tài liệu qua multipart/form-data. Trả document vừa tạo. */
    async uploadDocument(
      file: File,
      opts: { medicalVisitId?: string; documentType?: string } = {}
    ): Promise<MedicalDocument> {
      const formData = new FormData();
      formData.append('file', file, file.name);
      if (opts.medicalVisitId) formData.append('medicalVisitId', opts.medicalVisitId);
      if (opts.documentType) formData.append('documentType', opts.documentType);

      const doc = await firstValueFrom(
        http
          .post<ApiResponse<MedicalDocument>>(`${environment.apiUrl}/Documents/upload`, formData)
          .pipe(map(res => res.data))
      );

      // Cập nhật currentVisit nếu document thuộc visit đang xem
      const current = store.currentVisit();
      if (current && doc.medicalVisitId === current.id) {
        patchState(store, { currentVisit: { ...current, documents: [...current.documents, doc] } });
      }
      return doc;
    },

    async deleteDocument(id: string): Promise<void> {
      await firstValueFrom(api.delete(`Documents/${id}`));
      const current = store.currentVisit();
      if (current) {
        patchState(store, {
          currentVisit: { ...current, documents: current.documents.filter(d => d.id !== id) },
        });
      }
    },

    /** Lấy signed URL (1 giờ) để tải/xem tài liệu. */
    async getDownloadUrl(documentId: string): Promise<string> {
      return firstValueFrom(api.get<string>(`Documents/${documentId}/download`));
    },

  })),
);
