import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { Appointment, AppointmentDensity, AppointmentResource, WaitlistEntry } from '../models/appointments';

@Injectable({ providedIn: 'root' })
export class AppointmentService {
  private readonly http = inject(HttpClient);

  list(params?: { from?: string; to?: string; resourceId?: string }): Observable<Appointment[]> {
    let p = new HttpParams();
    if (params?.from) p = p.set('from', params.from);
    if (params?.to) p = p.set('to', params.to);
    if (params?.resourceId) p = p.set('resourceId', params.resourceId);
    return this.http.get<Appointment[]>('/api/appointments', { params: p });
  }

  resources(): Observable<AppointmentResource[]> {
    return this.http.get<AppointmentResource[]>('/api/appointment-resources');
  }

  create(body: unknown): Observable<Appointment> {
    return this.http.post<Appointment>('/api/appointments', body);
  }

  confirm(id: string): Observable<void> {
    return this.http.post<void>(`/api/appointments/${id}/confirm`, {});
  }

  cancel(id: string, reason?: string): Observable<void> {
    return this.http.post<void>(`/api/appointments/${id}/cancel`, { reason });
  }

  density(from: string, to: string): Observable<AppointmentDensity[]> {
    const params = new HttpParams().set('from', from).set('to', to);
    return this.http.get<AppointmentDensity[]>('/api/appointments/analytics/density', { params });
  }

  waitlist(): Observable<WaitlistEntry[]> {
    return this.http.get<WaitlistEntry[]>('/api/waitlist');
  }
}
