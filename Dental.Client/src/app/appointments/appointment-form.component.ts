import { DatePipe } from '@angular/common';
import { Component, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';
import { AppointmentResource } from '../models/appointments';
import { Patient } from '../models/patient';
import { TimeSlot } from '../models/appointments';
import { addMinutesToOffsetIso, localDateTimePartsToOffsetIso } from '../shared/datetime.util';
import { PatientService } from '../patients/patient.service';
import { AppointmentService } from './appointment.service';

@Component({
  selector: 'app-appointment-form',
  imports: [ReactiveFormsModule, RouterLink, TranslatePipe, DatePipe],
  templateUrl: './appointment-form.component.html',
  styleUrl: './appointment-form.component.scss',
})
export class AppointmentFormComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly router = inject(Router);
  private readonly api = inject(AppointmentService);
  private readonly patientsApi = inject(PatientService);

  readonly patients = signal<Patient[]>([]);
  readonly resources = signal<AppointmentResource[]>([]);
  readonly slots = signal<TimeSlot[]>([]);
  readonly errorKey = signal<string | undefined>(undefined);
  readonly selectedSlotStart = signal<string>('');

  readonly form = this.fb.nonNullable.group({
    patientId: ['', Validators.required],
    primaryResourceId: ['', Validators.required],
    date: ['', Validators.required],
    startTime: ['09:00', Validators.required],
    durationMinutes: [30, [Validators.required, Validators.min(15)]],
    notes: [''],
    scheduleSmsReminder: [true],
  });

  ngOnInit(): void {
    const today = new Date().toISOString().slice(0, 10);
    this.form.patchValue({ date: today });

    this.patientsApi.list().subscribe({ next: (p) => this.patients.set(p) });
    this.api.resources().subscribe({ next: (r) => this.resources.set(r) });
  }

  onScheduleParamsChange(): void {
    const v = this.form.getRawValue();
    this.selectedSlotStart.set('');
    if (!v.primaryResourceId || !v.date) {
      this.slots.set([]);
      return;
    }

    this.api.availability(v.primaryResourceId, v.date).subscribe({
      next: (s) => this.slots.set(s),
      error: () => this.slots.set([]),
    });
  }

  pickSlot(slot: TimeSlot): void {
    this.selectedSlotStart.set(slot.startAt);
    const d = new Date(slot.startAt);
    const pad = (n: number) => String(n).padStart(2, '0');
    this.form.patchValue({
      startTime: `${pad(d.getHours())}:${pad(d.getMinutes())}`,
      durationMinutes: Math.round((new Date(slot.endAt).getTime() - d.getTime()) / 60_000),
    });
  }

  save(): void {
    this.errorKey.set(undefined);
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const v = this.form.getRawValue();
    const picked = this.slots().find((s) => s.startAt === this.selectedSlotStart());
    const startAt = picked?.startAt ?? localDateTimePartsToOffsetIso(v.date, v.startTime);
    const endAt =
      picked?.endAt ??
      addMinutesToOffsetIso(localDateTimePartsToOffsetIso(v.date, v.startTime), v.durationMinutes);

    this.api
      .create({
        patientId: v.patientId,
        primaryResourceId: v.primaryResourceId,
        startAt,
        endAt,
        notes: v.notes || null,
        isOnlineBooking: false,
        scheduleSmsReminder: v.scheduleSmsReminder,
        additionalResourceIds: [],
      })
      .subscribe({
        next: () => void this.router.navigate(['/appointments']),
        error: (err) =>
          this.errorKey.set(err.status === 409 ? 'appointments.slotConflict' : 'common.loadError'),
      });
  }

  private addMinutesToTime(time: string, minutes: number): string {
    const [h, m] = time.split(':').map(Number);
    const total = h * 60 + m + minutes;
    const pad = (n: number) => String(n).padStart(2, '0');
    return `${pad(Math.floor(total / 60) % 24)}:${pad(total % 60)}`;
  }
}
