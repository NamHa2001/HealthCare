import { Routes } from '@angular/router';

export const ADMIN_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./admin-dashboard/admin-dashboard.component').then(m => m.AdminDashboardComponent),
  },
  {
    path: 'users',
    loadComponent: () =>
      import('./user-list/user-list.component').then(m => m.UserListComponent),
  },
  {
    path: 'audit-logs',
    loadComponent: () =>
      import('./audit-logs/audit-log-list.component').then(m => m.AuditLogListComponent),
  },
];
