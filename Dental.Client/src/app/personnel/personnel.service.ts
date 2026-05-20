import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import {
  CreatePersonnelRequest,
  DoctorAppointmentOption,
  Personnel,
  PersonnelType,
  UpdatePersonnelRequest,
} from '../models/personnel';

@Injectable({ providedIn: 'root' })
export class PersonnelService {
  private readonly http = inject(HttpClient);
  private readonly api = '/api/personnel';

  list(params?: { type?: PersonnelType; activeOnly?: boolean }): Observable<Personnel[]> {
    let p = new HttpParams();
    if (params?.type) p = p.set('type', params.type);
    if (params?.activeOnly) p = p.set('activeOnly', 'true');
    return this.http.get<Personnel[]>(this.api, { params: p });
  }

  doctorsForAppointments(): Observable<DoctorAppointmentOption[]> {
    return this.http.get<DoctorAppointmentOption[]>(`${this.api}/doctors-for-appointments`);
  }

  get(id: string): Observable<Personnel> {
    return this.http.get<Personnel>(`${this.api}/${id}`);
  }

  create(body: CreatePersonnelRequest): Observable<Personnel> {
    return this.http.post<Personnel>(this.api, body);
  }

  update(id: string, body: UpdatePersonnelRequest): Observable<Personnel> {
    return this.http.put<Personnel>(`${this.api}/${id}`, body);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.api}/${id}`);
  }
}
