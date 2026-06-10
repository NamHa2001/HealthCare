import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthStore } from './auth.store';

export const adminGuard: CanActivateFn = () => {
  const store = inject(AuthStore);
  const router = inject(Router);

  if (store.isLoggedIn() && store.isAdmin()) {
    return true;
  }
  return router.createUrlTree(['/dashboard']);
};
