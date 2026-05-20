import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';
import { AuthService } from '../core/auth.service';
import { passwordPolicyValidator } from '../auth/register/password-policy.validator';
import { passwordsMatchValidator } from '../auth/register/passwords-match.validator';
import { ToothIconComponent } from '../shared/tooth-icon.component';
import { PublicBookingService } from './public-booking.service';

@Component({
  selector: 'app-complete-account',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [ReactiveFormsModule, TranslatePipe, ToothIconComponent],
  templateUrl: './complete-account.component.html',
  styleUrl: './book-appointment.component.scss',
})
export class CompleteAccountComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly bookingApi = inject(PublicBookingService);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly fb = inject(FormBuilder);

  readonly inviteToken = signal<string | null>(null);
  readonly loadFailed = signal(false);
  readonly submitting = signal(false);
  readonly errorKey = signal<string | undefined>(undefined);

  readonly form = this.fb.group(
    {
      email: [{ value: '', disabled: true }],
      password: ['', [Validators.required, Validators.minLength(8), passwordPolicyValidator]],
      confirmPassword: ['', [Validators.required, Validators.minLength(8)]],
    },
    { validators: [passwordsMatchValidator] },
  );

  ngOnInit(): void {
    const token = this.route.snapshot.queryParamMap.get('token');
    if (!token) {
      this.loadFailed.set(true);
      return;
    }

    this.inviteToken.set(token);
    this.bookingApi.getInvite(token).subscribe({
      next: (info) => {
        this.form.patchValue({ email: info.email });
      },
      error: () => this.loadFailed.set(true),
    });
  }

  submit(): void {
    const token = this.inviteToken();
    if (!token || this.form.invalid || this.submitting()) {
      this.form.markAllAsTouched();
      return;
    }

    this.submitting.set(true);
    this.errorKey.set(undefined);
    const password = this.form.getRawValue().password!;

    this.bookingApi.completeInvite(token, { password }).subscribe({
      next: (res) => {
        this.auth.applyAuthResponse(res);
        void this.router.navigateByUrl('/');
      },
      error: () => {
        this.submitting.set(false);
        this.errorKey.set('booking.completeFailed');
      },
    });
  }
}
