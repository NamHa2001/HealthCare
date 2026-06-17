import { Routes } from '@angular/router';

export const REMINDERS_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./reminder-list/reminder-list.component').then(m => m.ReminderListComponent),
  },
  {
    path: 'add',
    loadComponent: () =>
      import('./reminder-form/reminder-form.component').then(m => m.ReminderFormComponent),
  },
];
