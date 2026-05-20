import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';

import { AuthService } from '../core/auth.service';
import { ToothIconComponent } from '../shared/tooth-icon.component';

@Component({
  selector: 'app-landing',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterLink, TranslatePipe, ReactiveFormsModule, ToothIconComponent],
  templateUrl: './landing.component.html',
  styleUrl: './landing.component.scss',
})
export class LandingComponent {
  private readonly fb = inject(FormBuilder);
  private readonly router = inject(Router);
  readonly auth = inject(AuthService);

  readonly contactSent = signal(false);

  readonly heroImageUrl =
    'https://images.unsplash.com/photo-1606811970810-945a1536f209?auto=format&fit=crop&w=1200&q=80';

  readonly services = [
    { icon: 'dentistry', key: 'alignment' },
    { icon: 'auto_awesome', key: 'cosmetic' },
    { icon: 'cleaning_services', key: 'hygiene' },
    { icon: 'medical_services', key: 'rootCanal' },
    { icon: 'support_agent', key: 'advisory' },
    { icon: 'search', key: 'cavity' },
  ] as const;

  readonly processes = [
    { key: 'cosmetic', image: 'https://images.unsplash.com/photo-1629909613654-28e377b9e381?auto=format&fit=crop&w=400&q=80' },
    { key: 'pediatric', image: 'https://images.unsplash.com/photo-1588776814546-1ffcf47267a5?auto=format&fit=crop&w=400&q=80' },
    { key: 'implants', image: 'https://images.unsplash.com/photo-1609840114035-3c981b782dfe?auto=format&fit=crop&w=400&q=80' },
  ] as const;

  readonly reviews = [
    { key: 'alan', stars: 5 },
    { key: 'sophie', stars: 5 },
    { key: 'james', stars: 5 },
  ] as const;

  readonly contactForm = this.fb.nonNullable.group({
    name: ['', Validators.required],
    email: ['', [Validators.required, Validators.email]],
    phone: ['', Validators.required],
    date: ['', Validators.required],
  });

  appointmentRoute(): string {
    if (!this.auth.isLoggedIn()) {
      return '/register';
    }

    if (this.auth.isPatient()) {
      return '/portal';
    }

    return this.auth.defaultRoute();
  }

  submitContact(): void {
    if (this.contactForm.invalid) {
      this.contactForm.markAllAsTouched();
      return;
    }

    if (!this.auth.isLoggedIn()) {
      void this.router.navigate(['/register'], {
        queryParams: { intent: 'appointment' },
      });
      return;
    }

    if (this.auth.isPatient()) {
      void this.router.navigateByUrl('/portal');
      return;
    }

    this.contactSent.set(true);
    this.contactForm.reset();
  }
}
