import { DecimalPipe } from '@angular/common';
import { Component, inject, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';
import { Examination, TreatmentPlan } from '../models/clinical';
import { ClinicalService } from './clinical.service';

@Component({
  selector: 'app-clinical-workspace',
  imports: [RouterLink, TranslatePipe, DecimalPipe],
  templateUrl: './clinical-workspace.component.html',
  styleUrl: './clinical-workspace.component.scss',
})
export class ClinicalWorkspaceComponent implements OnInit {
  private readonly api = inject(ClinicalService);

  readonly examinations = signal<Examination[]>([]);
  readonly plans = signal<TreatmentPlan[]>([]);
  readonly loadFailed = signal(false);

  ngOnInit(): void {
    this.api.examinations().subscribe({
      next: (data) => this.examinations.set(data),
      error: () => this.loadFailed.set(true),
    });
    this.api.treatmentPlans().subscribe({
      next: (data) => this.plans.set(data),
    });
  }
}
