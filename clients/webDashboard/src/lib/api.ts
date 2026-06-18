import axios, { AxiosError, type AxiosInstance, type InternalAxiosRequestConfig } from 'axios';
import type { AuthResponse, LoginCredentials, User, UserRole } from '../types/auth.types';

const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5197';

const STORAGE_KEYS = {
  token: 'auth_token',
  user: 'auth_user',
  tenant: 'auth_tenant',
} as const;

type JsonObject = Record<string, unknown>;

type RetryableRequestConfig = InternalAxiosRequestConfig & {
  _retry?: boolean;
};

interface BackendTokenResponse {
  accessToken: string;
  expiresAtUtc: string;
  userId: string;
  tenantId: string;
  email: string;
  role: string;
}

interface BootstrapAuthResponse {
  token: BackendTokenResponse;
}

interface BackendCurrentUserResponse {
  userId: string;
  email: string;
  name: string | null;
  role: string;
  tenantId: string;
}

function normalizeRole(role: string): UserRole {
  const normalizedRole = role.toLowerCase();

  if (normalizedRole === 'admin') {
    return 'admin';
  }

  if (normalizedRole === 'operator') {
    return 'operator';
  }

  if (normalizedRole === 'user') {
    return 'user';
  }

  return 'viewer';
}

function createUsername(email: string): string {
  return email.split('@')[0] ?? email;
}

function mapTokenResponse(response: BackendTokenResponse): AuthResponse {
  const user: User = {
    id: response.userId,
    username: createUsername(response.email),
    email: response.email,
    role: normalizeRole(response.role),
    tenantId: response.tenantId,
    permissions: [],
    createdAt: new Date().toISOString(),
  };
  user.fullName = response.email;

  return {
    accessToken: response.accessToken,
    expiresAt: response.expiresAtUtc,
    userId: response.userId,
    tenantId: response.tenantId,
    email: response.email,
    role: user.role,
    user,
  };
}

function mapCurrentUserResponse(response: BackendCurrentUserResponse): User {
  const user: User = {
    id: response.userId,
    username: createUsername(response.email),
    email: response.email,
    role: normalizeRole(response.role),
    tenantId: response.tenantId,
    permissions: [],
  };

  if (response.name) {
    user.fullName = response.name;
  }

  return user;
}

function readStorageItem(key: string): string | null {
  if (typeof window === 'undefined') {
    return null;
  }

  return window.localStorage.getItem(key);
}

function removeStorageItem(key: string): void {
  if (typeof window === 'undefined') {
    return;
  }

  window.localStorage.removeItem(key);
}

class ApiService {
  private readonly client: AxiosInstance;

  constructor() {
    this.client = axios.create({
      baseURL: API_BASE_URL,
      headers: {
        'Content-Type': 'application/json',
      },
    });

    this.setupInterceptors();
  }

  private setupInterceptors(): void {
    this.client.interceptors.request.use((config: InternalAxiosRequestConfig) => {
      const token = readStorageItem(STORAGE_KEYS.token);
      if (token) {
        config.headers.Authorization = `Bearer ${token}`;
      }

      const tenantId = readStorageItem(STORAGE_KEYS.tenant);
      if (tenantId) {
        config.headers['X-Tenant-Id'] = tenantId;
      }

      return config;
    });

    this.client.interceptors.response.use(
      (response) => response,
      async (error: AxiosError) => {
        const originalRequest = error.config as RetryableRequestConfig | undefined;

        if (error.response?.status === 401 && originalRequest && !originalRequest._retry) {
          originalRequest._retry = true;
          this.clearAuth();
          return Promise.reject(error);
        }

        return Promise.reject(error);
      },
    );
  }

  public clearAuth(redirect = true): void {
    removeStorageItem(STORAGE_KEYS.token);
    removeStorageItem(STORAGE_KEYS.user);
    removeStorageItem(STORAGE_KEYS.tenant);

    if (redirect && typeof window !== 'undefined') {
      window.location.href = '/login';
    }
  }

  async login(credentials: LoginCredentials): Promise<AuthResponse> {
    const response = await this.client.post<BackendTokenResponse>('/api/identity/auth/login', {
      tenantId: credentials.tenantId,
      email: credentials.email,
      password: credentials.password,
    });

    return mapTokenResponse(response.data);
  }

  async bootstrapTenant(request: {
    tenantName: string;
    adminEmail: string;
    adminFullName: string;
    adminPassword: string;
  }): Promise<AuthResponse> {
    const response = await this.client.post<BootstrapAuthResponse>('/api/identity/auth/bootstrap', request);
    return mapTokenResponse(response.data.token);
  }

  async getCurrentUser(): Promise<User> {
    const response = await this.client.get<BackendCurrentUserResponse>('/api/identity/auth/me');
    return mapCurrentUserResponse(response.data);
  }

  async getTenants(): Promise<User[]> {
    return [];
  }

  async getTenant(id: string): Promise<User> {
    const response = await this.client.get<BackendCurrentUserResponse>(`/api/identity/tenants/${id}`);
    return mapCurrentUserResponse(response.data);
  }

  async createTenant(name: string): Promise<User> {
    const response = await this.client.post<BackendCurrentUserResponse>('/api/identity/tenants', { name });
    return mapCurrentUserResponse(response.data);
  }

  async addUser(tenantId: string, request: {
    email: string;
    fullName: string;
    password: string;
    role: string;
  }): Promise<User> {
    const response = await this.client.post<BackendCurrentUserResponse>(`/api/identity/tenants/${tenantId}/users`, request);
    return mapCurrentUserResponse(response.data);
  }

  async createWallet(tenantId: string): Promise<JsonObject> {
    const response = await this.client.post<JsonObject>('/api/wallet/wallets', { tenantId });
    return response.data;
  }

  async getWallet(walletId: string): Promise<JsonObject> {
    const response = await this.client.get<JsonObject>(`/api/wallet/wallets/${walletId}`);
    return response.data;
  }

  async topUpWallet(walletId: string, amount: number, reference: string): Promise<JsonObject> {
    const response = await this.client.post<JsonObject>(`/api/wallet/wallets/${walletId}/topups`, {
      amount,
      reference,
    });
    return response.data;
  }

  async spendWallet(walletId: string, amount: number, reference: string): Promise<JsonObject> {
    const response = await this.client.post<JsonObject>(`/api/wallet/wallets/${walletId}/spend`, {
      amount,
      reference,
    });
    return response.data;
  }

  async getWalletTransactions(walletId: string): Promise<JsonObject[]> {
    const response = await this.client.get<JsonObject[]>(`/api/wallet/wallets/${walletId}/transactions`);
    return response.data;
  }

  async startJourney(request: {
    tenantId: string;
    vehicleId: string;
    routeCode: string;
    latitude: number;
    longitude: number;
  }): Promise<JsonObject> {
    const response = await this.client.post<JsonObject>('/api/telemetry/journeys/start', request);
    return response.data;
  }

  async checkIn(journeyId: string, cardId: string, stopCode: string): Promise<JsonObject> {
    const response = await this.client.post<JsonObject>(`/api/telemetry/journeys/${journeyId}/checkin`, { cardId, stopCode });
    return response.data;
  }

  async checkOut(journeyId: string, cardId: string, stopCode: string): Promise<JsonObject> {
    const response = await this.client.post<JsonObject>(`/api/telemetry/journeys/${journeyId}/checkout`, {
      cardId,
      stopCode,
    });
    return response.data;
  }

  async completeJourney(journeyId: string): Promise<JsonObject> {
    const response = await this.client.post<JsonObject>(`/api/telemetry/journeys/${journeyId}/complete`);
    return response.data;
  }

  async getActiveJourneys(busCode: string): Promise<JsonObject> {
    const response = await this.client.get<JsonObject>(`/api/telemetry/journeys/active/${busCode}`);
    return response.data;
  }

  async updateLocation(journeyId: string, request: {
    latitude: number;
    longitude: number;
    source: string;
  }): Promise<JsonObject> {
    const response = await this.client.post<JsonObject>(`/api/telemetry/journeys/${journeyId}/locations`, request);
    return response.data;
  }
}

export const apiService = new ApiService();
export { STORAGE_KEYS };
