import { useCallback, useEffect, useState, type ReactNode } from 'react';
import { DashboardContext, type DashboardContextType } from './useDashboard';

interface DashboardProviderProps {
  children: ReactNode;
}

export function DashboardProvider({ children }: DashboardProviderProps) {
  const [state, setState] = useState<Omit<DashboardContextType, 'refresh'>>({
    totalJourneys: 0,
    totalBuses: 0,
    totalUsers: 0,
    totalRevenue: 0,
    isLoading: true,
    error: null,
  });

  const fetchDashboardData = useCallback(async () => {
    try {
      setState((current) => ({ ...current, isLoading: true, error: null }));

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
      });
    } catch (error) {
      console.error('Error fetching dashboard data:', error);
      setState((current) => ({
        ...current,
        isLoading: false,
        error: 'Veri yüklenirken bir hata oluştu',
      }));
    }
  }, []);

  useEffect(() => {
    void fetchDashboardData();
  }, [fetchDashboardData]);

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
