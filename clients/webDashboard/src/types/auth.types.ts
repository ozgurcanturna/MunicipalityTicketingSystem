export interface User {
  id: string;
  username: string;
  email: string;
  fullName?: string;
  role: UserRole;
  tenantId: string;
  permissions: string[];
  lastLoginAt?: string | Date;
  createdAt?: string | Date;
  updatedAt?: string | Date;
}

export type UserRole = 'admin' | 'operator' | 'viewer' | 'user';

export interface LoginCredentials {
  email: string;
  password: string;
  tenantId: string;
}

export interface AuthResponse {
  accessToken: string;
  expiresAt: string | number;
  userId: string;
  tenantId: string;
  email: string;
  role: UserRole;
  user: User;
}

export interface AuthState {
  user: User | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  tenantId: string;
}

export interface TokenStorage {
  accessToken: string;
  expiresAt: string | number;
}

export interface Permission {
  resource: string;
  actions: string[];
}

export interface Role {
  name: UserRole;
  permissions: Permission[];
}
