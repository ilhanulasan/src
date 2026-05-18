import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';

import { OpenDentalPatient } from '../../models/opendental-patient';

@Injectable({ providedIn: 'root' })
export class OpenDentalService {
  private readonly http = inject(HttpClient);

  listPatients(limit = 100, offset = 0) {
    const params = new HttpParams().set('Limit', String(limit)).set('Offset', String(offset));
    return this.http.get<OpenDentalPatient[]>('/api/opendental/patients', { params });
  }

  getPatient(patNum: number | string) {
    return this.http.get<OpenDentalPatient>(`/api/opendental/patients/${patNum}`);
  }
}
