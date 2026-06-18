import { Link, Outlet, useLocation } from 'react-router-dom';
import { useAuth } from '../../features/auth';
import { LogOut, Home, Route, Bus, BarChart3, Users, Settings } from 'lucide-react';

const navigation = [
  { name: 'Özet', href: '/', icon: Home },
  { name: 'Seferler', href: '/journeys', icon: Route },
  { name: 'Otobüsler', href: '/buses', icon: Bus },
  { name: 'Raporlar', href: '/reports', icon: BarChart3 },
  { name: 'Kullanıcılar', href: '/users', icon: Users },
  { name: 'Ayarlar', href: '/settings', icon: Settings },
];

export default function Layout() {
  const location = useLocation();
  const { logout, user } = useAuth();

  const handleLogout = async () => {
    await logout();
  };

  return (
    <div className="min-h-screen bg-gray-50 dark:bg-gray-900">
      <aside className="fixed inset-y-0 left-0 z-50 w-64 bg-white dark:bg-gray-800 border-r border-gray-200 dark:border-gray-700">
        <div className="flex flex-col h-full">
          <div className="flex items-center justify-center h-16 border-b border-gray-200 dark:border-gray-700">
            <h1 className="text-xl font-bold text-blue-600 dark:text-blue-400">
              Belediyeye Bilet
            </h1>
          </div>

          <nav className="flex-1 px-4 py-6 space-y-2" aria-label="Ana menü">
            {navigation.map((item) => {
              const Icon = item.icon;
              const isActive = location.pathname === item.href;

              return (
                <Link
                  key={item.name}
                  to={item.href}
                  className={`flex items-center px-4 py-3 text-sm font-medium rounded-lg transition-colors ${
                    isActive
                      ? 'bg-blue-50 dark:bg-blue-900/20 text-blue-600 dark:text-blue-400'
                      : 'text-gray-700 dark:text-gray-300 hover:bg-gray-100 dark:hover:bg-gray-700'
                  }`}
                >
                  <Icon className="w-5 h-5 mr-3" aria-hidden="true" />
                  {item.name}
                </Link>
              );
            })}
          </nav>

          <div className="p-4 border-t border-gray-200 dark:border-gray-700">
            <button
              type="button"
              onClick={handleLogout}
              className="flex items-center w-full px-4 py-3 text-sm font-medium text-red-600 dark:text-red-400 rounded-lg hover:bg-red-50 dark:hover:bg-red-900/20 transition-colors"
            >
              <LogOut className="w-5 h-5 mr-3" aria-hidden="true" />
              Çıkış Yap
            </button>
          </div>
        </div>
      </aside>

      <div className="pl-64">
        <header className="flex items-center justify-end h-16 px-6 bg-white dark:bg-gray-800 border-b border-gray-200 dark:border-gray-700">
          <div className="flex items-center space-x-4">
            <span className="text-sm text-gray-600 dark:text-gray-400">
              {user?.fullName || user?.email || 'Admin'}
            </span>
            <div className="w-8 h-8 rounded-full bg-blue-600 flex items-center justify-center text-white font-medium" aria-hidden="true">
              {(user?.fullName || 'A').charAt(0)}
            </div>
          </div>
        </header>

        <main className="p-6">
          <Outlet />
        </main>
      </div>
    </div>
  );
}
