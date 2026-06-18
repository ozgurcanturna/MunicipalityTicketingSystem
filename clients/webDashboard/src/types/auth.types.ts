// Authentication & Authorization Types

export interface User {
  id: string;
  username: string;
  email: string;
  fullName?: string;
  role: UserRole;
  tenantId: string;
  permissions: string[];
  lastLoginAt?: string | Date;
  createdAt: Date;
  updatedAt?: Date;
}

export type UserRole = 'admin' | 'operator' | 'viewer';

export interface LoginCredentials {
  username: string;
  password: string;
}

export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  expiresAt: number;
  user: User;
}

export interface AuthState {
  user: User | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  tenantId: string;
}

// Token Storage
export interface TokenStorage {
  accessToken: string;
  refreshToken: string;
  expiresAt: number;
}

// Permission System
export interface Permission {
  resource: string;
  actions: string[];
}

export interface Role {
  name: UserRole;
  permissions: Permission[];
}
