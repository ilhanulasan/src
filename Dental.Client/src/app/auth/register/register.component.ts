import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';

import { AuthService } from '../../core/auth.service';
import { ToothIconComponent } from '../../shared/tooth-icon.component';
import { passwordPolicyValidator } from './password-policy.validator';
import { passwordsMatchValidator } from './passwords-match.validator';

const MaxPictureBytes = 512 * 1024;

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
  readonly submitAttempted = signal(false);
  readonly serverError = signal<string | null>(null);
  readonly picturePreview = signal<string | null>(null);
  readonly pictureError = signal<string | null>(null);

  private pictureDataUrl: string | null = null;

  readonly form = this.fb.group(
    {
      firstName: ['', [Validators.required, Validators.maxLength(128)]],
      lastName: ['', [Validators.required, Validators.maxLength(128)]],
      email: ['', [Validators.required, Validators.email, Validators.maxLength(256)]],
      phoneNumber: ['', [Validators.required, Validators.maxLength(32)]],
      address: ['', [Validators.maxLength(512)]],
      password: ['', [Validators.required, Validators.minLength(8), passwordPolicyValidator]],
      confirmPassword: ['', [Validators.required, Validators.minLength(8)]],
    },
    { validators: [passwordsMatchValidator] },
  );

  submit(): void {
    this.submitAttempted.set(true);

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
        pictureData: this.pictureDataUrl,
      })
      .subscribe({
        next: () => void this.router.navigateByUrl(this.auth.defaultRoute()),
        error: (err: HttpErrorResponse) => {
          this.submitting.set(false);
          this.serverError.set(this.mapRegisterError(err));
        },
      });
  }

  onPictureSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    input.value = '';
    if (!file) {
      return;
    }

    this.loadPictureFile(file);
  }

  clearPicture(): void {
    this.pictureDataUrl = null;
    this.picturePreview.set(null);
    this.pictureError.set(null);
  }

  showFieldError(controlName: string): boolean {
    const control = this.form.get(controlName);
    if (!control) {
      return false;
    }

    return control.invalid && (control.touched || this.submitAttempted());
  }

  fieldErrorKey(controlName: string): string | null {
    const control = this.form.get(controlName);
    if (!control?.errors) {
      return null;
    }

    if (control.errors['required']) {
      return 'auth.fieldRequired';
    }

    if (control.errors['email']) {
      return 'auth.invalidEmail';
    }

    if (control.errors['passwordPolicy'] || control.errors['minlength']) {
      return 'auth.passwordPolicy';
    }

    return 'auth.fieldInvalid';
  }

  showPasswordMismatch(): boolean {
    return (
      !!this.form.errors?.['mismatch'] &&
      (this.form.touched || this.form.dirty || this.submitAttempted())
    );
  }

  private loadPictureFile(file: File): void {
    this.pictureError.set(null);

    if (!file.type.startsWith('image/')) {
      this.pictureError.set('auth.pictureInvalidType');
      return;
    }

    if (file.size > MaxPictureBytes) {
      this.pictureError.set('auth.pictureTooLarge');
      return;
    }

    const reader = new FileReader();
    reader.onload = () => {
      const result = typeof reader.result === 'string' ? reader.result : null;
      if (!result) {
        this.pictureError.set('auth.pictureReadFailed');
        return;
      }

      this.pictureDataUrl = result;
      this.picturePreview.set(result);
    };
    reader.onerror = () => this.pictureError.set('auth.pictureReadFailed');
    reader.readAsDataURL(file);
  }

  private mapRegisterError(err: HttpErrorResponse): string {
    const body = err.error as { errors?: Record<string, string[]> } | undefined;
    const messages = body?.errors ? Object.values(body.errors).flat() : [];
    const first = messages[0]?.toLowerCase() ?? '';

    if (first.includes('already taken') || first.includes('is already')) {
      return 'auth.emailInUse';
    }

    if (first.includes('password')) {
      return 'auth.passwordPolicy';
    }

    return 'auth.registerFailed';
  }
}
