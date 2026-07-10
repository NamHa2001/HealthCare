import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthStore } from './auth.store';

/** Chỉ cho vào khu vực bác sĩ khi JWT có role 'doctor'. */
export const doctorGuard: CanActivateFn = () => {
  const authStore = inject(AuthStore);
  const router = inject(Router);

  if (authStore.isDoctor()) return true;
  return router.createUrlTree(['/dashboard']);
};
