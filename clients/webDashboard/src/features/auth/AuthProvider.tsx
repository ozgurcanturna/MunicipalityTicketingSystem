import { createContext, useContext, useState, useEffect, type ReactNode } from 'react';
import { useNavigate } from 'react-router-dom';
import { signalRHubConnection } from '../../lib/signalR';

interface AuthContextType {
  user: User | null;
  isLoading: boolean;
  login: (username: string, password: string) => Promise<void>;
  logout: () => Promise<void>;
  isAuthenticated: boolean;
}

interface User {
  id: string;
  username: string;
  email: string;
  role: string;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

interface AuthProviderProps {
  children: ReactNode;
}

export function AuthProvider({ children }: AuthProviderProps) {
  const [user, setUser] = useState<User | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const navigate = useNavigate();

  useEffect(() => {
    // Check if user is logged in on mount
    const token = localStorage.getItem('token');
    if (token) {
      setUser({
        id: '1',
        username: 'admin',
        email: 'admin@example.com',
        role: 'admin',
      });
      signalRHubConnection.start();
    }
    setIsLoading(false);
  }, []);

  const login = async (username: string, password: string) => {
    // TODO: Implement actual API call
    // For now, simulate authentication
    const mockToken = 'mock-jwt-token';
    localStorage.setItem('token', mockToken);
    
    setUser({
      id: '1',
      username,
      email: `${username}@example.com`,
      role: 'admin',
    });
    
    signalRHubConnection.start();
    navigate('/');
  };

  const logout = async () => {
    localStorage.removeItem('token');
    setUser(null);
    signalRHubConnection.stop();
  };

  const isAuthenticated = !!user;

  return (
    <AuthContext.Provider
      value={{
        user,
        isLoading,
        login,
        logout,
        isAuthenticated,
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
