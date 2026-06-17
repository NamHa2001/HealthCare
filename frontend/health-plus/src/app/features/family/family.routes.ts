import { Routes } from '@angular/router';

export const FAMILY_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () => import('./family.component').then(m => m.FamilyComponent),
  },
  {
    path: 'accept-invite',
    loadComponent: () => import('./accept-invite/accept-invite.component').then(m => m.AcceptInviteComponent),
  },
];
