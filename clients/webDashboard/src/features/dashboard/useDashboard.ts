import { createContext, useContext } from 'react';

export interface DashboardContextType {
  totalJourneys: number;
  totalBuses: number;
  totalUsers: number;
  totalRevenue: number;
  isLoading: boolean;
  error: string | null;
  refresh: () => Promise<void>;
}

export const DashboardContext = createContext<DashboardContextType | undefined>(undefined);

export function useDashboard() {
  const context = useContext(DashboardContext);
  if (context === undefined) {
    throw new Error('useDashboard must be used within a DashboardProvider');
  }
  return context;
}
