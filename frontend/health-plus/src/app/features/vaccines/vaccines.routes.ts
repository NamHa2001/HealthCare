import { Routes } from '@angular/router';

export const VACCINES_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./vaccine-list/vaccine-list.component').then(m => m.VaccineListComponent),
  },
  {
    path: 'add',
    loadComponent: () =>
      import('./vaccine-form/vaccine-form.component').then(m => m.VaccineFormComponent),
  },
  {
    path: 'progress',
    loadComponent: () =>
      import('./vaccine-progress/vaccine-progress.component').then(m => m.VaccineProgressComponent),
  },
];
