import { Routes } from '@angular/router';

import { OdontogramEditorComponent } from '../odontogram/odontogram-editor/odontogram-editor.component';

import { PatientDetailComponent } from './patient-detail/patient-detail.component';
import { PatientFormComponent } from './patient-form/patient-form.component';
import { PatientListComponent } from './patient-list/patient-list.component';

export const patientRoutes: Routes = [
  { path: '', component: PatientListComponent },
  { path: 'new', component: PatientFormComponent },
  { path: ':id/edit', component: PatientFormComponent },
  { path: ':id/odontogram', component: OdontogramEditorComponent },
  { path: ':id', component: PatientDetailComponent },
];
