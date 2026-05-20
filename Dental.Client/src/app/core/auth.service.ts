import { HttpClient } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import { Observable, tap } from 'rxjs';

import { AuthResponse, UserProfile } from '../models/auth';
import { AppRoles } from './roles';

const TOKEN_KEY = 'dental-auth-token';
const USER_KEY = 'dental-auth-user';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly api = '/api/auth';

  readonly token = signal<string | null>(null);
  readonly user = signal<UserProfile | null>(null);

  constructor() {
    this.restoreFromStorage();
  }

  isLoggedIn(): boolean {
    return !!this.token();
  }

  restoreFromStorage(): void {
    if (typeof localStorage === 'undefined') {
      return;
    }

    const storedToken = localStorage.getItem(TOKEN_KEY);
    const storedUser = localStorage.getItem(USER_KEY);
    if (!storedToken || !storedUser) {
      return;
    }

    try {
      this.token.set(storedToken);
      this.user.set(JSON.parse(storedUser) as UserProfile);
    } catch {
      this.clearSession();
    }
  }

  login(email: string, password: string): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.api}/login`, { email, password }).pipe(tap((r) => this.persist(r)));
  }

  register(payload: {
    email: string;
    password: string;
    firstName: string;
    lastName: string;
    phoneNumber: string;
    address?: string | null;
    pictureData?: string | null;
  }): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.api}/register`, payload).pipe(tap((r) => this.persist(r)));
  }

  refreshProfile(): Observable<UserProfile> {
    return this.http.get<UserProfile>(`${this.api}/me`).pipe(
      tap((profile) => {
        this.user.set(profile);
        if (typeof localStorage !== 'undefined') {
          localStorage.setItem(USER_KEY, JSON.stringify(profile));
        }
      }),
    );
  }

  logout(): void {
    this.clearSession();
  }

  avatarUrl(): string | null {
    const url = this.user()?.pictureUrl;
    return url && url.trim().length > 0 ? url.trim() : null;
  }

  displayName(): string {
    const u = this.user();
    if (!u) {
      return '';
    }

    return `${u.firstName} ${u.lastName}`.trim();
  }

  roles(): string[] {
    return this.user()?.roles ?? [];
  }

  hasRole(role: string): boolean {
    return this.roles().includes(role);
  }

  hasAnyRole(allowed: string[]): boolean {
    const mine = this.roles();
    return allowed.some((r) => mine.includes(r));
  }

  isPatient(): boolean {
    return this.hasRole(AppRoles.Patient) && !this.hasRole(AppRoles.Admin);
  }

  isDoctor(): boolean {
    return this.hasRole(AppRoles.Doctor) && !this.hasRole(AppRoles.Admin);
  }

  isAdmin(): boolean {
    return this.hasRole(AppRoles.Admin);
  }

  defaultRoute(): string {
    if (this.isPatient()) {
      return '/portal';
    }

    if (this.isDoctor()) {
      return '/appointments';
    }

    return '/dashboard';
  }

  private persist(response: AuthResponse): void {
    this.token.set(response.token);
    this.user.set(response.user);
    if (typeof localStorage === 'undefined') {
      return;
    }

    localStorage.setItem(TOKEN_KEY, response.token);
    localStorage.setItem(USER_KEY, JSON.stringify(response.user));
  }

  private clearSession(): void {
    this.token.set(null);
    this.user.set(null);
    if (typeof localStorage !== 'undefined') {
      localStorage.removeItem(TOKEN_KEY);
      localStorage.removeItem(USER_KEY);
    }
  }
}
