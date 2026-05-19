import { Component, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';
import { forkJoin, of } from 'rxjs';
import { catchError, debounceTime, distinctUntilChanged, switchMap } from 'rxjs';
import { Icd10Code } from '../models/clinical';
import { Patient } from '../models/patient';
import { PatientService } from '../patients/patient.service';
import { ClinicalService } from './clinical.service';

@Component({
  selector: 'app-examination-form',
  imports: [ReactiveFormsModule, RouterLink, TranslatePipe],
  templateUrl: './examination-form.component.html',
  styleUrl: './examination-form.component.scss',
})
export class ExaminationFormComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly clinical = inject(ClinicalService);
  private readonly patientsApi = inject(PatientService);

  readonly patients = signal<Patient[]>([]);
  readonly icdResults = signal<Icd10Code[]>([]);
  readonly selectedIcd = signal<Icd10Code[]>([]);
  readonly errorKey = signal<string | undefined>(undefined);
  readonly savedExamId = signal<string | undefined>(undefined);

  readonly form = this.fb.nonNullable.group({
    patientId: ['', Validators.required],
    chiefComplaint: ['', Validators.required],
    clinicalFindings: [''],
    notes: [''],
    status: ['Draft'],
    icdSearch: [''],
  });

  ngOnInit(): void {
    const patientId = this.route.snapshot.queryParamMap.get('patientId');
    if (patientId) this.form.patchValue({ patientId });

    this.patientsApi.list().subscribe({ next: (p) => this.patients.set(p) });

    this.form.controls.icdSearch.valueChanges
      .pipe(
        debounceTime(300),
        distinctUntilChanged(),
        switchMap((q) => this.clinical.icd10Search(q.trim())),
      )
      .subscribe({ next: (codes) => this.icdResults.set(codes) });
  }

  addIcd(code: Icd10Code): void {
    if (this.selectedIcd().some((c) => c.id === code.id)) return;
    this.selectedIcd.update((list) => [...list, code]);
    this.form.patchValue({ icdSearch: '' });
    this.icdResults.set([]);
  }

  removeIcd(id: string): void {
    this.selectedIcd.update((list) => list.filter((c) => c.id !== id));
  }

  save(): void {
    this.errorKey.set(undefined);
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const v = this.form.getRawValue();
    this.clinical
      .createExamination({
        patientId: v.patientId,
        chiefComplaint: v.chiefComplaint,
        clinicalFindings: v.clinicalFindings || null,
        notes: v.notes || null,
        status: v.status,
        examinedAt: new Date().toISOString(),
      })
      .subscribe({
        next: (exam) => {
          this.savedExamId.set(exam.id);
          const icdList = this.selectedIcd();
          if (icdList.length === 0) {
            void this.router.navigate(['/clinical']);
            return;
          }

          forkJoin(
            icdList.map((code, idx) =>
              this.clinical.addDiagnosis(exam.id, { icd10CodeId: code.id, isPrimary: idx === 0 }).pipe(
                catchError(() => of(null)),
              ),
            ),
          ).subscribe({
            next: (results) => {
              const failed = results.filter((r) => r === null).length;
              if (failed > 0) {
                this.errorKey.set('clinical.diagnosisPartialError');
              }
              void this.router.navigate(['/clinical']);
            },
          });
        },
        error: () => this.errorKey.set('common.loadError'),
      });
  }
}
