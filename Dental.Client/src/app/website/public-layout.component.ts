import { Component, inject, signal } from '@angular/core';
import { NavigationEnd, Router, RouterLink, RouterOutlet } from '@angular/router';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { filter } from 'rxjs';

import { AuthService } from '../core/auth.service';
import { ToothIconComponent } from '../shared/tooth-icon.component';

@Component({
  selector: 'app-public-layout',
  imports: [RouterOutlet, RouterLink, TranslatePipe, ToothIconComponent],
  templateUrl: './public-layout.component.html',
  styleUrl: './public-layout.component.scss',
})
export class PublicLayoutComponent {
  readonly auth = inject(AuthService);
  readonly translate = inject(TranslateService);
  private readonly router = inject(Router);

  readonly menuOpen = signal(false);
  readonly headerScrolled = signal(false);

  constructor() {
    this.router.events.pipe(filter((e): e is NavigationEnd => e instanceof NavigationEnd)).subscribe(() => {
      this.menuOpen.set(false);
    });
  }

  appointmentRoute(): string {
    return '/book-appointment';
  }

  workspaceRoute(): string {
    return this.auth.isLoggedIn() ? this.auth.defaultRoute() : '/login';
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

  closeUserMenu(details: HTMLDetailsElement): void {
    details.open = false;
  }

  toggleMenu(): void {
    this.menuOpen.update((v) => !v);
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

  logout(): void {
    this.auth.logout();
    void this.router.navigateByUrl('/');
  }

  onScroll(event: Event): void {
    const el = event.target as HTMLElement;
    this.headerScrolled.set(el.scrollTop > 12);
  }
}
