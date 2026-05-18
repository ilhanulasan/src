import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { Product, StockExpiryAlert, StockLot, Supplier } from '../models/inventory';

@Injectable({ providedIn: 'root' })
export class InventoryService {
  private readonly http = inject(HttpClient);

  suppliers(): Observable<Supplier[]> {
    return this.http.get<Supplier[]>('/api/suppliers');
  }

  products(): Observable<Product[]> {
    return this.http.get<Product[]>('/api/products');
  }

  stockLots(): Observable<StockLot[]> {
    return this.http.get<StockLot[]>('/api/stock/lots');
  }

  expiryAlerts(): Observable<StockExpiryAlert[]> {
    return this.http.get<StockExpiryAlert[]>('/api/stock/expiry-alerts');
  }
}
