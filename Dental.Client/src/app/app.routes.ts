import { Routes } from '@angular/router';

import { authGuard } from './core/auth.guard';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () => import('./auth/login/login.component').then((m) => m.LoginComponent),
  },
  {
    path: 'register',
    loadComponent: () => import('./auth/register/register.component').then((m) => m.RegisterComponent),
  },
  {
    path: '',
    loadComponent: () => import('./layout/main-layout.component').then((m) => m.MainLayoutComponent),
    children: [
      {
        path: '',
        loadComponent: () => import('./home/home.component').then((m) => m.HomeComponent),
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
        path: 'integrations/opendental',
        loadChildren: () =>
          import('./integrations/opendental/opendental.routes').then((m) => m.openDentalRoutes),
      },
      {
        path: 'appointments',
        loadComponent: () =>
          import('./appointments/appointment-list.component').then((m) => m.AppointmentListComponent),
        data: { titleKey: 'nav.appointments' },
      },
      {
        path: 'clinical',
        loadComponent: () =>
          import('./clinical/clinical-workspace.component').then((m) => m.ClinicalWorkspaceComponent),
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
          import('./finance/finance-dashboard.component').then((m) => m.FinanceDashboardComponent),
        data: { titleKey: 'nav.finance' },
      },
      {
        path: 'inventory',
        loadComponent: () =>
          import('./inventory/inventory-dashboard.component').then((m) => m.InventoryDashboardComponent),
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
    ],
  },
];
