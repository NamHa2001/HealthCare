import { Routes } from '@angular/router';

export const SHARING_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./share-manager/share-manager.component').then(m => m.ShareManagerComponent),
  },
];
