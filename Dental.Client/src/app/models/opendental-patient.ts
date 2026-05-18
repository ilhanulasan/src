/** Fields commonly returned by GET /patients (Open Dental REST API uses PascalCase). */
export interface OpenDentalPatient {
  PatNum?: number;
  FName?: string;
  LName?: string;
  PreferredName?: string;
  WirelessPhone?: string;
  HmPhone?: string;
  WkPhone?: string;
  Birthdate?: string;
  Email?: string;
  AddrNote?: string;
  EstBalance?: string;
}
