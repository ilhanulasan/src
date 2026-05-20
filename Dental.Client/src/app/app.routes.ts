import { Routes } from '@angular/router';

import { authGuard } from './core/auth.guard';
import { guestGuard } from './core/guest.guard';
import { roleGuard } from './core/role.guard';
import { AdminOnlyRoles, AppRoles, PatientPortalRoles, StaffRoles } from './core/roles';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () => import('./website/public-layout.component').then((m) => m.PublicLayoutComponent),
    children: [
      {
        path: '',
        loadComponent: () => import('./website/landing.component').then((m) => m.LandingComponent),
      },
      {
        path: 'login',
        canActivate: [guestGuard],
        loadComponent: () => import('./auth/login/login.component').then((m) => m.LoginComponent),
      },
      {
        path: 'register',
        canActivate: [guestGuard],
        loadComponent: () => import('./auth/register/register.component').then((m) => m.RegisterComponent),
      },
    ],
  },
  {
    path: '',
    loadComponent: () => import('./layout/main-layout.component').then((m) => m.MainLayoutComponent),
    canActivate: [authGuard],
    children: [
      {
        path: 'dashboard',
        canActivate: [roleGuard(AppRoles.Admin)],
        loadComponent: () => import('./home/home.component').then((m) => m.HomeComponent),
      },
      {
        path: 'profile',
        loadComponent: () => import('./profile/profile.component').then((m) => m.ProfileComponent),
      },
      {
        path: 'patients',
        canActivate: [roleGuard(...StaffRoles)],
        loadChildren: () => import('./patients/patients.routes').then((m) => m.patientRoutes),
      },
      {
        path: 'appointments',
        canActivate: [roleGuard(...StaffRoles)],
        loadChildren: () => import('./appointments/appointments.routes').then((m) => m.appointmentRoutes),
      },
      {
        path: 'clinical',
        canActivate: [roleGuard(...StaffRoles)],
        loadChildren: () => import('./clinical/clinical.routes').then((m) => m.clinicalRoutes),
      },
      {
        path: 'portal',
        canActivate: [roleGuard(...PatientPortalRoles)],
        loadChildren: () => import('./portal/portal.routes').then((m) => m.portalRoutes),
      },
      {
        path: 'staff',
        canActivate: [roleGuard(...AdminOnlyRoles)],
        loadChildren: () => import('./staff/staff.routes').then((m) => m.staffRoutes),
      },
      {
        path: 'finance',
        canActivate: [roleGuard(...AdminOnlyRoles)],
        loadComponent: () =>
          import('./finance/finance-dashboard.component').then((m) => m.FinanceDashboardComponent),
        data: { titleKey: 'nav.finance' },
      },
      {
        path: 'inventory',
        canActivate: [roleGuard(...AdminOnlyRoles)],
        loadComponent: () =>
          import('./inventory/inventory-dashboard.component').then((m) => m.InventoryDashboardComponent),
        data: { titleKey: 'nav.inventory' },
      },
      {
        path: 'reports',
        canActivate: [roleGuard(...AdminOnlyRoles)],
        loadComponent: () =>
          import('./sections/section-placeholder.component').then((m) => m.SectionPlaceholderComponent),
        data: { titleKey: 'nav.reports' },
      },
      {
        path: 'settings',
        canActivate: [roleGuard(...AdminOnlyRoles)],
        loadComponent: () =>
          import('./sections/section-placeholder.component').then((m) => m.SectionPlaceholderComponent),
        data: { titleKey: 'nav.settings' },
      },
    ],
  },
];
