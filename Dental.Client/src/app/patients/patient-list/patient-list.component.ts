import { CommonModule } from '@angular/common';
import { Component, inject, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { EducationLevel, Patient } from '../../models/patient';
import { PatientService } from '../patient.service';

@Component({
  selector: 'app-patient-list',
  imports: [CommonModule, RouterLink, TranslatePipe],
  templateUrl: './patient-list.component.html',
  styleUrl: './patient-list.component.scss',
})
export class PatientListComponent implements OnInit {
  private readonly patientsApi = inject(PatientService);
  private readonly translate = inject(TranslateService);

  readonly rows = signal<Patient[]>([]);
  readonly loadFailed = signal(false);

  ngOnInit(): void {
    this.refresh();
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

  delete(id: string, event: Event): void {
    event.preventDefault();
    event.stopPropagation();
    const ok =
      typeof globalThis.confirm === 'function'
        ? globalThis.confirm(this.translate.instant('patients.confirmDelete'))
        : true;
    if (!ok) {
      return;
    }

    this.patientsApi.delete(id).subscribe({
      next: () => this.refresh(),
    });
  }

  refresh(): void {
    this.loadFailed.set(false);
    this.patientsApi.list().subscribe({
      next: (data) => this.rows.set(data),
      error: () => {
        this.rows.set([]);
        this.loadFailed.set(true);
      },
    });
  }
}
