import { Routes } from '@angular/router';

export const MEDICATIONS_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./medication-list/medication-list.component').then(m => m.MedicationListComponent),
    title: 'Thuốc — Health+',
  },
  {
    path: 'new',
    loadComponent: () =>
      import('./medication-form/medication-form.component').then(m => m.MedicationFormComponent),
    title: 'Thêm thuốc — Health+',
  },
  {
    path: ':id/edit',
    loadComponent: () =>
      import('./medication-form/medication-form.component').then(m => m.MedicationFormComponent),
    title: 'Chỉnh sửa thuốc — Health+',
  },
  {
    path: ':id/schedule',
    loadComponent: () =>
      import('./medication-schedule/medication-schedule.component').then(m => m.MedicationScheduleComponent),
    title: 'Lịch uống thuốc — Health+',
  },
  {
    path: 'logs',
    loadComponent: () =>
      import('./medication-log/medication-log.component').then(m => m.MedicationLogComponent),
    title: 'Nhật ký uống thuốc — Health+',
  },
];
