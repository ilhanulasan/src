import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import {
  PatientAllergy,
  PatientBalance,
  PatientClinicalNote,
  PatientDocument,
  PatientKvkkConsent,
  PatientMedicalHistory,
} from '../models/patient';

@Injectable({ providedIn: 'root' })
export class PatientClinicalService {
  private readonly http = inject(HttpClient);

  private base(patientId: string): string {
    return `/api/patients/${patientId}`;
  }

  medicalHistories(patientId: string): Observable<PatientMedicalHistory[]> {
    return this.http.get<PatientMedicalHistory[]>(`${this.base(patientId)}/medical-histories`);
  }

  addMedicalHistory(patientId: string, body: Partial<PatientMedicalHistory>): Observable<PatientMedicalHistory> {
    return this.http.post<PatientMedicalHistory>(`${this.base(patientId)}/medical-histories`, body);
  }

  allergies(patientId: string): Observable<PatientAllergy[]> {
    return this.http.get<PatientAllergy[]>(`${this.base(patientId)}/allergies`);
  }

  addAllergy(patientId: string, body: Partial<PatientAllergy>): Observable<PatientAllergy> {
    return this.http.post<PatientAllergy>(`${this.base(patientId)}/allergies`, body);
  }

  clinicalNotes(patientId: string): Observable<PatientClinicalNote[]> {
    return this.http.get<PatientClinicalNote[]>(`${this.base(patientId)}/clinical-notes`);
  }

  addClinicalNote(patientId: string, body: Partial<PatientClinicalNote>): Observable<PatientClinicalNote> {
    return this.http.post<PatientClinicalNote>(`${this.base(patientId)}/clinical-notes`, body);
  }

  kvkkConsents(patientId: string): Observable<PatientKvkkConsent[]> {
    return this.http.get<PatientKvkkConsent[]>(`${this.base(patientId)}/kvkk-consents`);
  }

  recordKvkk(patientId: string, body: Partial<PatientKvkkConsent>): Observable<PatientKvkkConsent> {
    return this.http.post<PatientKvkkConsent>(`${this.base(patientId)}/kvkk-consents`, body);
  }

  documents(patientId: string): Observable<PatientDocument[]> {
    return this.http.get<PatientDocument[]>(`${this.base(patientId)}/documents`);
  }

  uploadDocument(patientId: string, file: File, category: string, description?: string): Observable<PatientDocument> {
    const form = new FormData();
    form.append('file', file);
    form.append('category', category);
    if (description) form.append('description', description);
    return this.http.post<PatientDocument>(`${this.base(patientId)}/documents`, form);
  }

  balance(patientId: string): Observable<PatientBalance> {
    return this.http.get<PatientBalance>(`${this.base(patientId)}/balance`);
  }
}
