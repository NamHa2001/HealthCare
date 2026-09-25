import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { EMPTY, throwError } from 'rxjs';
import { IndexedDbService } from '../db/indexeddb.service';
import { OfflineService } from '../services/offline.service';

const MUTATING_METHODS = new Set(['POST', 'PUT', 'PATCH', 'DELETE']);

// Paths that should never be queued (auth flows need real-time response)
const EXCLUDED_PATHS = ['/Auth/', '/auth/'];

export const offlineQueueInterceptor: HttpInterceptorFn = (req, next) => {
  const offline = inject(OfflineService);
  const db = inject(IndexedDbService);

  const isMutating = MUTATING_METHODS.has(req.method);
  const isExcluded = EXCLUDED_PATHS.some(p => req.url.includes(p));

  if (!offline.isOnline() && isMutating && !isExcluded) {
    // BUG-25: từng lưu kèm authToken lúc queue, nhưng khi replay request đi qua lại authInterceptor
    // — interceptor đó luôn ghi đè header Authorization bằng token mới nhất, nên giá trị lưu ở đây
    // chưa từng thực sự được dùng trên đường đi chính (chỉ vô dụng theo cách khác nếu đã logout).
    db.enqueue({
      method: req.method as 'POST' | 'PUT' | 'PATCH' | 'DELETE',
      url: req.urlWithParams,
      body: req.body,
    }).catch(console.error);

    // Signal upstream that request was queued, not failed
    return throwError(() => Object.assign(new Error('OFFLINE_QUEUED'), { queued: true }));
  }

  if (!offline.isOnline() && !isMutating && !isExcluded) {
    // GET when offline → let service worker / cache handle it; if no cache, fail silently
    return next(req);
  }

  return next(req);
};
