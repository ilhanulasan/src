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
  dateOfBirth: string;
  gender: string;
  education: EducationLevel;
}
