import { CommonModule } from '@angular/common';
import { Component, inject, OnInit, signal } from '@angular/core';
import {
  FormBuilder,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';
import { passwordPolicyValidator } from '../../auth/register/password-policy.validator';
import { AppRoles } from '../../core/roles';
import { StaffService } from '../staff.service';

@Component({
  selector: 'app-staff-form',
  imports: [CommonModule, ReactiveFormsModule, RouterLink, TranslatePipe],
  templateUrl: './staff-form.component.html',
  styleUrl: './staff-form.component.scss',
})
export class StaffFormComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly staffApi = inject(StaffService);

  readonly editingId = this.route.snapshot.paramMap.get('id');
  readonly isEdit = !!this.editingId;
  readonly titleKey = signal(this.isEdit ? 'staff.editTitle' : 'staff.createTitle');
  readonly submitErrorKey = signal<string | undefined>(undefined);
  readonly assignableRoles = [
    AppRoles.Admin,
    AppRoles.Patient,
    AppRoles.Doctor,
    AppRoles.Nurse,
    AppRoles.Technician,
    AppRoles.Finance,
  ];

  readonly form = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    firstName: ['', Validators.required],
    lastName: ['', Validators.required],
    phoneNumber: [''],
    address: [''],
    password: ['', [Validators.required, passwordPolicyValidator]],
    confirmPassword: ['', Validators.required],
    roles: this.fb.nonNullable.control<string[]>([], [Validators.required, Validators.minLength(1)]),
    newPassword: ['', passwordPolicyValidator],
    confirmNewPassword: [''],
  });

  ngOnInit(): void {
    if (this.isEdit) {
      this.form.controls.password.clearValidators();
      this.form.controls.password.updateValueAndValidity();
      this.form.controls.confirmPassword.clearValidators();
      this.form.controls.confirmPassword.updateValueAndValidity();
    }

    const id = this.editingId;
    if (!id) {
      return;
    }

    this.staffApi.get(id).subscribe({
      next: (u) => {
        this.form.patchValue({
          email: u.email,
          firstName: u.firstName,
          lastName: u.lastName,
          phoneNumber: u.phoneNumber ?? '',
          address: u.address ?? '',
          roles: [...u.roles],
        });
      },
      error: () => this.router.navigate(['/staff']),
    });
  }

  roleKey(role: string): string {
    return `staff.role.${role}`;
  }

  isRoleChecked(role: string): boolean {
    return this.form.controls.roles.value.includes(role);
  }

  toggleRole(role: string, checked: boolean): void {
    const current = this.form.controls.roles.value;
    const next = checked ? [...current, role] : current.filter((r) => r !== role);
    this.form.controls.roles.setValue(next);
    this.form.controls.roles.markAsTouched();
  }

  save(): void {
    this.submitErrorKey.set(undefined);

    if (!this.isEdit && this.form.controls.password.value !== this.form.controls.confirmPassword.value) {
      this.submitErrorKey.set('auth.passwordMismatch');
      return;
    }

    if (this.isEdit) {
      const newPw = this.form.controls.newPassword.value;
      const confirmNew = this.form.controls.confirmNewPassword.value;
      if (newPw || confirmNew) {
        if (newPw !== confirmNew) {
          this.submitErrorKey.set('auth.passwordMismatch');
          return;
        }
        if (this.form.controls.newPassword.invalid) {
          this.form.controls.newPassword.markAsTouched();
          return;
        }
      }
    }

    if (this.form.controls.roles.invalid) {
      this.form.controls.roles.markAsTouched();
    }

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const v = this.form.getRawValue();

    if (this.isEdit && this.editingId) {
      this.staffApi
        .update(this.editingId, {
          email: v.email,
          firstName: v.firstName,
          lastName: v.lastName,
          phoneNumber: v.phoneNumber || null,
          address: v.address || null,
          roles: v.roles,
        })
        .subscribe({
          next: () => {
            const newPw = v.newPassword;
            if (newPw) {
              this.staffApi.resetPassword(this.editingId!, { newPassword: newPw }).subscribe({
                next: () => this.router.navigate(['/staff']),
                error: () => this.submitErrorKey.set('staff.saveError'),
              });
            } else {
              this.router.navigate(['/staff']);
            }
          },
          error: () => this.submitErrorKey.set('staff.saveError'),
        });
      return;
    }

    this.staffApi
      .create({
        email: v.email,
        password: v.password,
        firstName: v.firstName,
        lastName: v.lastName,
        phoneNumber: v.phoneNumber || null,
        address: v.address || null,
        roles: v.roles,
      })
      .subscribe({
        next: () => this.router.navigate(['/staff']),
        error: () => this.submitErrorKey.set('staff.saveError'),
      });
  }
}
