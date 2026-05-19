import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { Appointment, AppointmentResource, TimeSlot } from '../models/appointments';
import { Patient } from '../models/patient';

export type { TimeSlot };

@Injectable({ providedIn: 'root' })
export class PortalService {
  private readonly http = inject(HttpClient);
  private readonly api = '/api/portal';

  patient(): Observable<Patient> {
    return this.http.get<Patient>(`${this.api}/patient`);
  }

  link(body: {
    socialSecurityNumber?: string;
    createFromProfile: boolean;
    firstName?: string;
    lastName?: string;
    phone?: string;
    email?: string;
    dateOfBirth?: string;
    gender?: string;
  }): Observable<Patient> {
    return this.http.post<Patient>(`${this.api}/link`, body);
  }

  resources(): Observable<AppointmentResource[]> {
    return this.http.get<AppointmentResource[]>(`${this.api}/resources`);
  }

  availability(resourceId: string, date: string): Observable<TimeSlot[]> {
    const params = new HttpParams().set('resourceId', resourceId).set('date', date);
    return this.http.get<TimeSlot[]>(`${this.api}/availability`, { params });
  }

  book(body: { resourceId: string; startAt: string; endAt: string; notes?: string }): Observable<Appointment> {
    return this.http.post<Appointment>(`${this.api}/book`, body);
  }

  myAppointments(): Observable<Appointment[]> {
    return this.http.get<Appointment[]>(`${this.api}/appointments`);
  }

  cancelAppointment(id: string): Observable<void> {
    return this.http.post<void>(`${this.api}/appointments/${id}/cancel`, {});
  }
}
