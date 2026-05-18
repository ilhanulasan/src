import { CommonModule } from '@angular/common';
import { Component, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';

import { OpenDentalPatient } from '../../models/opendental-patient';
import { OpenDentalService } from './open-dental.service';

@Component({
  selector: 'app-open-dental-patient-detail',
  imports: [CommonModule, RouterLink, TranslatePipe],
  templateUrl: './open-dental-patient-detail.component.html',
  styleUrl: './open-dental-patient-detail.component.scss',
})
export class OpenDentalPatientDetailComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly od = inject(OpenDentalService);

  readonly patient = signal<OpenDentalPatient | null>(null);
  readonly loadFailed = signal(false);
  readonly loading = signal(true);

  ngOnInit(): void {
    const patNum = this.route.snapshot.paramMap.get('patNum');
    if (!patNum) {
      this.loading.set(false);
      this.loadFailed.set(true);
      return;
    }

    this.od.getPatient(patNum).subscribe({
      next: (p) => {
        this.patient.set(p);
        this.loading.set(false);
      },
      error: () => {
        this.loadFailed.set(true);
        this.loading.set(false);
      },
    });
  }
}
