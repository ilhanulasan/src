export const PersonnelTypes = {
  Doctor: 'Doctor',
  Nurse: 'Nurse',
  Technician: 'Technician',
  PatientCaregiver: 'PatientCaregiver',
  Secretary: 'Secretary',
} as const;

export type PersonnelType = (typeof PersonnelTypes)[keyof typeof PersonnelTypes];

export const DentalSpecialties = {
  OralMaxillofacialSurgery: 'OralMaxillofacialSurgery',
  Orthodontics: 'Orthodontics',
  Pedodontics: 'Pedodontics',
  Periodontics: 'Periodontics',
  Endodontics: 'Endodontics',
  Prosthodontics: 'Prosthodontics',
  RestorativeDentistry: 'RestorativeDentistry',
  OralMaxillofacialRadiology: 'OralMaxillofacialRadiology',
} as const;

export type DentalSpecialty = (typeof DentalSpecialties)[keyof typeof DentalSpecialties];

export const ALL_PERSONNEL_TYPES: PersonnelType[] = [
  PersonnelTypes.Doctor,
  PersonnelTypes.Nurse,
  PersonnelTypes.Technician,
  PersonnelTypes.PatientCaregiver,
  PersonnelTypes.Secretary,
];

export const ALL_DENTAL_SPECIALTIES: DentalSpecialty[] = [
  DentalSpecialties.OralMaxillofacialSurgery,
  DentalSpecialties.Orthodontics,
  DentalSpecialties.Pedodontics,
  DentalSpecialties.Periodontics,
  DentalSpecialties.Endodontics,
  DentalSpecialties.Prosthodontics,
  DentalSpecialties.RestorativeDentistry,
  DentalSpecialties.OralMaxillofacialRadiology,
];

export interface Personnel {
  id: string;
  firstName: string;
  lastName: string;
  email?: string | null;
  phone?: string | null;
  notes?: string | null;
  personnelType: PersonnelType;
  specialties: DentalSpecialty[];
  userId?: string | null;
  appointmentResourceId?: string | null;
  isActive: boolean;
}

export interface DoctorAppointmentOption {
  personnelId: string;
  resourceId: string;
  displayName: string;
  specialties: DentalSpecialty[];
}

export interface CreatePersonnelRequest {
  firstName: string;
  lastName: string;
  email?: string | null;
  phone?: string | null;
  notes?: string | null;
  personnelType: PersonnelType;
  specialties: DentalSpecialty[];
  isActive: boolean;
}

export interface UpdatePersonnelRequest extends CreatePersonnelRequest {
  id: string;
}
