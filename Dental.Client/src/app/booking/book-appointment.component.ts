import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { DoctorAppointmentOption } from '../models/personnel';
import { TimeSlot } from '../models/appointments';
import { ToothIconComponent } from '../shared/tooth-icon.component';
import { readStoredLang } from '../core/i18n.initializer';
import { PublicBookingService } from './public-booking.service';

@Component({
  selector: 'app-book-appointment',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [ReactiveFormsModule, RouterLink, TranslatePipe, DatePipe, ToothIconComponent],
  templateUrl: './book-appointment.component.html',
  styleUrl: './book-appointment.component.scss',
})
export class BookAppointmentComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly bookingApi = inject(PublicBookingService);
  private readonly router = inject(Router);
  private readonly translate = inject(TranslateService);

  readonly doctors = signal<DoctorAppointmentOption[]>([]);
  readonly slots = signal<TimeSlot[]>([]);
  readonly submitting = signal(false);
  readonly submitted = signal(false);
  readonly errorKey = signal<string | undefined>(undefined);
  readonly selectedSlotStart = signal('');

  readonly form = this.fb.nonNullable.group({
    firstName: ['', Validators.required],
    lastName: ['', Validators.required],
    email: ['', [Validators.required, Validators.email]],
    phone: ['', Validators.required],
    resourceId: ['', Validators.required],
    date: ['', Validators.required],
    notes: [''],
  });

  ngOnInit(): void {
    const today = new Date().toISOString().slice(0, 10);
    this.form.patchValue({ date: today });
    this.bookingApi.doctors().subscribe({ next: (d) => this.doctors.set(d) });
  }

  specialtyKey(specialty: string): string {
    return `personnel.specialty.${specialty}`;
  }

  doctorLabel(doctor: DoctorAppointmentOption): string {
    if (!doctor.specialties.length) {
      return doctor.displayName;
    }
    const labels = doctor.specialties.map((s) => this.translate.instant(this.specialtyKey(s)));
    return `${doctor.displayName} — ${labels.join(', ')}`;
  }

  onScheduleParamsChange(): void {
    this.selectedSlotStart.set('');
    const v = this.form.getRawValue();
    if (!v.resourceId || !v.date) {
      this.slots.set([]);
      return;
    }

    this.bookingApi.availability(v.resourceId, v.date).subscribe({
      next: (s) => this.slots.set(s),
      error: () => this.slots.set([]),
    });
  }

  pickSlot(slot: TimeSlot): void {
    this.selectedSlotStart.set(slot.startAt);
  }

  submit(): void {
    this.errorKey.set(undefined);
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const slot = this.slots().find((s) => s.startAt === this.selectedSlotStart());
    if (!slot) {
      this.errorKey.set('booking.pickSlot');
      return;
    }

    const v = this.form.getRawValue();
    this.submitting.set(true);

    this.bookingApi
      .book({
        firstName: v.firstName.trim(),
        lastName: v.lastName.trim(),
        email: v.email.trim(),
        phone: v.phone.trim(),
        resourceId: v.resourceId,
        startAt: slot.startAt,
        endAt: slot.endAt,
        notes: v.notes || null,
        preferTurkish: readStoredLang() === 'tr',
      })
      .subscribe({
        next: () => {
          this.submitting.set(false);
          this.submitted.set(true);
        },
        error: (err) => {
          this.submitting.set(false);
          this.errorKey.set(err.status === 409 ? 'appointments.slotConflict' : 'booking.failed');
        },
      });
  }
}
