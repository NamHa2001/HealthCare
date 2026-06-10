import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { AuthService } from '../auth/auth.service';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const snackBar = inject(MatSnackBar);
  const router = inject(Router);
  const authService = inject(AuthService);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      let message = 'Đã xảy ra lỗi. Vui lòng thử lại.';

      // Envelope lỗi SRS §8.2: { success: false, error: { code, message } }
      if (error.error?.error?.message) {
        message = error.error.error.message;
      } else if (error.status === 0) {
        message = 'Không thể kết nối tới máy chủ.';
      }

      // 401 mà không còn refresh token → buộc đăng nhập lại.
      if (error.status === 401 && !authService.getRefreshToken()) {
        router.navigate(['/auth/login']);
      }

      snackBar.open(message, 'Đóng', { duration: 4000 });
      return throwError(() => error);
    }),
  );
};
