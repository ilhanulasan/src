import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { AuthResponse } from '../models/auth';
import { TimeSlot } from '../models/appointments';
import { DoctorAppointmentOption } from '../models/personnel';

export interface GuestBookRequest {
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
  resourceId: string;
  startAt: string;
  endAt: string;
  notes?: string | null;
  dateOfBirth?: string | null;
  gender?: string | null;
  preferTurkish: boolean;
}

export interface GuestBookResponse {
  appointmentId: string;
  patientId: string;
  isNewPatient: boolean;
  needsRegistration: boolean;
  messageKey: string;
}

export interface RegistrationInviteInfo {
  firstName: string;
  lastName: string;
  email: string;
  phone?: string | null;
}

@Injectable({ providedIn: 'root' })
export class PublicBookingService {
  private readonly http = inject(HttpClient);
  private readonly api = '/api/public/booking';
  private readonly authApi = '/api/auth';

  doctors(): Observable<DoctorAppointmentOption[]> {
    return this.http.get<DoctorAppointmentOption[]>(`${this.api}/doctors`);
  }

  availability(resourceId: string, date: string): Observable<TimeSlot[]> {
    const params = new HttpParams().set('resourceId', resourceId).set('date', date);
    return this.http.get<TimeSlot[]>(`${this.api}/availability`, { params });
  }

  book(body: GuestBookRequest): Observable<GuestBookResponse> {
    return this.http.post<GuestBookResponse>(this.api, body);
  }

  confirm(appointmentId: string, token: string): Observable<void> {
    return this.http.post<void>(`${this.api}/confirm`, { appointmentId, token });
  }

  getInvite(token: string): Observable<RegistrationInviteInfo> {
    return this.http.get<RegistrationInviteInfo>(`${this.authApi}/invite/${encodeURIComponent(token)}`);
  }

  completeInvite(
    token: string,
    body: { password: string; email?: string | null; phone?: string | null },
  ): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(
      `${this.authApi}/invite/${encodeURIComponent(token)}/complete`,
      body,
    );
  }
}
