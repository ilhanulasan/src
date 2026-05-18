export interface AppointmentResource {
  id: string;
  name: string;
  resourceType: string;
  defaultDurationMinutes: number;
  color?: string | null;
  isActive: boolean;
}

export interface Appointment {
  id: string;
  patientId: string;
  primaryResourceId: string;
  startAt: string;
  endAt: string;
  status: string;
  notes?: string | null;
  isOnlineBooking: boolean;
  primaryResource?: AppointmentResource;
}

export interface WaitlistEntry {
  id: string;
  patientId: string;
  status: string;
  priority: number;
  notes?: string | null;
}

export interface AppointmentDensity {
  date: string;
  count: number;
}
