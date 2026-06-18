import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr';

class SignalRService {
  private connection: HubConnection | null = null;
  private readonly hubUrl: string;

  constructor(hubUrl: string) {
    this.hubUrl = hubUrl;
  }

  async start(): Promise<void> {
    if (this.connection?.state === 'Connected') {
      return;
    }

    this.connection = new HubConnectionBuilder()
      .withUrl(this.hubUrl, {
        accessTokenFactory: () => {
          if (typeof window === 'undefined') {
            return '';
          }

          return window.localStorage.getItem('auth_token') || '';
        },
      })
      .withAutomaticReconnect()
      .configureLogging(LogLevel.Information)
      .build();

    try {
      await this.connection.start();
      console.log('SignalR Connected');
    } catch (err) {
      console.warn('SignalR connection skipped:', err);
      this.connection = null;
    }
  }

  async stop(): Promise<void> {
    if (this.connection) {
      try {
        await this.connection.stop();
      } catch (err) {
        console.warn('SignalR disconnect failed:', err);
      } finally {
        this.connection = null;
      }
    }
  }

  getConnection(): HubConnection | null {
    return this.connection;
  }

  on<T>(eventName: string, handler: (...args: T[]) => void): void {
    this.connection?.on(eventName, handler);
  }

  off(eventName: string): void {
    this.connection?.off(eventName);
  }

  async invoke<T>(methodName: string, ...args: unknown[]): Promise<T> {
    if (!this.connection) {
      throw new Error('Connection not established');
    }

    return await this.connection.invoke<T>(methodName, ...args);
  }
}

export const signalRHubConnection = new SignalRService(
  import.meta.env.VITE_SIGNALR_URL || `${import.meta.env.VITE_API_URL || 'http://localhost:5197'}/hubs/telemetry`
);
