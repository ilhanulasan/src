import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, catchError, of, throwError } from 'rxjs';

import { OdontographDocument } from './models/odontograph';

@Injectable({ providedIn: 'root' })
export class OdontographService {
  private readonly http = inject(HttpClient);

  getForPatient(patientId: string): Observable<OdontographDocument | null> {
    const url = `/api/patients/${patientId}/odontograph`;
    return this.http.get<OdontographDocument>(url).pipe(
      catchError((err) => {
        if (err.status === 404) return of(null);
        return throwError(() => err);
      }),
    );
  }

  save(patientId: string, document: OdontographDocument): Observable<OdontographDocument> {
    const url = `/api/patients/${patientId}/odontograph`;
    return this.http.put<OdontographDocument>(url, document);
  }
}
