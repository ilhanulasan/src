import { Routes } from '@angular/router';

import { OpenDentalPatientDetailComponent } from './open-dental-patient-detail.component';
import { OpenDentalPatientListComponent } from './open-dental-patient-list.component';

export const openDentalRoutes: Routes = [
  { path: '', component: OpenDentalPatientListComponent },
  { path: 'patient/:patNum', component: OpenDentalPatientDetailComponent },
];
