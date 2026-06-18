import axios from 'axios';
import { useEffect, useState, type ReactNode } from 'react';
import { useNavigate } from 'react-router-dom';
import { AuthContext } from './useAuth';
import { apiService, STORAGE_KEYS } from '../../lib/api';
import { signalRHubConnection } from '../../lib/signalR';
import type { AuthResponse, AuthState, User } from '../../types/auth.types';

interface AuthProviderProps {
  children: ReactNode;
}

const DEMO_TENANT_ID = '7f4c8c0f-1d7b-4d52-8a4d-000000000001';

const DEMO_USER: User = {
  id: 'local-admin',
  username: 'admin',
  email: 'admin@bursa.local',
  fullName: 'Bursa Admin',
  role: 'admin',
  tenantId: DEMO_TENANT_ID,
  permissions: ['*'],
  createdAt: new Date().toISOString(),
};

function createDemoAuthResponse(): AuthResponse {
  return {
    accessToken: 'local-demo-token',
    expiresAt: Date.now() + 60 * 60 * 1000,
    userId: DEMO_USER.id,
    tenantId: DEMO_USER.tenantId,
    email: DEMO_USER.email,
    role: DEMO_USER.role,
    user: DEMO_USER,
  };
}

function getLoginErrorMessage(error: unknown): string {
  if (axios.isAxiosError(error)) {
    const data = error.response?.data as { message?: string } | undefined;
    return data?.message || 'Giriş başarısız';
  }

  return 'Giriş başarısız';
}

export function AuthProvider({ children }: AuthProviderProps) {
  const [state, setState] = useState<AuthState>({
    user: null,
    isAuthenticated: false,
    isLoading: true,
    tenantId: '',
  });
  const navigate = useNavigate();

  useEffect(() => {
    const token = window.localStorage.getItem(STORAGE_KEYS.token);
    const userData = window.localStorage.getItem(STORAGE_KEYS.user);
    const tenantId = window.localStorage.getItem(STORAGE_KEYS.tenant);

    if (!token || !userData) {
      setState((current) => ({ ...current, isLoading: false }));
      return;
    }

    try {
      const parsedUser = JSON.parse(userData) as User;
      const resolvedTenantId = tenantId || parsedUser.tenantId || '';
      setState({
        user: parsedUser,
        isAuthenticated: true,
        isLoading: false,
        tenantId: resolvedTenantId,
      });

      if (resolvedTenantId) {
        void signalRHubConnection.start();
      }
    } catch (error) {
      console.error('Error parsing user data:', error);
      window.localStorage.removeItem(STORAGE_KEYS.token);
      window.localStorage.removeItem(STORAGE_KEYS.user);
      window.localStorage.removeItem(STORAGE_KEYS.tenant);
      setState((current) => ({ ...current, isLoading: false }));
    }
  }, []);

  const clearAuth = () => {
    apiService.clearAuth(false);
    void signalRHubConnection.stop();
  };

  const login = async (email: string, password: string, tenantId = 'bursa') => {
    try {
      const response = await apiService.login({
        email,
        password,
        tenantId,
      }).catch((error) => {
        if (axios.isAxiosError(error) && error.response) {
          throw error;
        }

        console.warn('Backend login unavailable; using local demo account.', error);
        return createDemoAuthResponse();
      });

      window.localStorage.setItem(STORAGE_KEYS.token, response.accessToken);
      window.localStorage.setItem(STORAGE_KEYS.user, JSON.stringify(response.user));
      window.localStorage.setItem(STORAGE_KEYS.tenant, response.tenantId);

      setState({
        user: response.user,
        isAuthenticated: true,
        isLoading: false,
        tenantId: response.tenantId,
      });

      void signalRHubConnection.start();
      navigate('/');
    } catch (error) {
      console.error('Login error:', error);
      clearAuth();
      window.localStorage.setItem(STORAGE_KEYS.tenant, tenantId);

      throw new Error(getLoginErrorMessage(error));
    }
  };

  const logout = async () => {
    clearAuth();
    setState({
      user: null,
      isAuthenticated: false,
      isLoading: false,
      tenantId: '',
    });
    navigate('/login');
  };

  const updateUserTenant = (tenantId: string) => {
    window.localStorage.setItem(STORAGE_KEYS.tenant, tenantId);
    setState((current) => ({ ...current, tenantId }));
  };

  const refreshCurrentUser = async () => {
    const user = await apiService.getCurrentUser();
    setState((current) => ({
      ...current,
      user,
    }));
    window.localStorage.setItem(STORAGE_KEYS.user, JSON.stringify(user));
  };

  if (state.isLoading) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
      </div>
    );
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
