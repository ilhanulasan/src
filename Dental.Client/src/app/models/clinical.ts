export interface Icd10Code {
  id: string;
  code: string;
  descriptionTr: string;
  descriptionEn?: string | null;
}

export interface Examination {
  id: string;
  patientId: string;
  doctorUserId?: string | null;
  examinedAt: string;
  status: string;
  chiefComplaint?: string | null;
  clinicalFindings?: string | null;
  notes?: string | null;
  diagnoses?: ExaminationDiagnosis[];
}

export interface ExaminationDiagnosis {
  id: string;
  examinationId: string;
  icd10CodeId: string;
  isPrimary: boolean;
  icd10Code?: Icd10Code;
}

export interface TreatmentPlan {
  id: string;
  patientId: string;
  title: string;
  status: string;
  estimatedTotal: number;
  items?: TreatmentPlanItem[];
}

export interface TreatmentPlanItem {
  id: string;
  procedureName: string;
  status: string;
  unitPrice: number;
  quantity: number;
}
