import { DecimalPipe } from '@angular/common';
import { Component, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';
import {
  EducationLevel,
  Patient,
  PatientAllergy,
  PatientBalance,
  PatientClinicalNote,
  PatientDocument,
  PatientKvkkConsent,
  PatientMedicalHistory,
} from '../../models/patient';
import { PatientClinicalService } from '../patient-clinical.service';
import { PatientService } from '../patient.service';

@Component({
  selector: 'app-patient-detail',
  imports: [RouterLink, TranslatePipe, DecimalPipe],
  templateUrl: './patient-detail.component.html',
  styleUrl: './patient-detail.component.scss',
})
export class PatientDetailComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly patientsApi = inject(PatientService);
  private readonly clinicalApi = inject(PatientClinicalService);

  readonly id = this.route.snapshot.paramMap.get('id')!;
  readonly patient = signal<Patient | undefined>(undefined);
  readonly allergies = signal<PatientAllergy[]>([]);
  readonly histories = signal<PatientMedicalHistory[]>([]);
  readonly notes = signal<PatientClinicalNote[]>([]);
  readonly consents = signal<PatientKvkkConsent[]>([]);
  readonly documents = signal<PatientDocument[]>([]);
  readonly balance = signal<PatientBalance | undefined>(undefined);
  readonly activeTab = signal<'overview' | 'clinical' | 'documents' | 'kvkk' | 'finance'>('overview');
  readonly loading = signal(true);
  readonly failed = signal(false);

  ngOnInit(): void {
    this.patientsApi.get(this.id).subscribe({
      next: (p) => {
        this.patient.set(p);
        this.loading.set(false);
        this.loadClinical();
      },
      error: () => {
        this.failed.set(true);
        this.loading.set(false);
      },
    });
  }

  loadClinical(): void {
    this.clinicalApi.allergies(this.id).subscribe({ next: (d) => this.allergies.set(d) });
    this.clinicalApi.medicalHistories(this.id).subscribe({ next: (d) => this.histories.set(d) });
    this.clinicalApi.clinicalNotes(this.id).subscribe({ next: (d) => this.notes.set(d) });
    this.clinicalApi.kvkkConsents(this.id).subscribe({ next: (d) => this.consents.set(d) });
    this.clinicalApi.documents(this.id).subscribe({ next: (d) => this.documents.set(d) });
    this.clinicalApi.balance(this.id).subscribe({ next: (d) => this.balance.set(d) });
  }

  setTab(tab: 'overview' | 'clinical' | 'documents' | 'kvkk' | 'finance'): void {
    this.activeTab.set(tab);
  }

  grantKvkk(): void {
    this.clinicalApi
      .recordKvkk(this.id, { consentType: 'DataProcessing', isGranted: true, consentVersion: '1.0' })
      .subscribe({ next: () => this.clinicalApi.kvkkConsents(this.id).subscribe({ next: (d) => this.consents.set(d) }) });
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;
    this.clinicalApi.uploadDocument(this.id, file, 'Medical').subscribe({
      next: () => this.clinicalApi.documents(this.id).subscribe({ next: (d) => this.documents.set(d) }),
    });
    input.value = '';
  }

  eduKey(level: EducationLevel): string {
    const map: Record<EducationLevel, string> = {
      [EducationLevel.ElementarySchool]: 'edu.elementarySchool',
      [EducationLevel.HighSchool]: 'edu.highSchool',
      [EducationLevel.Graduate]: 'edu.graduate',
      [EducationLevel.Masters]: 'edu.masters',
      [EducationLevel.Phd]: 'edu.phd',
    };
    return map[level] ?? '';
  }

  genderKey(raw: string): string {
    const k = raw.toLowerCase();
    if (k === 'male') return 'gender.male';
    if (k === 'female') return 'gender.female';
    return 'gender.other';
  }
}
