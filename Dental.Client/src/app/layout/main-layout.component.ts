import { Component, inject, signal } from '@angular/core';
import { NavigationEnd, Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { filter } from 'rxjs';

import { AuthService } from '../core/auth.service';
import { ToothIconComponent } from '../shared/tooth-icon.component';

@Component({
  selector: 'app-main-layout',
  imports: [RouterOutlet, RouterLink, RouterLinkActive, TranslatePipe, ToothIconComponent],
  templateUrl: './main-layout.component.html',
  styleUrl: './main-layout.component.scss',
})
export class MainLayoutComponent {
  readonly translate = inject(TranslateService);
  readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  readonly sidebarOpen = signal(false);

  constructor() {
    this.router.events.pipe(filter((e): e is NavigationEnd => e instanceof NavigationEnd)).subscribe(() => {
      this.sidebarOpen.set(false);
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

  toggleSidebar(): void {
    this.sidebarOpen.update((v) => !v);
  }
}
