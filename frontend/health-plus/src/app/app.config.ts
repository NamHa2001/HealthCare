import { provideHttpClient, withInterceptors } from '@angular/common/http';
import {
  ApplicationConfig,
  inject,
  isDevMode,
  provideAppInitializer,
  provideBrowserGlobalErrorListeners,
} from '@angular/core';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { provideRouter } from '@angular/router';
import { provideServiceWorker } from '@angular/service-worker';

import { firstValueFrom } from 'rxjs';
import { AuthService } from './core/auth/auth.service';
import { SyncService } from './core/services/sync.service';
import { authInterceptor } from './core/interceptors/auth.interceptor';
import { errorInterceptor } from './core/interceptors/error.interceptor';
import { offlineQueueInterceptor } from './core/interceptors/offline-queue.interceptor';
import { routes } from './app.routes';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes),
    provideAnimationsAsync(),
    provideHttpClient(withInterceptors([errorInterceptor, authInterceptor, offlineQueueInterceptor])),
    provideServiceWorker('ngsw-worker.js', {
      enabled: !isDevMode(),
      registrationStrategy: 'registerWhenStable:30000',
    }),
    provideAppInitializer(async () => {
      // inject() phải gọi đồng bộ trước await đầu tiên
      const auth = inject(AuthService);
      const sync = inject(SyncService);
      await firstValueFrom(auth.restoreSession());
      sync.processSyncQueue();
    }),
  ],
};
