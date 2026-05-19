import { Routes } from '@angular/router';

import { AppointmentCalendarComponent } from './appointment-calendar.component';
import { AppointmentFormComponent } from './appointment-form.component';
import { AppointmentListComponent } from './appointment-list.component';

export const appointmentRoutes: Routes = [
  { path: '', component: AppointmentCalendarComponent },
  { path: 'list', component: AppointmentListComponent },
  { path: 'new', component: AppointmentFormComponent },
];
