import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';
import { ToothIconComponent } from '../shared/tooth-icon.component';
import { PublicBookingService } from './public-booking.service';

@Component({
  selector: 'app-confirm-appointment',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterLink, TranslatePipe, ToothIconComponent],
  templateUrl: './confirm-appointment.component.html',
  styleUrl: './book-appointment.component.scss',
})
export class ConfirmAppointmentComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly bookingApi = inject(PublicBookingService);

  readonly state = signal<'loading' | 'ok' | 'fail'>('loading');

  ngOnInit(): void {
    const appointmentId = this.route.snapshot.queryParamMap.get('appointmentId');
    const token = this.route.snapshot.queryParamMap.get('token');

    if (!appointmentId || !token) {
      this.state.set('fail');
      return;
    }

    this.bookingApi.confirm(appointmentId, token).subscribe({
      next: () => this.state.set('ok'),
      error: () => this.state.set('fail'),
    });
  }
}
