import { RoleType } from './role-type.enum';

export interface User {
  id: string;
  username: string;
  fullName: string;
  email: string;
  role: RoleType;
  managerId?: string;
  managerName?: string;
  departmentId?: string;
  departmentName?: string;
  isActive: boolean;
}

export interface AuthState {
  user: User | null;
  token: string | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  error: string | null;
}
