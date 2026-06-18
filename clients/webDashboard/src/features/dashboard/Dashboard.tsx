import { useDashboard } from './DashboardProvider';
import { TrendingUp, Bus, Users, DollarSign } from 'lucide-react';

export default function Dashboard() {
  const { totalJourneys, totalBuses, totalUsers, totalRevenue } = useDashboard();

  const stats = [
    {
      name: 'Toplam Sefer',
      value: totalJourneys.toLocaleString(),
      icon: TrendingUp,
      color: 'blue',
    },
    {
      name: 'Toplam Otobüs',
      value: totalBuses.toLocaleString(),
      icon: Bus,
      color: 'green',
    },
    {
      name: 'Toplam Kullanıcı',
      value: totalUsers.toLocaleString(),
      icon: Users,
      color: 'purple',
    },
    {
      name: 'Toplam Gelir',
      value: `₺${totalRevenue.toLocaleString()}`,
      icon: DollarSign,
      color: 'orange',
    },
  ];

  return (
    <div>
      <h1 className="text-2xl font-bold text-gray-900 dark:text-white mb-6">
        Özet
      </h1>

      <div className="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-4">
        {stats.map((stat) => {
          const Icon = stat.icon;
          const colorClasses = {
            blue: 'bg-blue-500',
            green: 'bg-green-500',
            purple: 'bg-purple-500',
            orange: 'bg-orange-500',
          };

          return (
            <div
              key={stat.name}
              className="overflow-hidden bg-white dark:bg-gray-800 rounded-lg shadow p-5"
            >
              <div className="flex items-center">
                <div className={`flex-shrink-0 rounded-md p-3 ${colorClasses[stat.color as keyof typeof colorClasses]}`}>
                  <Icon className="h-6 w-6 text-white" aria-hidden="true" />
                </div>
                <div className="ml-5 w-0 flex-1">
                  <dl>
                    <dt className="text-sm font-medium text-gray-500 dark:text-gray-400 truncate">
                      {stat.name}
                    </dt>
                    <dd>
                      <div className="text-lg font-medium text-gray-900 dark:text-white">
                        {stat.value}
                      </div>
                    </dd>
                  </dl>
                </div>
              </div>
            </div>
          );
        })}
      </div>

      {/* Recent Activity Section */}
      <div className="mt-8 bg-white dark:bg-gray-800 shadow rounded-lg">
        <div className="px-4 py-5 sm:px-6">
          <h3 className="text-lg leading-6 font-medium text-gray-900 dark:text-white">
            Son Aktiviteler
          </h3>
        </div>
        <div className="border-t border-gray-200 dark:border-gray-700">
          <div className="px-4 py-4 sm:px-6">
            <p className="text-sm text-gray-500 dark:text-gray-400">
              Son 24 saat içinde 125 yeni bilet satışı ve 45 yeni kullanıcı kaydı yapıldı.
            </p>
          </div>
        </div>
      </div>
    </div>
  );
}
