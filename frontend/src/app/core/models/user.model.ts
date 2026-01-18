export interface User {
  id: string;
  username: string;
  fullName: string;
  email: string;
  role: string;
  managerId?: string;
  departmentId?: string;
  isActive: boolean;
}

export interface AuthState {
  user: User | null;
  token: string | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  error: string | null;
}
