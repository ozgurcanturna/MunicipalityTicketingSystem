import { createContext, useContext, type ReactNode } from 'react';

interface DashboardContextType {
  totalJourneys: number;
  totalBuses: number;
  totalUsers: number;
  totalRevenue: number;
}

const DashboardContext = createContext<DashboardContextType | undefined>(undefined);

interface DashboardProviderProps {
  children: ReactNode;
}

export function DashboardProvider({ children }: DashboardProviderProps) {
  // TODO: Fetch real data from API
  const value: DashboardContextType = {
    totalJourneys: 1250,
    totalBuses: 45,
    totalUsers: 3420,
    totalRevenue: 125000,
  };

  return (
    <DashboardContext.Provider value={value}>
      {children}
    </DashboardContext.Provider>
  );
}

export function useDashboard() {
  const context = useContext(DashboardContext);
  if (context === undefined) {
    throw new Error('useDashboard must be used within a DashboardProvider');
  }
  return context;
}
