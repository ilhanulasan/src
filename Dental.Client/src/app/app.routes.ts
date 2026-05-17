import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', redirectTo: 'patients', pathMatch: 'full' },
  {
    path: 'patients',
    loadChildren: () => import('./patients/patients.routes').then((m) => m.patientRoutes),
  },
];
