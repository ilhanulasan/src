import { CommonModule } from '@angular/common';
import { Component, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';
import { EducationLevel, Patient } from '../../models/patient';
import { PatientService } from '../patient.service';

@Component({
  selector: 'app-patient-detail',
  imports: [CommonModule, RouterLink, TranslatePipe],
  templateUrl: './patient-detail.component.html',
  styleUrl: './patient-detail.component.scss',
})
export class PatientDetailComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly patientsApi = inject(PatientService);

  readonly id = this.route.snapshot.paramMap.get('id')!;
  readonly patient = signal<Patient | undefined>(undefined);
  readonly loading = signal(true);
  readonly failed = signal(false);

  ngOnInit(): void {
    this.patientsApi.get(this.id).subscribe({
      next: (p) => {
        this.patient.set(p);
        this.loading.set(false);
      },
      error: () => {
        this.failed.set(true);
        this.loading.set(false);
      },
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

  genderKey(raw: string): string {
    const k = raw.toLowerCase();
    if (k === 'male') return 'gender.male';
    if (k === 'female') return 'gender.female';
    return 'gender.other';
  }
}
