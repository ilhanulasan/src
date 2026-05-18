import { Component, inject, OnInit, signal } from '@angular/core';
import { TranslatePipe } from '@ngx-translate/core';
import { Product, StockExpiryAlert, Supplier } from '../models/inventory';
import { InventoryService } from './inventory.service';

@Component({
  selector: 'app-inventory-dashboard',
  imports: [TranslatePipe],
  templateUrl: './inventory-dashboard.component.html',
  styleUrl: './inventory-dashboard.component.scss',
})
export class InventoryDashboardComponent implements OnInit {
  private readonly api = inject(InventoryService);

  readonly suppliers = signal<Supplier[]>([]);
  readonly products = signal<Product[]>([]);
  readonly expiryAlerts = signal<StockExpiryAlert[]>([]);

  ngOnInit(): void {
    this.api.suppliers().subscribe({ next: (d) => this.suppliers.set(d) });
    this.api.products().subscribe({ next: (d) => this.products.set(d) });
    this.api.expiryAlerts().subscribe({ next: (d) => this.expiryAlerts.set(d) });
  }
}
