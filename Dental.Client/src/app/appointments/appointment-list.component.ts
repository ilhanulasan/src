import { DatePipe } from '@angular/common';
import { Component, inject, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';
import { Appointment } from '../models/appointments';
import { AppointmentService } from './appointment.service';

@Component({
  selector: 'app-appointment-list',
  imports: [RouterLink, TranslatePipe, DatePipe],
  templateUrl: './appointment-list.component.html',
  styleUrl: './appointment-list.component.scss',
})
export class AppointmentListComponent implements OnInit {
  private readonly api = inject(AppointmentService);

  readonly rows = signal<Appointment[]>([]);
  readonly waitlistCount = signal(0);
  readonly loadFailed = signal(false);

  ngOnInit(): void {
    const from = new Date();
    from.setDate(from.getDate() - 7);
    const to = new Date();
    to.setDate(to.getDate() + 30);

    this.api.list({ from: from.toISOString(), to: to.toISOString() }).subscribe({
      next: (data) => this.rows.set(data),
      error: () => {
        this.rows.set([]);
        this.loadFailed.set(true);
      },
    });

    this.api.waitlist().subscribe({
      next: (w) => this.waitlistCount.set(w.filter((x) => x.status === 'Active').length),
    });
  }

  confirm(id: string): void {
    this.api.confirm(id).subscribe({ next: () => this.ngOnInit() });
  }

  cancel(id: string): void {
    this.api.cancel(id).subscribe({ next: () => this.ngOnInit() });
  }
}
