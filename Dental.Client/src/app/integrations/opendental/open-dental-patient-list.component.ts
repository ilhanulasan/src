import { CommonModule } from '@angular/common';
import { Component, inject, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';

import { OpenDentalPatient } from '../../models/opendental-patient';
import { OpenDentalService } from './open-dental.service';

@Component({
  selector: 'app-open-dental-patient-list',
  imports: [CommonModule, RouterLink, TranslatePipe],
  templateUrl: './open-dental-patient-list.component.html',
  styleUrl: './open-dental-patient-list.component.scss',
})
export class OpenDentalPatientListComponent implements OnInit {
  private readonly od = inject(OpenDentalService);

  readonly rows = signal<OpenDentalPatient[]>([]);
  readonly loadFailed = signal(false);
  readonly busy = signal(false);
  readonly loadMoreAvailable = signal(false);
  private offset = 0;

  ngOnInit(): void {
    this.reload();
  }

  reload(): void {
    this.offset = 0;
    this.rows.set([]);
    this.loadMoreAvailable.set(false);
    this.fetchNext(true);
  }

  loadMore(): void {
    this.fetchNext(false);
  }

  private fetchNext(replace: boolean): void {
    this.busy.set(true);
    this.loadFailed.set(false);
    this.od.listPatients(100, this.offset).subscribe({
      next: (batch) => {
        const merged = replace ? [...batch] : [...this.rows(), ...batch];
        this.rows.set(merged);
        this.offset = merged.length;
        this.loadMoreAvailable.set(batch.length === 100);
        this.busy.set(false);
      },
      error: () => {
        this.busy.set(false);
        if (replace) {
          this.rows.set([]);
        }

        this.loadFailed.set(true);
      },
    });
  }
}
