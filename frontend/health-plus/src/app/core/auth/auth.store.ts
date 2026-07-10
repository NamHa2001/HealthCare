import { computed } from '@angular/core';
import { patchState, signalStore, withComputed, withMethods, withState } from '@ngrx/signals';
import { User } from './models/auth-response.model';

interface AuthState {
  currentUser: User | null;
  accessToken: string | null;
  refreshToken: string | null;
}

const initialState: AuthState = {
  currentUser: null,
  accessToken: null,
  refreshToken: null,
};

export const AuthStore = signalStore(
  { providedIn: 'root' },
  withState(initialState),
  withComputed((store) => ({
    isLoggedIn: computed(() => !!store.accessToken()),
    roles: computed(() => store.currentUser()?.roles ?? []),
    isAdmin: computed(() => (store.currentUser()?.roles ?? []).includes('admin')),
    isDoctor: computed(() => (store.currentUser()?.roles ?? []).includes('doctor')),
  })),
  withMethods((store) => ({
    setAuth(user: User, accessToken: string, refreshToken: string): void {
      patchState(store, { currentUser: user, accessToken, refreshToken });
    },
    setUser(user: User): void {
      patchState(store, { currentUser: user });
    },
    setTokens(accessToken: string, refreshToken: string): void {
      patchState(store, { accessToken, refreshToken });
    },
    clear(): void {
      patchState(store, initialState);
    },
  })),
);
