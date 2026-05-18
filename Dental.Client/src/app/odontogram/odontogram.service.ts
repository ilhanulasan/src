import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { catchError, Observable, of, throwError } from 'rxjs';
import { Odontogram } from './models/odontogram';

@Injectable({ providedIn: 'root' })
export class OdontogramService {
  private readonly http = inject(HttpClient);

  getForPatient(patientId: string): Observable<Odontogram | null> {
    const url = `/api/patients/${patientId}/odontogram`;
    return this.http.get<Odontogram>(url).pipe(
      catchError((err: { status?: number }) =>
        err.status === 404 ? of(null) : throwError(() => err),
      ),
    );
  }

  /** Creates or replaces the patient's chart snapshot. */
  save(patientId: string, odontogram: Odontogram): Observable<Odontogram> {
    const url = `/api/patients/${patientId}/odontogram`;
    return this.http.put<Odontogram>(url, odontogram);
  }
}
