export const AppRoles = {
  Admin: 'Admin',
  Patient: 'Patient',
  Doctor: 'Doctor',
  Nurse: 'Nurse',
  Technician: 'Technician',
  Finance: 'Finance',
} as const;

export type AppRole = (typeof AppRoles)[keyof typeof AppRoles];

export const StaffRoles: AppRole[] = [AppRoles.Admin, AppRoles.Doctor];
export const AdminOnlyRoles: AppRole[] = [AppRoles.Admin];
export const PatientPortalRoles: AppRole[] = [AppRoles.Patient, AppRoles.Admin];
