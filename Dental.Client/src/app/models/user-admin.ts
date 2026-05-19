export interface CreateUserRequest {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
  phoneNumber?: string | null;
  address?: string | null;
  roles: string[];
}

export interface UpdateUserRequest {
  email: string;
  firstName: string;
  lastName: string;
  phoneNumber?: string | null;
  address?: string | null;
  roles: string[];
}

export interface ResetUserPasswordRequest {
  newPassword: string;
}
