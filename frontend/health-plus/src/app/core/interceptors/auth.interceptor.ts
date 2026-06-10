import { HttpErrorResponse, HttpEvent, HttpHandlerFn, HttpInterceptorFn, HttpRequest } from '@angular/common/http';
import { inject } from '@angular/core';
import { BehaviorSubject, Observable, catchError, filter, switchMap, take, throwError } from 'rxjs';
import { AuthService } from '../auth/auth.service';

/** Các endpoint công khai — không gắn Authorization và không thử refresh. */
const PUBLIC_AUTH_PATHS = [
  '/Auth/login',
  '/Auth/register',
  '/Auth/refresh-token',
  '/Auth/forgot-password',
  '/Auth/reset-password',
  '/Auth/verify-email',
];

// Trạng thái refresh dùng chung để gom các request 401 đồng thời.
let isRefreshing = false;
const refreshToken$ = new BehaviorSubject<string | null>(null);

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const isPublic = PUBLIC_AUTH_PATHS.some((p) => req.url.includes(p));

  const token = authService.getAccessToken();
  const authReq = token && !isPublic ? addToken(req, token) : req;

  return next(authReq).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 401 && !isPublic && authService.getRefreshToken()) {
        return handle401(req, next, authService);
      }
      return throwError(() => error);
    }),
  );
};

function addToken(req: HttpRequest<unknown>, token: string): HttpRequest<unknown> {
  return req.clone({ setHeaders: { Authorization: `Bearer ${token}` } });
}

function handle401(
  req: HttpRequest<unknown>,
  next: HttpHandlerFn,
  authService: AuthService,
): Observable<HttpEvent<unknown>> {
  if (!isRefreshing) {
    isRefreshing = true;
    refreshToken$.next(null);

    return authService.refreshToken().pipe(
      switchMap((res) => {
        isRefreshing = false;
        refreshToken$.next(res.accessToken);
        return next(addToken(req, res.accessToken));
      }),
      catchError((err) => {
        isRefreshing = false;
        return throwError(() => err);
      }),
    );
  }

  // Đang refresh: chờ token mới rồi phát lại request.
  return refreshToken$.pipe(
    filter((t): t is string => t !== null),
    take(1),
    switchMap((t) => next(addToken(req, t))),
  );
}
