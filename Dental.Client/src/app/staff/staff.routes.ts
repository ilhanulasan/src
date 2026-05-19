import { Routes } from '@angular/router';

import { StaffFormComponent } from './staff-form/staff-form.component';
import { StaffListComponent } from './staff-list/staff-list.component';

export const staffRoutes: Routes = [
  { path: '', component: StaffListComponent },
  { path: 'new', component: StaffFormComponent },
  { path: ':id/edit', component: StaffFormComponent },
];
