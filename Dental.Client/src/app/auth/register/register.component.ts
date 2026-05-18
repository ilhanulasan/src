import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';

import { AuthService } from '../../core/auth.service';
import { ToothIconComponent } from '../../shared/tooth-icon.component';
import { passwordsMatchValidator } from './passwords-match.validator';

@Component({
  selector: 'app-register',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [ReactiveFormsModule, RouterLink, TranslatePipe, ToothIconComponent],
  templateUrl: './register.component.html',
  styleUrl: './register.component.scss',
})
export class RegisterComponent {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  readonly submitting = signal(false);
  readonly serverError = signal<string | null>(null);

  readonly form = this.fb.group(
    {
      firstName: ['', [Validators.required, Validators.maxLength(128)]],
      lastName: ['', [Validators.required, Validators.maxLength(128)]],
      email: ['', [Validators.required, Validators.email, Validators.maxLength(256)]],
      phoneNumber: ['', [Validators.required, Validators.maxLength(32)]],
      address: ['', [Validators.maxLength(512)]],
      pictureUrl: ['', [Validators.maxLength(2048)]],
      password: ['', [Validators.required, Validators.minLength(8)]],
      confirmPassword: ['', [Validators.required, Validators.minLength(8)]],
    },
    { validators: [passwordsMatchValidator] },
  );

  readonly heroExampleUrl =
    'https://dentistry.emu.edu.tr/PublishingImages/Stock/dentistry-01.jpg?RenditionID=3';

  submit(): void {
    if (this.form.invalid || this.submitting()) {
      this.form.markAllAsTouched();
      return;
    }

    this.submitting.set(true);
    this.serverError.set(null);

    const raw = this.form.getRawValue();
    this.auth
      .register({
        email: raw.email!.trim(),
        password: raw.password!,
        firstName: raw.firstName!.trim(),
        lastName: raw.lastName!.trim(),
        phoneNumber: raw.phoneNumber!.trim(),
        address: raw.address?.trim() ? raw.address.trim() : null,
        pictureUrl: raw.pictureUrl?.trim() ? raw.pictureUrl.trim() : null,
      })
      .subscribe({
        next: () => void this.router.navigateByUrl('/'),
        error: () => {
          this.submitting.set(false);
          this.serverError.set('auth.registerFailed');
        },
      });
  }
}
