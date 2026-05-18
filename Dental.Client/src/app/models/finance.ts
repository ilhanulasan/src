export interface FinancialAccount {
  id: string;
  name: string;
  accountType: string;
  currency: string;
  balance: number;
}

export interface Invoice {
  id: string;
  patientId: string;
  invoiceNumber: string;
  issueDate: string;
  status: string;
  totalAmount: number;
  paidAmount: number;
}

export interface Payment {
  id: string;
  patientId: string;
  amount: number;
  method: string;
  paidAt: string;
  notes?: string | null;
}

export interface PaymentInstallmentPlan {
  id: string;
  patientId: string;
  totalAmount: number;
  installmentCount: number;
  isActive: boolean;
  installments?: PaymentInstallment[];
}

export interface PaymentInstallment {
  id: string;
  installmentNumber: number;
  amount: number;
  dueDate: string;
  isPaid: boolean;
}
