import { DatePipe } from '@angular/common';
import { Component, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';
import { Appointment, AppointmentResource } from '../models/appointments';
import { Patient } from '../models/patient';
import { AuthService } from '../core/auth.service';
import { PortalService, TimeSlot } from './portal.service';

@Component({
  selector: 'app-portal-home',
  imports: [ReactiveFormsModule, RouterLink, TranslatePipe, DatePipe],
  templateUrl: './portal-home.component.html',
  styleUrl: './portal-home.component.scss',
})
export class PortalHomeComponent implements OnInit {
  private readonly portal = inject(PortalService);
  private readonly auth = inject(AuthService);
  private readonly fb = inject(FormBuilder);

  readonly patient = signal<Patient | null>(null);
  readonly needsLink = signal(false);
  readonly resources = signal<AppointmentResource[]>([]);
  readonly slots = signal<TimeSlot[]>([]);
  readonly appointments = signal<Appointment[]>([]);
  readonly activeTab = signal<'book' | 'mine' | 'link'>('book');
  readonly messageKey = signal<string | undefined>(undefined);
  readonly errorKey = signal<string | undefined>(undefined);

  readonly linkForm = this.fb.nonNullable.group({
    socialSecurityNumber: [''],
    createFromProfile: [true],
  });

  readonly bookForm = this.fb.nonNullable.group({
    resourceId: ['', Validators.required],
    date: ['', Validators.required],
    slotStart: ['', Validators.required],
    notes: [''],
  });

  ngOnInit(): void {
    this.loadPatient();
    this.portal.resources().subscribe({ next: (r) => this.resources.set(r) });
    this.loadAppointments();
  }

  loadPatient(): void {
    this.portal.patient().subscribe({
      next: (p) => {
        this.patient.set(p);
        this.needsLink.set(false);
        if (this.activeTab() === 'link') this.activeTab.set('book');
      },
      error: (err) => {
        if (err.status === 404) {
          this.needsLink.set(true);
          this.activeTab.set('link');
        }
      },
    });
  }

  loadAppointments(): void {
    this.portal.myAppointments().subscribe({ next: (a) => this.appointments.set(a) });
  }

  linkProfile(): void {
    this.errorKey.set(undefined);
    const u = this.auth.user();
    const v = this.linkForm.getRawValue();
    this.portal
      .link({
        socialSecurityNumber: v.socialSecurityNumber || undefined,
        createFromProfile: v.createFromProfile,
        firstName: u?.firstName,
        lastName: u?.lastName,
        phone: u?.phoneNumber ?? undefined,
        email: u?.email,
      })
      .subscribe({
        next: (p) => {
          this.patient.set(p);
          this.needsLink.set(false);
          this.activeTab.set('book');
          this.messageKey.set('portal.linked');
        },
        error: () => this.errorKey.set('portal.linkFailed'),
      });
  }

  onBookParamsChange(): void {
    const v = this.bookForm.getRawValue();
    if (!v.resourceId || !v.date) {
      this.slots.set([]);
      return;
    }

    this.portal.availability(v.resourceId, v.date).subscribe({
      next: (s) => this.slots.set(s),
      error: () => this.slots.set([]),
    });
  }

  bookSlot(slot: TimeSlot): void {
    this.bookForm.patchValue({ slotStart: slot.startAt });
  }

  submitBooking(): void {
    this.errorKey.set(undefined);
    this.messageKey.set(undefined);
    const v = this.bookForm.getRawValue();
    const slot = this.slots().find((s) => s.startAt === v.slotStart);
    if (!slot) {
      this.errorKey.set('portal.pickSlot');
      return;
    }

    this.portal
      .book({
        resourceId: v.resourceId,
        startAt: slot.startAt,
        endAt: slot.endAt,
        notes: v.notes || undefined,
      })
      .subscribe({
        next: () => {
          this.messageKey.set('portal.booked');
          this.bookForm.reset({ resourceId: '', date: '', slotStart: '', notes: '' });
          this.slots.set([]);
          this.loadAppointments();
          this.activeTab.set('mine');
        },
        error: (err) =>
          this.errorKey.set(err.status === 409 ? 'appointments.slotConflict' : 'common.loadError'),
      });
  }

  cancelAppt(id: string): void {
    this.portal.cancelAppointment(id).subscribe({
      next: () => {
        this.messageKey.set('portal.cancelled');
        this.loadAppointments();
      },
      error: () => this.errorKey.set('portal.cancelFailed'),
    });
  }

  selectedSlotLabel(): string {
    const start = this.bookForm.getRawValue().slotStart;
    if (!start) return '';
    return new Date(start).toLocaleString();
  }
}
