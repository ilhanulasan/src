import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { Patient } from '../models/patient';

@Injectable({ providedIn: 'root' })
export class PatientService {
  private readonly http = inject(HttpClient);
  private readonly api = '/api/patients';

  list(): Observable<Patient[]> {
    return this.http.get<Patient[]>(this.api);
  }

  get(id: string): Observable<Patient> {
    return this.http.get<Patient>(`${this.api}/${id}`);
  }

  create(patient: Omit<Patient, 'id'>): Observable<Patient> {
    return this.http.post<Patient>(this.api, patient);
  }

  update(id: string, patient: Patient): Observable<void> {
    return this.http.put<void>(`${this.api}/${id}`, patient);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.api}/${id}`);
  }
}
