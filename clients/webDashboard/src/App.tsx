import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { Routes, Route } from 'react-router-dom';
import { AuthProvider } from './features/auth/AuthProvider';
import { DashboardProvider } from './features/dashboard/DashboardProvider';
import Layout from './components/layout/Layout';
import Dashboard from './features/dashboard/Dashboard';
import Journeys from './features/journeys/Journeys';
import Buses from './features/buses/Buses';
import Reports from './features/reports/Reports';
import Users from './features/users/Users';
import Settings from './features/settings/Settings';
import Login from './features/auth/Login';
import RequireAuth from './components/auth/RequireAuth';
import './index.css';

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: 1000 * 60 * 5, // 5 dakika
      refetchOnWindowFocus: false,
    },
  },
});

function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <AuthProvider>
        <DashboardProvider>
          <Routes>
            <Route path="/login" element={<Login />} />
            <Route
              path="/"
              element={
                <RequireAuth>
                  <Layout />
                </RequireAuth>
              }
            >
              <Route index element={<Dashboard />} />
              <Route path="journeys" element={<Journeys />} />
              <Route path="buses" element={<Buses />} />
              <Route path="reports" element={<Reports />} />
              <Route path="users" element={<Users />} />
              <Route path="settings" element={<Settings />} />
            </Routes>
          </AuthProvider>
        </DashboardProvider>
      </QueryClientProvider>
  );
}

export default App;
