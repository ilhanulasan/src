import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { UserProfile } from '../models/auth';
import { CreateUserRequest, ResetUserPasswordRequest, UpdateUserRequest } from '../models/user-admin';

@Injectable({ providedIn: 'root' })
export class StaffService {
  private readonly http = inject(HttpClient);
  private readonly api = '/api/users';

  list(): Observable<UserProfile[]> {
    return this.http.get<UserProfile[]>(this.api);
  }

  get(id: string): Observable<UserProfile> {
    return this.http.get<UserProfile>(`${this.api}/${id}`);
  }

  create(body: CreateUserRequest): Observable<UserProfile> {
    return this.http.post<UserProfile>(this.api, body);
  }

  update(id: string, body: UpdateUserRequest): Observable<UserProfile> {
    return this.http.put<UserProfile>(`${this.api}/${id}`, body);
  }

  resetPassword(id: string, body: ResetUserPasswordRequest): Observable<void> {
    return this.http.put<void>(`${this.api}/${id}/password`, body);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.api}/${id}`);
  }
}
