import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { Examination, Icd10Code, TreatmentPlan } from '../models/clinical';

@Injectable({ providedIn: 'root' })
export class ClinicalService {
  private readonly http = inject(HttpClient);

  examinations(patientId?: string): Observable<Examination[]> {
    let params = new HttpParams();
    if (patientId) params = params.set('patientId', patientId);
    return this.http.get<Examination[]>('/api/examinations', { params });
  }

  icd10Search(q: string): Observable<Icd10Code[]> {
    return this.http.get<Icd10Code[]>('/api/icd10', { params: { q } });
  }

  treatmentPlans(patientId?: string): Observable<TreatmentPlan[]> {
    let params = new HttpParams();
    if (patientId) params = params.set('patientId', patientId);
    return this.http.get<TreatmentPlan[]>('/api/treatment-plans', { params });
  }
}
