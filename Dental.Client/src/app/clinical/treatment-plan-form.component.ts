import { Component, inject, OnInit, signal } from '@angular/core';
import { FormArray, FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';
import { Patient } from '../models/patient';
import { PatientService } from '../patients/patient.service';
import { ClinicalService } from './clinical.service';

@Component({
  selector: 'app-treatment-plan-form',
  imports: [ReactiveFormsModule, RouterLink, TranslatePipe],
  templateUrl: './treatment-plan-form.component.html',
  styleUrl: './treatment-plan-form.component.scss',
})
export class TreatmentPlanFormComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly clinical = inject(ClinicalService);
  private readonly patientsApi = inject(PatientService);

  readonly patients = signal<Patient[]>([]);
  readonly errorKey = signal<string | undefined>(undefined);

  readonly form = this.fb.nonNullable.group({
    patientId: ['', Validators.required],
    title: ['', Validators.required],
    description: [''],
    status: ['Draft'],
    plannedStartDate: [''],
    plannedEndDate: [''],
    items: this.fb.array([this.newItemGroup()]),
  });

  ngOnInit(): void {
    const patientId = this.route.snapshot.queryParamMap.get('patientId');
    if (patientId) this.form.patchValue({ patientId });
    this.patientsApi.list().subscribe({ next: (p) => this.patients.set(p) });
  }

  get items(): FormArray {
    return this.form.get('items') as FormArray;
  }

  addItem(): void {
    this.items.push(this.newItemGroup());
  }

  removeItem(index: number): void {
    if (this.items.length > 1) this.items.removeAt(index);
  }

  save(): void {
    this.errorKey.set(undefined);
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const v = this.form.getRawValue();
    this.clinical
      .createTreatmentPlan({
        patientId: v.patientId,
        title: v.title,
        description: v.description || null,
        status: v.status,
        plannedStartDate: v.plannedStartDate || null,
        plannedEndDate: v.plannedEndDate || null,
        items: v.items.map((item, idx) => ({
          procedureName: item.procedureName,
          toothNumbers: item.toothNumbers || null,
          sortOrder: idx,
          status: 'Planned',
          unitPrice: Number(item.unitPrice),
          quantity: Number(item.quantity),
          notes: item.notes || null,
        })),
      })
      .subscribe({
        next: () => void this.router.navigate(['/clinical']),
        error: () => this.errorKey.set('common.loadError'),
      });
  }

  private newItemGroup() {
    return this.fb.nonNullable.group({
      procedureName: ['', Validators.required],
      toothNumbers: [''],
      unitPrice: [0, Validators.required],
      quantity: [1, Validators.required],
      notes: [''],
    });
  }
}
