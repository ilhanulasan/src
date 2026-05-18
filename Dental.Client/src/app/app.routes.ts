import { Routes } from '@angular/router';

import { authGuard } from './core/auth.guard';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () => import('./home/home.component').then((m) => m.HomeComponent),
  },
  {
    path: 'login',
    loadComponent: () => import('./auth/login/login.component').then((m) => m.LoginComponent),
  },
  {
    path: 'register',
    loadComponent: () => import('./auth/register/register.component').then((m) => m.RegisterComponent),
  },
  {
    path: 'profile',
    canActivate: [authGuard],
    loadComponent: () => import('./profile/profile.component').then((m) => m.ProfileComponent),
  },
  {
    path: 'patients',
    loadChildren: () => import('./patients/patients.routes').then((m) => m.patientRoutes),
  },
  {
    path: 'appointments',
    loadComponent: () =>
      import('./sections/section-placeholder.component').then((m) => m.SectionPlaceholderComponent),
    data: { titleKey: 'nav.appointments' },
  },
  {
    path: 'clinical',
    loadComponent: () =>
      import('./sections/section-placeholder.component').then((m) => m.SectionPlaceholderComponent),
    data: { titleKey: 'nav.clinical' },
  },
  {
    path: 'staff',
    loadComponent: () =>
      import('./sections/section-placeholder.component').then((m) => m.SectionPlaceholderComponent),
    data: { titleKey: 'nav.staff' },
  },
  {
    path: 'finance',
    loadComponent: () =>
      import('./sections/section-placeholder.component').then((m) => m.SectionPlaceholderComponent),
    data: { titleKey: 'nav.finance' },
  },
  {
    path: 'inventory',
    loadComponent: () =>
      import('./sections/section-placeholder.component').then((m) => m.SectionPlaceholderComponent),
    data: { titleKey: 'nav.inventory' },
  },
  {
    path: 'reports',
    loadComponent: () =>
      import('./sections/section-placeholder.component').then((m) => m.SectionPlaceholderComponent),
    data: { titleKey: 'nav.reports' },
  },
  {
    path: 'settings',
    loadComponent: () =>
      import('./sections/section-placeholder.component').then((m) => m.SectionPlaceholderComponent),
    data: { titleKey: 'nav.settings' },
  },
];
