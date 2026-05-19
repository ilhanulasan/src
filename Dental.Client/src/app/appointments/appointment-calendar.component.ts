import { DatePipe } from '@angular/common';
import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';
import { Appointment, AppointmentResource } from '../models/appointments';
import { Patient } from '../models/patient';
import { PatientService } from '../patients/patient.service';
import { AppointmentService } from './appointment.service';

const HOUR_START = 8;
const HOUR_END = 20;
const HOUR_COUNT = HOUR_END - HOUR_START;

@Component({
  selector: 'app-appointment-calendar',
  imports: [RouterLink, TranslatePipe, DatePipe],
  templateUrl: './appointment-calendar.component.html',
  styleUrl: './appointment-calendar.component.scss',
})
export class AppointmentCalendarComponent implements OnInit {
  private readonly api = inject(AppointmentService);
  private readonly patientsApi = inject(PatientService);

  readonly resources = signal<AppointmentResource[]>([]);
  readonly patients = signal<Patient[]>([]);
  readonly appointments = signal<Appointment[]>([]);
  readonly selectedResourceId = signal<string>('');
  readonly weekStart = signal(this.startOfWeek(new Date()));
  readonly selectedAppt = signal<Appointment | null>(null);
  readonly loadFailed = signal(false);

  readonly weekDays = computed(() => {
    const start = this.weekStart();
    return Array.from({ length: 7 }, (_, i) => {
      const d = new Date(start);
      d.setDate(d.getDate() + i);
      return d;
    });
  });

  readonly hours = Array.from({ length: HOUR_COUNT }, (_, i) => HOUR_START + i);

  ngOnInit(): void {
    this.patientsApi.list().subscribe({ next: (p) => this.patients.set(p) });
    this.api.resources().subscribe({
      next: (r) => {
        this.resources.set(r);
        if (r.length && !this.selectedResourceId()) {
          this.selectedResourceId.set(r[0].id);
        }
        this.loadWeek();
      },
      error: () => this.loadFailed.set(true),
    });
  }

  patientName(patientId: string): string {
    const p = this.patients().find((x) => x.id === patientId);
    return p ? `${p.name} ${p.surname}` : patientId.slice(0, 8);
  }

  onResourceChange(event: Event): void {
    this.selectedResourceId.set((event.target as HTMLSelectElement).value);
    this.loadWeek();
  }

  prevWeek(): void {
    const d = new Date(this.weekStart());
    d.setDate(d.getDate() - 7);
    this.weekStart.set(d);
    this.loadWeek();
  }

  nextWeek(): void {
    const d = new Date(this.weekStart());
    d.setDate(d.getDate() + 7);
    this.weekStart.set(d);
    this.loadWeek();
  }

  today(): void {
    this.weekStart.set(this.startOfWeek(new Date()));
    this.loadWeek();
  }

  loadWeek(): void {
    const start = this.weekStart();
    const end = new Date(start);
    end.setDate(end.getDate() + 7);
    end.setHours(23, 59, 59, 999);

    this.api
      .list({
        from: start.toISOString(),
        to: end.toISOString(),
        resourceId: this.selectedResourceId() || undefined,
      })
      .subscribe({
        next: (data) => this.appointments.set(data),
        error: () => this.loadFailed.set(true),
      });
  }

  apptsForDay(day: Date): Appointment[] {
    const dayStart = new Date(day);
    dayStart.setHours(0, 0, 0, 0);
    const dayEnd = new Date(day);
    dayEnd.setHours(23, 59, 59, 999);

    return this.appointments().filter((a) => {
      const s = new Date(a.startAt);
      return s >= dayStart && s <= dayEnd && a.status !== 'Cancelled';
    });
  }

  blockStyle(appt: Appointment): Record<string, string> {
    const start = new Date(appt.startAt);
    const end = new Date(appt.endAt);
    const topMin = (start.getHours() - HOUR_START) * 60 + start.getMinutes();
    const heightMin = Math.max(20, (end.getTime() - start.getTime()) / 60000);
    return {
      top: `${(topMin / (HOUR_COUNT * 60)) * 100}%`,
      height: `${(heightMin / (HOUR_COUNT * 60)) * 100}%`,
    };
  }

  statusClass(status: string): string {
    return `status-${status.toLowerCase()}`;
  }

  selectAppt(appt: Appointment, event: Event): void {
    event.stopPropagation();
    this.selectedAppt.set(appt);
  }

  closeDetail(): void {
    this.selectedAppt.set(null);
  }

  confirmSelected(): void {
    const a = this.selectedAppt();
    if (!a) return;
    this.api.confirm(a.id).subscribe({ next: () => this.afterAction() });
  }

  cancelSelected(): void {
    const a = this.selectedAppt();
    if (!a) return;
    this.api.cancel(a.id).subscribe({ next: () => this.afterAction() });
  }

  private afterAction(): void {
    this.selectedAppt.set(null);
    this.loadWeek();
  }

  private startOfWeek(d: Date): Date {
    const x = new Date(d);
    const day = x.getDay();
    const diff = day === 0 ? -6 : 1 - day;
    x.setDate(x.getDate() + diff);
    x.setHours(0, 0, 0, 0);
    return x;
  }
}
