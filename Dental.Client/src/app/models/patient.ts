export enum EducationLevel {
  ElementarySchool = 'ElementarySchool',
  HighSchool = 'HighSchool',
  Graduate = 'Graduate',
  Masters = 'Masters',
  Phd = 'Phd',
}

export interface Patient {
  id: string;
  name: string;
  surname: string;
  socialSecurityNumber: string;
  address?: string | null;
  phone?: string | null;
  email?: string | null;
  dateOfBirth: string;
  gender: string;
  education: EducationLevel;
  bloodType?: string | null;
  emergencyContactName?: string | null;
  emergencyContactPhone?: string | null;
  clinicalSummary?: string | null;
  isActive?: boolean;
}

export interface PatientAllergy {
  id: string;
  patientId: string;
  substance: string;
  severity?: string | null;
  reaction?: string | null;
  isActive: boolean;
}

export interface PatientMedicalHistory {
  id: string;
  patientId: string;
  title: string;
  description?: string | null;
  recordedOn?: string | null;
  recordedBy?: string | null;
}

export interface PatientClinicalNote {
  id: string;
  patientId: string;
  title: string;
  content: string;
  isConfidential: boolean;
}

export interface PatientKvkkConsent {
  id: string;
  patientId: string;
  consentType: string;
  isGranted: boolean;
  consentedAt: string;
  consentVersion?: string | null;
}

export interface PatientDocument {
  id: string;
  patientId: string;
  fileName: string;
  category: string;
  fileSizeBytes: number;
  description?: string | null;
}

export interface PatientBalance {
  patientId: string;
  totalCharges: number;
  totalPayments: number;
  balance: number;
}
