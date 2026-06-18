import axios, { AxiosInstance, AxiosError } from 'axios';
import type { User, AuthResponse, LoginCredentials } from '../types/auth.types';

const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5197';

class ApiService {
  private client: AxiosInstance;

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
    // Request interceptor
    this.client.interceptors.request.use((config) => {
      const token = localStorage.getItem('auth_token');
      if (token) {
        config.headers.Authorization = `Bearer ${token}`;
      }
      
      // Add tenant header if available
      const tenantId = localStorage.getItem('auth_tenant');
      if (tenantId) {
        config.headers['X-Tenant-Id'] = tenantId;
      }

      return config;
    });

    // Response interceptor for token refresh
    this.client.interceptors.response.use(
      (response) => response,
      async (error: AxiosError) => {
        const originalRequest = error.config as any;

        // If 401 and not already retrying
        if (error.response?.status === 401 && !originalRequest._retry) {
          originalRequest._retry = true;

          try {
            const refreshToken = localStorage.getItem('refresh_token');
            if (refreshToken) {
              const newTokens = await this.refreshToken(refreshToken);
              
              // Update storage
              localStorage.setItem('auth_token', newTokens.accessToken);
              localStorage.setItem('refresh_token', newTokens.refreshToken);

              // Retry original request
              originalRequest.headers.Authorization = `Bearer ${newTokens.accessToken}`;
              return this.client(originalRequest);
            }
          } catch (refreshError) {
            // Refresh failed, clear auth
            this.clearAuth();
            throw refreshError;
          }
        }

        return Promise.reject(error);
      }
    );
  }

  private async refreshToken(refreshToken: string): Promise<{ accessToken: string; refreshToken: string }> {
    const response = await axios.post(`${API_BASE_URL}/auth/refresh`, {
      refreshToken,
    });
    return response.data;
  }

  public clearAuth(): void {
    localStorage.removeItem('auth_token');
    localStorage.removeItem('refresh_token');
    localStorage.removeItem('auth_user');
    localStorage.removeItem('auth_tenant');
    window.location.href = '/login';
  }

  // Auth endpoints
  async login(credentials: LoginCredentials): Promise<AuthResponse> {
    const response = await this.client.post<AuthResponse>('/api/identity/auth/login', {
      ...credentials,
      tenantId: localStorage.getItem('auth_tenant') || 'bursa',
    });
    return response.data;
  }

  async bootstrapTenant(request: {
    tenantName: string;
    adminEmail: string;
    adminFullName: string;
    adminPassword: string;
  }): Promise<AuthResponse> {
    const response = await this.client.post<AuthResponse>('/api/identity/auth/bootstrap', request);
    return response.data;
  }

  async getCurrentUser(): Promise<User> {
    const response = await this.client.get<User>('/api/identity/auth/me');
    return response.data;
  }

  // Tenant endpoints
  async getTenants(): Promise<User[]> {
    const response = await this.client.get<User[]>('/api/identity/tenants');
    return response.data;
  }

  async getTenant(id: string): Promise<User> {
    const response = await this.client.get<User>(`/api/identity/tenants/${id}`);
    return response.data;
  }

  async createTenant(name: string): Promise<User> {
    const response = await this.client.post<User>('/api/identity/tenants', { name });
    return response.data;
  }

  async addUser(tenantId: string, request: {
    email: string;
    fullName: string;
    password: string;
    role: string;
  }): Promise<User> {
    const response = await this.client.post<User>(`/api/identity/tenants/${tenantId}/users`, request);
    return response.data;
  }

  // Wallet endpoints
  async createWallet(tenantId: string): Promise<any> {
    const response = await this.client.post('/api/wallet/wallets', { tenantId });
    return response.data;
  }

  async getWallet(walletId: string): Promise<any> {
    const response = await this.client.get(`/api/wallet/wallets/${walletId}`);
    return response.data;
  }

  async topUpWallet(walletId: string, amount: number, reference: string): Promise<any> {
    const response = await this.client.post(`/api/wallet/wallets/${walletId}/topups`, {
      amount,
      reference,
    });
    return response.data;
  }

  async spendWallet(walletId: string, amount: number, reference: string): Promise<any> {
    const response = await this.client.post(`/api/wallet/wallets/${walletId}/spend`, {
      amount,
      reference,
    });
    return response.data;
  }

  async getWalletTransactions(walletId: string): Promise<any[]> {
    const response = await this.client.get(`/api/wallet/wallets/${walletId}/transactions`);
    return response.data;
  }

  // Journey endpoints
  async startJourney(request: {
    tenantId: string;
    busCode: string;
    passengerName: string;
    boardingStop: string;
    destinationStop: string;
    boardingTime: Date;
  }): Promise<any> {
    const response = await this.client.post('/api/telemetry/journeys/start', request);
    return response.data;
  }

  async checkIn(journeyId: string, passengerName: string): Promise<any> {
    const response = await this.client.post(`/api/telemetry/journeys/${journeyId}/checkin`, { passengerName });
    return response.data;
  }

  async checkOut(journeyId: string, passengerName: string, alightingStop: string): Promise<any> {
    const response = await this.client.post(`/api/telemetry/journeys/${journeyId}/checkout`, { 
      passengerName,
      alightingStop,
    });
    return response.data;
  }

  async completeJourney(journeyId: string): Promise<any> {
    const response = await this.client.post(`/api/telemetry/journeys/${journeyId}/complete`);
    return response.data;
  }

  async getActiveJourneys(busCode: string): Promise<any[]> {
    const response = await this.client.get(`/api/telemetry/journeys/active/${busCode}`);
    return response.data;
  }

  async updateLocation(journeyId: string, request: {
    passengerName: string;
    latitude: number;
    longitude: number;
    timestamp: Date;
  }): Promise<any> {
    const response = await this.client.post(`/api/telemetry/journeys/${journeyId}/location`, request);
    return response.data;
  }
}

export const apiService = new ApiService();