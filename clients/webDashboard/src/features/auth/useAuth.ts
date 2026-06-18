import { createContext, useContext } from 'react';
import type { AuthState } from '../../types/auth.types';

export interface AuthContextType extends AuthState {
  login: (email: string, password: string, tenantId?: string) => Promise<void>;
  logout: () => Promise<void>;
  updateUserTenant: (tenantId: string) => void;
  refreshCurrentUser: () => Promise<void>;
}

export const AuthContext = createContext<AuthContextType | undefined>(undefined);

export function useAuth() {
  const context = useContext(AuthContext);
  if (context === undefined) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
}
