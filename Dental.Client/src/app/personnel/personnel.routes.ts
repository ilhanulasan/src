import { Routes } from '@angular/router';

import { PersonnelFormComponent } from './personnel-form/personnel-form.component';
import { PersonnelListComponent } from './personnel-list/personnel-list.component';

export const personnelRoutes: Routes = [
  { path: '', component: PersonnelListComponent },
  { path: 'new', component: PersonnelFormComponent },
  { path: ':id/edit', component: PersonnelFormComponent },
];
