import { inject } from '@angular/core';
import { patchState, signalStore, withMethods, withState } from '@ngrx/signals';
import { firstValueFrom } from 'rxjs';
import { ApiService } from '../../core/services/api.service';
import {
  CreateShareGrantRequest,
  CreateShareGrantResult,
  ShareGrant,
} from './models/share.model';

interface SharingState {
  profileId: string | null;
  grants: ShareGrant[];
  loading: boolean;
  /** Kết quả tạo mới nhất — chứa token gốc, chỉ hiển thị 1 lần */
  lastCreated: CreateShareGrantResult | null;
}

const initialState: SharingState = {
  profileId: null,
  grants: [],
  loading: false,
  lastCreated: null,
};

export const SharingStore = signalStore(
  { providedIn: 'root' },
  withState(initialState),
  withMethods((store, api = inject(ApiService)) => ({

    async load(): Promise<void> {
      patchState(store, { loading: true });
      try {
        let profileId = store.profileId();
        if (!profileId) {
          const profile = await firstValueFrom(api.get<{ id: string }>('HealthProfiles/me'));
          profileId = profile.id;
        }
        const grants = await firstValueFrom(
          api.get<ShareGrant[]>('ShareGrants', { profileId }));
        patchState(store, { profileId, grants, loading: false });
      } catch {
        patchState(store, { loading: false });
      }
    },

    async create(request: Omit<CreateShareGrantRequest, 'healthProfileId'>): Promise<CreateShareGrantResult> {
      const result = await firstValueFrom(api.post<CreateShareGrantResult>('ShareGrants', {
        ...request,
        healthProfileId: store.profileId(),
      }));
      patchState(store, { lastCreated: result });
      await this.load();
      return result;
    },

    async revoke(id: string): Promise<void> {
      await firstValueFrom(api.delete(`ShareGrants/${id}`));
      patchState(store, {
        grants: store.grants().map(g =>
          g.id === id ? { ...g, revokedAt: new Date().toISOString(), isActive: false } : g),
        lastCreated: store.lastCreated()?.id === id ? null : store.lastCreated(),
      });
    },

    clearLastCreated(): void {
      patchState(store, { lastCreated: null });
    },
  }))
);
