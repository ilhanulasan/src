import { CommonModule } from '@angular/common';
import { Component, inject, OnInit, signal } from '@angular/core';
import {
  FormBuilder,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';
import { EducationLevel } from '../../models/patient';
import { PatientService } from '../patient.service';

@Component({
  selector: 'app-patient-form',
  imports: [CommonModule, ReactiveFormsModule, RouterLink, TranslatePipe],
  templateUrl: './patient-form.component.html',
  styleUrl: './patient-form.component.scss',
})
export class PatientFormComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly patientsApi = inject(PatientService);

  readonly editingId = this.route.snapshot.paramMap.get('id');
  readonly isEdit = !!this.editingId;
  readonly titleKey = signal(this.isEdit ? 'patients.editTitle' : 'patients.createTitle');
  readonly submitErrorKey = signal<string | undefined>(undefined);
  readonly educationLevels = [
    EducationLevel.ElementarySchool,
    EducationLevel.HighSchool,
    EducationLevel.Graduate,
    EducationLevel.Masters,
    EducationLevel.Phd,
  ];

  readonly form = this.fb.nonNullable.group({
    name: ['', Validators.required],
    surname: ['', Validators.required],
    socialSecurityNumber: ['', Validators.required],
    address: [''],
    phone: [''],
    dateOfBirth: ['', Validators.required],
    gender: ['Male', Validators.required],
    education: [EducationLevel.Graduate, Validators.required],
  });

  ngOnInit(): void {
    const id = this.editingId;
    if (!id) {
      return;
    }

    this.patientsApi.get(id).subscribe({
      next: (p) => {
        this.form.patchValue({
          name: p.name,
          surname: p.surname,
          socialSecurityNumber: p.socialSecurityNumber,
          address: p.address ?? '',
          phone: p.phone ?? '',
          dateOfBirth: p.dateOfBirth,
          gender: p.gender,
          education: p.education as EducationLevel,
        });
      },
      error: () => this.router.navigate(['/patients']),
    });
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

  save(): void {
    this.submitErrorKey.set(undefined);
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const v = this.form.getRawValue();
    if (this.isEdit && this.editingId) {
      const body = {
        id: this.editingId,
        name: v.name,
        surname: v.surname,
        socialSecurityNumber: v.socialSecurityNumber,
        address: v.address || null,
        phone: v.phone || null,
        dateOfBirth: v.dateOfBirth,
        gender: v.gender,
        education: v.education,
      };

      this.patientsApi.update(this.editingId, body).subscribe({
        next: () => void this.router.navigate(['/patients', this.editingId]),
        error: (err) => this.submitErrorKey.set(this.mapConflict(err)),
      });
      return;
    }

    this.patientsApi
      .create({
        name: v.name,
        surname: v.surname,
        socialSecurityNumber: v.socialSecurityNumber,
        address: v.address || null,
        phone: v.phone || null,
        dateOfBirth: v.dateOfBirth,
        gender: v.gender,
        education: v.education,
      })
      .subscribe({
        next: (created) => void this.router.navigate(['/patients', created.id]),
        error: (err) => this.submitErrorKey.set(this.mapConflict(err)),
      });
  }

  private mapConflict(err: { status?: number }): string {
    return err.status === 409 ? 'patients.ssnTaken' : 'patients.loadError';
  }

  cancel(): void {
    if (this.isEdit && this.editingId) {
      void this.router.navigate(['/patients', this.editingId]);
    } else {
      void this.router.navigate(['/patients']);
    }
  }
}
