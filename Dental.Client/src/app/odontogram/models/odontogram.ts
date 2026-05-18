/** Odontogram model shaped for OdontoManage-style APIs and Dental.Web patient IDs (Guid strings). */

export interface Pathology {
  id: number;
  name?: string;
  description?: string;
  color?: string;
}

export type TreatmentKind = 'extraction' | 'endo' | 'crown' | 'bridge';

export interface Treatment {
  id: number;
  name?: string;
  description?: string;
  durationMinutes?: number;
  /** Resolved from local catalog — required for overlays after hydrate */
  kind?: TreatmentKind;
}

export interface Tooth {
  id: number;
  toothNumber: number;
}

export interface ToothPathology {
  id?: number;
  tooth: Tooth;
  pathology: Pathology;
  toothFace: number;
}

export interface ToothTreatment {
  id?: number;
  treatment: Treatment;
  toothNumber: number;
  toothFace: number;
  status: 'pending' | 'done';
}

export interface BridgeTreatment {
  id?: number;
  treatment: Treatment;
  startTooth: number;
  endTooth: number;
  status: 'pending' | 'done';
}

/** Wire format persisted by Dental.Web `/api/patients/{patientId}/odontogram` */
export interface Odontogram {
  id?: string;
  patient: { id: string } | string;
  type?: 'adult' | 'child';
  toothPathologies: ToothPathology[];
  toothTreatments: ToothTreatment[];
  bridgeTreatments: BridgeTreatment[];
}
