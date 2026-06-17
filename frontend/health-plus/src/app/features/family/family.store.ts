import { inject } from '@angular/core';
import { patchState, signalStore, withMethods, withState } from '@ngrx/signals';
import { firstValueFrom } from 'rxjs';
import { ApiService } from '../../core/services/api.service';
import { FamilyGroup, FamilyMember, AddMemberRequest } from './models/family.model';

interface FamilyState {
  group: FamilyGroup | null;
  loading: boolean;
}

export const FamilyStore = signalStore(
  { providedIn: 'root' },
  withState<FamilyState>({ group: null, loading: false }),
  withMethods((store, api = inject(ApiService)) => ({
    async loadFamily(): Promise<void> {
      patchState(store, { loading: true });
      try {
        const data = await firstValueFrom(api.get<FamilyGroup | null>('family'));
        patchState(store, { group: data, loading: false });
      } catch {
        patchState(store, { loading: false });
      }
    },

    async createGroup(name: string): Promise<FamilyGroup> {
      const group = await firstValueFrom(api.post<FamilyGroup>('family', { name }));
      patchState(store, { group });
      return group;
    },

    async addMember(req: AddMemberRequest): Promise<FamilyMember> {
      const member = await firstValueFrom(api.post<FamilyMember>('family/members', req));
      const current = store.group();
      if (current) {
        patchState(store, { group: { ...current, members: [...current.members, member] } });
      }
      return member;
    },

    async removeMember(memberId: string): Promise<void> {
      await firstValueFrom(api.delete(`family/members/${memberId}`));
      const current = store.group();
      if (current) {
        patchState(store, {
          group: { ...current, members: current.members.filter(m => m.id !== memberId) },
        });
      }
    },
  }))
);
