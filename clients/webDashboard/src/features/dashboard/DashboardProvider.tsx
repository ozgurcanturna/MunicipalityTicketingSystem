import { createContext, useContext, useState, useEffect, type ReactNode } from 'react';
interface DashboardContextType {
  totalJourneys: number;
  totalBuses: number;
  totalUsers: number;
  totalRevenue: number;
  isLoading: boolean;
  error: string | null;
  refresh: () => Promise<void>;
}

const DashboardContext = createContext<DashboardContextType | undefined>(undefined);

interface DashboardProviderProps {
  children: ReactNode;
}

export function DashboardProvider({ children }: DashboardProviderProps) {
  const [state, setState] = useState<DashboardContextType>({
    totalJourneys: 0,
    totalBuses: 0,
    totalUsers: 0,
    totalRevenue: 0,
    isLoading: true,
    error: null,
    refresh: async () => {},
  });

    const fetchDashboardData = async () => {
    try {
      setState(prev => ({ ...prev, isLoading: true, error: null }));

      // Use mock data for dashboard stats (backend endpoints not available yet)
      const usersCount = 24;
      const busesCount = 45;
      const journeysCount = 1250;
      const revenue = 125000;

      setState({
        totalJourneys: journeysCount,
        totalBuses: busesCount,
        totalUsers: usersCount,
        totalRevenue: revenue,
        isLoading: false,
        error: null,
    refresh: fetchDashboardData,
      });
    } catch (error) {
      console.error('Error fetching dashboard data:', error);
      setState(prev => ({
        ...prev,
        isLoading: false,
        error: 'Veri yüklenirken bir hata oluştu',
        refresh: fetchDashboardData,
      }));
    }
  };

  useEffect(() => {
    fetchDashboardData();
  }, []);

  const contextValue: DashboardContextType = {
    ...state,
    refresh: fetchDashboardData,
  };

  return (
    <DashboardContext.Provider value={contextValue}>
      {children}
    </DashboardContext.Provider>
  );
}

// Separate hook file for Fast Refresh compatibility
export function useDashboard() {
  const context = useContext(DashboardContext);
  if (context === undefined) {
    throw new Error('useDashboard must be used within a DashboardProvider');
  }
  return context;
}

