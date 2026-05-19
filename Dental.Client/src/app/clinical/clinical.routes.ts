import { Routes } from '@angular/router';

import { ClinicalWorkspaceComponent } from './clinical-workspace.component';
import { ExaminationFormComponent } from './examination-form.component';
import { TreatmentPlanFormComponent } from './treatment-plan-form.component';

export const clinicalRoutes: Routes = [
  { path: '', component: ClinicalWorkspaceComponent },
  { path: 'examinations/new', component: ExaminationFormComponent },
  { path: 'treatment-plans/new', component: TreatmentPlanFormComponent },
];
