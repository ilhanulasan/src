import { DecimalPipe } from '@angular/common';
import { Component, inject, OnInit, signal } from '@angular/core';
import { TranslatePipe } from '@ngx-translate/core';
import { FinancialAccount, Invoice, Payment } from '../models/finance';
import { FinanceService } from './finance.service';

@Component({
  selector: 'app-finance-dashboard',
  imports: [TranslatePipe, DecimalPipe],
  templateUrl: './finance-dashboard.component.html',
  styleUrl: './finance-dashboard.component.scss',
})
export class FinanceDashboardComponent implements OnInit {
  private readonly api = inject(FinanceService);

  readonly accounts = signal<FinancialAccount[]>([]);
  readonly invoices = signal<Invoice[]>([]);
  readonly payments = signal<Payment[]>([]);

  ngOnInit(): void {
    this.api.accounts().subscribe({ next: (d) => this.accounts.set(d) });
    this.api.invoices().subscribe({ next: (d) => this.invoices.set(d) });
    this.api.payments().subscribe({ next: (d) => this.payments.set(d) });
  }
}
