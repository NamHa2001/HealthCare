import { Routes } from '@angular/router';
import { authGuard } from './core/auth/auth.guard';
import { adminGuard } from './core/auth/admin.guard';

export const routes: Routes = [
  // Redirect gốc → dashboard
  {
    path: '',
    redirectTo: 'dashboard',
    pathMatch: 'full',
  },

  // Trang xác thực (không có sidebar)
  {
    path: 'auth',
    loadComponent: () =>
      import('./layouts/auth-layout/auth-layout.component').then(m => m.AuthLayoutComponent),
    loadChildren: () =>
      import('./features/auth/auth.routes').then(m => m.AUTH_ROUTES),
  },

  // Trang bảo vệ (có sidebar + topbar)
  {
    path: '',
    loadComponent: () =>
      import('./layouts/main-layout/main-layout.component').then(m => m.MainLayoutComponent),
    canActivate: [authGuard],
    children: [
      {
        path: 'dashboard',
        loadComponent: () =>
          import('./features/dashboard/dashboard.component').then(m => m.DashboardComponent),
        title: 'Dashboard — Health+',
      },
      {
        path: 'health-records',
        loadChildren: () =>
          import('./features/health-records/health-records.routes').then(m => m.HEALTH_RECORDS_ROUTES),
        title: 'Hồ sơ sức khỏe — Health+',
      },
      {
        path: 'medical-history',
        loadChildren: () =>
          import('./features/medical-history/medical-history.routes').then(m => m.MEDICAL_HISTORY_ROUTES),
        title: 'Lịch sử khám — Health+',
      },
      {
        path: 'medications',
        loadChildren: () =>
          import('./features/medications/medications.routes').then(m => m.MEDICATIONS_ROUTES),
        title: 'Thuốc — Health+',
      },
      {
        path: 'ocr',
        loadChildren: () =>
          import('./features/ocr/ocr.routes').then(m => m.OCR_ROUTES),
        title: 'Quét đơn thuốc — Health+',
      },
      {
        path: 'admin',
        loadComponent: () =>
          import('./features/admin/admin-dashboard/admin-dashboard.component').then(m => m.AdminDashboardComponent),
        canActivate: [adminGuard],
        title: 'Quản trị — Health+',
      },
    ],
  },

  // Fallback — redirect về dashboard (authGuard sẽ chuyển sang login nếu chưa đăng nhập)
  {
    path: '**',
    redirectTo: 'dashboard',
  },
];
