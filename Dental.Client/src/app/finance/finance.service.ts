import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { FinancialAccount, Invoice, Payment, PaymentInstallmentPlan } from '../models/finance';

@Injectable({ providedIn: 'root' })
export class FinanceService {
  private readonly http = inject(HttpClient);

  accounts(): Observable<FinancialAccount[]> {
    return this.http.get<FinancialAccount[]>('/api/financial-accounts');
  }

  invoices(patientId?: string): Observable<Invoice[]> {
    let params = new HttpParams();
    if (patientId) params = params.set('patientId', patientId);
    return this.http.get<Invoice[]>('/api/invoices', { params });
  }

  payments(patientId?: string): Observable<Payment[]> {
    let params = new HttpParams();
    if (patientId) params = params.set('patientId', patientId);
    return this.http.get<Payment[]>('/api/payments', { params });
  }

  installmentPlans(patientId?: string): Observable<PaymentInstallmentPlan[]> {
    let params = new HttpParams();
    if (patientId) params = params.set('patientId', patientId);
    return this.http.get<PaymentInstallmentPlan[]>('/api/installment-plans', { params });
  }
}
