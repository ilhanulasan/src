export interface UserProfile {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  phoneNumber?: string | null;
  address?: string | null;
  pictureUrl?: string | null;
  roles: string[];
}

export interface AuthResponse {
  token: string;
  user: UserProfile;
}
