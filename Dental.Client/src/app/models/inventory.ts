export interface Supplier {
  id: string;
  companyName: string;
  contactPerson?: string | null;
  phone?: string | null;
  email?: string | null;
  isActive: boolean;
}

export interface Product {
  id: string;
  name: string;
  sku?: string | null;
  barcode?: string | null;
  unit: string;
  minimumStockLevel: number;
  unitPrice: number;
  isActive: boolean;
}

export interface StockLot {
  id: string;
  productId: string;
  barcode?: string | null;
  batchNumber?: string | null;
  quantityOnHand: number;
  expiryDate?: string | null;
  product?: Product;
}

export interface StockExpiryAlert {
  lotId: string;
  productId: string;
  productName: string;
  batchNumber?: string | null;
  expiryDate: string;
  quantityOnHand: number;
  isExpired: boolean;
}
