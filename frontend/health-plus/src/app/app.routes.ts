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

  // Trang public: xem hồ sơ được chia sẻ qua token — KHÔNG cần đăng nhập
  {
    path: 'shared/:token',
    loadComponent: () =>
      import('./features/sharing/shared-viewer/shared-viewer.component').then(m => m.SharedViewerComponent),
    title: 'Hồ sơ được chia sẻ — Health+',
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
        path: 'vaccines',
        loadChildren: () =>
          import('./features/vaccines/vaccines.routes').then(m => m.VACCINES_ROUTES),
        title: 'Vắc-xin — Health+',
      },
      {
        path: 'medications',
        loadChildren: () =>
          import('./features/medications/medications.routes').then(m => m.MEDICATIONS_ROUTES),
        title: 'Thuốc — Health+',
      },
      {
        path: 'reminders',
        loadChildren: () =>
          import('./features/reminders/reminders.routes').then(m => m.REMINDERS_ROUTES),
        title: 'Nhắc lịch — Health+',
      },
      {
        path: 'ocr',
        loadChildren: () =>
          import('./features/ocr/ocr.routes').then(m => m.OCR_ROUTES),
        title: 'Quét đơn thuốc — Health+',
      },
      {
        path: 'analytics',
        loadChildren: () =>
          import('./features/analytics/analytics.routes').then(m => m.ANALYTICS_ROUTES),
        title: 'Phân tích — Health+',
      },
      {
        path: 'sharing',
        loadChildren: () =>
          import('./features/sharing/sharing.routes').then(m => m.SHARING_ROUTES),
        title: 'Chia sẻ hồ sơ — Health+',
      },
      {
        path: 'family',
        loadChildren: () =>
          import('./features/family/family.routes').then(m => m.FAMILY_ROUTES),
        title: 'Gia đình — Health+',
      },
      {
        path: 'settings',
        loadChildren: () =>
          import('./features/settings/settings.routes').then(m => m.SETTINGS_ROUTES),
        title: 'Cài đặt — Health+',
      },
      {
        path: 'admin',
        loadChildren: () =>
          import('./features/admin/admin.routes').then(m => m.ADMIN_ROUTES),
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
