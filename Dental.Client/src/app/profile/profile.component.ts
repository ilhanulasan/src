import { ChangeDetectionStrategy, Component, DestroyRef, inject, OnInit } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { RouterLink } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';

import { AuthService } from '../core/auth.service';
import { ToothIconComponent } from '../shared/tooth-icon.component';

@Component({
  selector: 'app-profile',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterLink, TranslatePipe, ToothIconComponent],
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.scss',
})
export class ProfileComponent implements OnInit {
  readonly auth = inject(AuthService);
  private readonly destroyRef = inject(DestroyRef);

  readonly user = this.auth.user;

  ngOnInit(): void {
    this.auth.refreshProfile().pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      error: () => {
        // Keep cached profile if the refresh fails (expired token, etc.).
      },
    });
  }
}
