import { Routes } from '@angular/router';

export const HEALTH_RECORDS_ROUTES: Routes = [
  {
    path: '',
    redirectTo: 'profile',
    pathMatch: 'full',
  },
  {
    path: 'profile',
    loadComponent: () =>
      import('./profile/health-profile.component').then(m => m.HealthProfileComponent),
    title: 'Hồ sơ sức khỏe — Health+',
  },
  {
    path: 'profile/edit',
    loadComponent: () =>
      import('./profile/health-profile-form.component').then(m => m.HealthProfileFormComponent),
    title: 'Chỉnh sửa hồ sơ — Health+',
  },
  {
    path: 'measurements',
    loadComponent: () =>
      import('./measurements/measurements.component').then(m => m.MeasurementsComponent),
    title: 'Chỉ số sức khỏe — Health+',
  },
  {
    path: 'blood-pressure',
    loadComponent: () =>
      import('./blood-pressure/blood-pressure.component').then(m => m.BloodPressureComponent),
    title: 'Huyết áp — Health+',
  },
];
