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
          const token = localStorage.getItem('token');
          return token || '';
        },
      })
      .withAutomaticReconnect()
      .configureLogging(LogLevel.Information)
      .build();

    try {
      await this.connection.start();
      console.log('SignalR Connected');
      return this.connection;
    } catch (err) {
      console.error('SignalR Connection Error:', err);
      throw err;
    }
  }

  async stop(): Promise<void> {
    if (this.connection) {
      await this.connection.stop();
      this.connection = null;
      console.log('SignalR Disconnected');
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
  `${import.meta.env.VITE_API_URL}/hubs/telemetry`
);
