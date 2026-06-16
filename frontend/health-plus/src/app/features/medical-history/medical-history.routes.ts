import { Routes } from '@angular/router';

export const MEDICAL_HISTORY_ROUTES: Routes = [
  {
    path: '',
    redirectTo: 'visits',
    pathMatch: 'full',
  },
  {
    path: 'visits',
    loadComponent: () =>
      import('./visit-list/visit-list.component').then(m => m.VisitListComponent),
    title: 'Lịch sử khám — Health+',
  },
  {
    path: 'visits/new',
    loadComponent: () =>
      import('./visit-form/visit-form.component').then(m => m.VisitFormComponent),
    title: 'Thêm lần khám — Health+',
  },
  {
    path: 'visits/:id',
    loadComponent: () =>
      import('./visit-detail/visit-detail.component').then(m => m.VisitDetailComponent),
    title: 'Chi tiết lần khám — Health+',
  },
  {
    path: 'visits/:id/edit',
    loadComponent: () =>
      import('./visit-form/visit-form.component').then(m => m.VisitFormComponent),
    title: 'Chỉnh sửa lần khám — Health+',
  },
];
