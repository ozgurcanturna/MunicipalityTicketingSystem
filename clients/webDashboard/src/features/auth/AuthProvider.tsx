import { createContext, useContext, useState, useEffect, type ReactNode } from 'react';
import { useNavigate } from 'react-router-dom';
import { signalRHubConnection } from '../../lib/signalR';
import { apiService } from '../../lib/api';
import type { User, AuthState } from '../../types/auth.types';

interface AuthContextType extends AuthState {
  login: (username: string, password: string, tenantId?: string) => Promise<void>;
  logout: () => Promise<void>;
  updateUserTenant: (tenantId: string) => void;
  refreshCurrentUser: () => Promise<void>;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

interface AuthProviderProps {
  children: ReactNode;
}

const STORAGE_KEYS = {
  token: 'auth_token',
  refreshToken: 'refresh_token',
  user: 'auth_user',
  tenant: 'auth_tenant',
} as const;

export function AuthProvider({ children }: AuthProviderProps) {
  const [state, setState] = useState<AuthState>({
    user: null,
    isAuthenticated: false,
    isLoading: true,
    tenantId: '',
  });
  const navigate = useNavigate();

  const loadAuthFromStorage = () => {
    const token = localStorage.getItem(STORAGE_KEYS.token);
    const userData = localStorage.getItem(STORAGE_KEYS.user);
    const tenantId = localStorage.getItem(STORAGE_KEYS.tenant);

        if (token && userData) {
      try {
        const parsedUser = JSON.parse(userData) as User;
        const resolvedTenantId = tenantId || parsedUser.tenantId || '';
        setState({
          user: parsedUser,
          isAuthenticated: true,
          isLoading: false,
          tenantId: resolvedTenantId,
        });

        // Start SignalR connection with current tenant
        if (resolvedTenantId) {
          signalRHubConnection.start();
        }
      } catch (error) {
        console.error('Error parsing user data:', error);
        clearAuth();
      }
    } else {
      setState(prev => ({ ...prev, isLoading: false }));
    }
  };

  useEffect(() => {
    loadAuthFromStorage();
  }, []);

  const clearAuth = () => {
    localStorage.removeItem(STORAGE_KEYS.token);
    localStorage.removeItem(STORAGE_KEYS.refreshToken);
    localStorage.removeItem(STORAGE_KEYS.user);
    localStorage.removeItem(STORAGE_KEYS.tenant);
    signalRHubConnection.stop();
  };

  const login = async (username: string, password: string, tenantId: string = 'bursa') => {
    try {
      // Store tenant temporarily for login request
      localStorage.setItem(STORAGE_KEYS.tenant, tenantId);

      const response = await apiService.login({
        username,
        password,
      });

      // Store tokens
      localStorage.setItem(STORAGE_KEYS.token, response.accessToken);
      localStorage.setItem(STORAGE_KEYS.refreshToken, response.refreshToken);
      localStorage.setItem(STORAGE_KEYS.user, JSON.stringify(response.user));
      localStorage.setItem(STORAGE_KEYS.tenant, response.user.tenantId);

      setState({
        user: response.user,
        isAuthenticated: true,
        isLoading: false,
        tenantId: response.user.tenantId,
      });

      // Start SignalR connection
      signalRHubConnection.start();

      navigate('/');
    } catch (error) {
      console.error('Login error:', error);

      // Clear any partial auth data
      clearAuth();
      localStorage.setItem(STORAGE_KEYS.tenant, tenantId);

      const errorMessage = (error as Error).message || 'Giriş başarısız';
      throw new Error(errorMessage);
    }
  };

  const logout = async () => {
    apiService.clearAuth();
    setState({
      user: null,
      isAuthenticated: false,
      isLoading: false,
      tenantId: '',
    });
    navigate('/login');
  };

  const updateUserTenant = (tenantId: string) => {
    localStorage.setItem(STORAGE_KEYS.tenant, tenantId);
    setState(prev => ({ ...prev, tenantId }));
  };

    const refreshCurrentUser = async () => {
    try {
      const user = await apiService.getCurrentUser();
      setState(prev => ({
        ...prev,
        user,
      }));
      localStorage.setItem(STORAGE_KEYS.user, JSON.stringify(user));
    } catch (error) {
      console.error('Error refreshing user:', error);
      throw error;
    }
  };

  if (state.isLoading) {
    return null; // Or a loading spinner
  }
  return (
    <AuthContext.Provider
      value={{
        ...state,
        login,
        logout,
        updateUserTenant,
        refreshCurrentUser,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (context === undefined) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
}

