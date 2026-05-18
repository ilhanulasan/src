import { afterNextRender, Component, inject } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';

import { AuthService } from './core/auth.service';
import { ToothIconComponent } from './shared/tooth-icon.component';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, RouterLink, RouterLinkActive, TranslatePipe, ToothIconComponent],
  templateUrl: './app.html',
  styleUrl: './app.scss',
})
export class App {
  readonly translate = inject(TranslateService);
  readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  constructor() {
    afterNextRender(() => {
      if (typeof localStorage === 'undefined') {
        return;
      }

      const saved = localStorage.getItem('dental-lang');
      if (saved === 'en' || saved === 'tr') {
        void this.translate.use(saved);
      }
    });
  }

  initials(): string {
    const u = this.auth.user();
    if (!u) {
      return '';
    }

    const a = u.firstName?.charAt(0) ?? '';
    const b = u.lastName?.charAt(0) ?? '';
    return `${a}${b}`.toUpperCase();
  }

  logout(): void {
    this.auth.logout();
    void this.router.navigateByUrl('/');
  }

  closeUserMenu(details: HTMLDetailsElement): void {
    details.open = false;
  }

  onLangChange(event: Event): void {
    const value = (event.target as HTMLSelectElement).value;
    if (value !== 'en' && value !== 'tr') {
      return;
    }

    void this.translate.use(value);
    if (typeof localStorage !== 'undefined') {
      localStorage.setItem('dental-lang', value);
    }
  }
}
