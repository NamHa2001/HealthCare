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
    catchError((error: unknown) => {
      // Request đã được xếp hàng offline — hiển thị thông báo nhẹ, không phải lỗi.
      if (error instanceof Error && (error as any).queued) {
        snackBar.open('Đang ngoại tuyến — yêu cầu sẽ được đồng bộ khi có kết nối.', 'OK', {
          duration: 4000,
          panelClass: ['bg-gray-800', 'text-white'],
        });
        return throwError(() => error);
      }

      if (!(error instanceof HttpErrorResponse)) return throwError(() => error);

      // 401 mà không còn refresh token → buộc đăng nhập lại.
      if (error.status === 401 && !authService.getRefreshToken()) {
        router.navigate(['/auth/login']);
      }

      // 404: component tự xử lý empty-state, không hiện snackbar.
      if (error.status === 404) {
        return throwError(() => error);
      }

      let message = 'Đã xảy ra lỗi. Vui lòng thử lại.';
      if (error.error?.error?.message) {
        message = error.error.error.message;
      } else if (error.status === 0) {
        message = 'Không thể kết nối tới máy chủ.';
      }

      snackBar.open(message, 'Đóng', { duration: 4000 });
      return throwError(() => error);
    }),
  );
};
