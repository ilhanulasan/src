import { CommonModule } from '@angular/common';
import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';
import {
  ALL_DENTAL_SPECIALTIES,
  ALL_PERSONNEL_TYPES,
  DentalSpecialty,
  PersonnelType,
  PersonnelTypes,
} from '../../models/personnel';
import { PersonnelService } from '../personnel.service';

@Component({
  selector: 'app-personnel-form',
  imports: [CommonModule, ReactiveFormsModule, RouterLink, TranslatePipe],
  templateUrl: './personnel-form.component.html',
  styleUrl: './personnel-form.component.scss',
})
export class PersonnelFormComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly personnelApi = inject(PersonnelService);

  readonly editingId = this.route.snapshot.paramMap.get('id');
  readonly isEdit = !!this.editingId;
  readonly titleKey = signal(this.isEdit ? 'personnel.editTitle' : 'personnel.createTitle');
  readonly submitErrorKey = signal<string | undefined>(undefined);
  readonly personnelTypes = ALL_PERSONNEL_TYPES;
  readonly dentalSpecialties = ALL_DENTAL_SPECIALTIES;
  readonly PersonnelTypes = PersonnelTypes;

  readonly form = this.fb.nonNullable.group({
    firstName: ['', Validators.required],
    lastName: ['', Validators.required],
    email: [''],
    phone: [''],
    notes: [''],
    personnelType: [PersonnelTypes.Doctor as PersonnelType, Validators.required],
    specialties: this.fb.nonNullable.control<DentalSpecialty[]>([]),
    isActive: [true],
  });

  readonly isDoctor = computed(() => this.form.controls.personnelType.value === PersonnelTypes.Doctor);

  ngOnInit(): void {
    const id = this.editingId;
    if (!id) {
      return;
    }

    this.personnelApi.get(id).subscribe({
      next: (p) => {
        this.form.patchValue({
          firstName: p.firstName,
          lastName: p.lastName,
          email: p.email ?? '',
          phone: p.phone ?? '',
          notes: p.notes ?? '',
          personnelType: p.personnelType,
          specialties: [...p.specialties],
          isActive: p.isActive,
        });
      },
      error: () => this.router.navigate(['/personnel']),
    });
  }

  typeKey(type: PersonnelType): string {
    return `personnel.type.${type}`;
  }

  specialtyKey(specialty: DentalSpecialty): string {
    return `personnel.specialty.${specialty}`;
  }

  isSpecialtyChecked(specialty: DentalSpecialty): boolean {
    return this.form.controls.specialties.value.includes(specialty);
  }

  toggleSpecialty(specialty: DentalSpecialty, checked: boolean): void {
    const current = this.form.controls.specialties.value;
    const next = checked ? [...current, specialty] : current.filter((s) => s !== specialty);
    this.form.controls.specialties.setValue(next);
  }

  onTypeChange(): void {
    if (!this.isDoctor()) {
      this.form.controls.specialties.setValue([]);
    }
  }

  save(): void {
    this.submitErrorKey.set(undefined);
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const v = this.form.getRawValue();
    const payload = {
      firstName: v.firstName,
      lastName: v.lastName,
      email: v.email || null,
      phone: v.phone || null,
      notes: v.notes || null,
      personnelType: v.personnelType,
      specialties: this.isDoctor() ? v.specialties : [],
      isActive: v.isActive,
    };

    if (this.isEdit && this.editingId) {
      this.personnelApi
        .update(this.editingId, { id: this.editingId, ...payload })
        .subscribe({
          next: () => this.router.navigate(['/personnel']),
          error: () => this.submitErrorKey.set('personnel.saveError'),
        });
      return;
    }

    this.personnelApi.create(payload).subscribe({
      next: () => this.router.navigate(['/personnel']),
      error: () => this.submitErrorKey.set('personnel.saveError'),
    });
  }
}
