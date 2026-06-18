import { useState, useEffect } from 'react';
import { BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, Legend, ResponsiveContainer, LineChart, Line } from 'recharts';
import { Download, Calendar, TrendingUp, DollarSign, Bus } from 'lucide-react';

interface RevenueData {
  date: string;
  revenue: number;
  tickets: number;
}

interface JourneyStats {
  totalJourneys: number;
  completedJourneys: number;
  activeJourneys: number;
  cancelledJourneys: number;
}

export default function Reports() {
  const [revenueData, setRevenueData] = useState<RevenueData[]>([]);
  const [journeyStats, setJourneyStats] = useState<JourneyStats | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [selectedPeriod, setSelectedPeriod] = useState<'week' | 'month' | 'year'>('month');

  const fetchReports = async () => {
    try {
      setIsLoading(true);
      
      // Mock data for now
      const mockRevenueData: RevenueData[] = [
        { date: '1 Oca', revenue: 12500, tickets: 245 },
        { date: '8 Oca', revenue: 15300, tickets: 298 },
        { date: '15 Oca', revenue: 18700, tickets: 356 },
        { date: '22 Oca', revenue: 14200, tickets: 278 },
        { date: '29 Oca', revenue: 16800, tickets: 312 },
        { date: '5 Şub', revenue: 19500, tickets: 378 },
        { date: '12 Şub', revenue: 21200, tickets: 405 },
      ];

      setRevenueData(mockRevenueData);
      
      setJourneyStats({
        totalJourneys: 1250,
        completedJourneys: 1180,
        activeJourneys: 45,
        cancelledJourneys: 25,
      });
    } catch (error) {
      console.error('Error fetching reports:', error);
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    fetchReports();
  }, [selectedPeriod]);

  const totalRevenue = revenueData.reduce((sum, day) => sum + day.revenue, 0);
  const totalTickets = revenueData.reduce((sum, day) => sum + day.tickets, 0);

  const exportReport = () => {
    alert('Rapor PDF olarak dışa aktarılacak');
  };

  return (
    <div>
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-2xl font-bold text-gray-900 dark:text-white">
          Raporlar ve İstatistikler
        </h1>
                <button
          type="button"
          onClick={exportReport}
          className="flex items-center px-4 py-2 text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 rounded-md"
          aria-label="Rapor indir"
        >
          <Download className="h-4 w-4 mr-2" />
          Rapor İndir
        </button>
      </div>

      <div className="flex gap-4 mb-6">
              <select
                value={selectedPeriod}
                onChange={(e) => setSelectedPeriod(e.target.value as 'week' | 'month' | 'year')}
                className="px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-md focus:ring-blue-500 focus:border-blue-500 dark:bg-gray-700 dark:text-white"
                aria-label="Rapor dönemi seçin"
              >
                <option value="week">Son 7 Gün</option>
                <option value="month">Son 30 Gün</option>
                <option value="year">Son 1 Yıl</option>
              </select>
            </div>

      {isLoading ? (
        <div className="flex items-center justify-center min-h-64">
          <p className="text-gray-500 dark:text-gray-400">Yükleniyor...</p>
        </div>
      ) : (
        <>
          {/* Summary Cards */}
          <div className="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-4 mb-6">
            <div className="overflow-hidden bg-white dark:bg-gray-800 rounded-lg shadow p-5">
              <div className="flex items-center">
                <div className="flex-shrink-0 rounded-md p-3 bg-blue-500">
                  <DollarSign className="h-6 w-6 text-white" />
                </div>
                <div className="ml-5 w-0 flex-1">
                  <dl>
                    <dt className="text-sm font-medium text-gray-500 dark:text-gray-400 truncate">
                      Toplam Gelir
                    </dt>
                    <dd>
                      <div className="text-lg font-medium text-gray-900 dark:text-white">
                        ₺{totalRevenue.toLocaleString()}
                      </div>
                    </dd>
                  </dl>
                </div>
              </div>
            </div>

            <div className="overflow-hidden bg-white dark:bg-gray-800 rounded-lg shadow p-5">
              <div className="flex items-center">
                <div className="flex-shrink-0 rounded-md p-3 bg-green-500">
                  <Bus className="h-6 w-6 text-white" />
                </div>
                <div className="ml-5 w-0 flex-1">
                  <dl>
                    <dt className="text-sm font-medium text-gray-500 dark:text-gray-400 truncate">
                      Toplam Bilet
                    </dt>
                    <dd>
                      <div className="text-lg font-medium text-gray-900 dark:text-white">
                        {totalTickets.toLocaleString()}
                      </div>
                    </dd>
                  </dl>
                </div>
              </div>
            </div>

            <div className="overflow-hidden bg-white dark:bg-gray-800 rounded-lg shadow p-5">
              <div className="flex items-center">
                <div className="flex-shrink-0 rounded-md p-3 bg-purple-500">
                  <TrendingUp className="h-6 w-6 text-white" />
                </div>
                <div className="ml-5 w-0 flex-1">
                  <dl>
                    <dt className="text-sm font-medium text-gray-500 dark:text-gray-400 truncate">
                      Tamamlanan Sefer
                    </dt>
                    <dd>
                      <div className="text-lg font-medium text-gray-900 dark:text-white">
                        {journeyStats?.completedJourneys.toLocaleString() || '0'}
                      </div>
                    </dd>
                  </dl>
                </div>
              </div>
            </div>

            <div className="overflow-hidden bg-white dark:bg-gray-800 rounded-lg shadow p-5">
              <div className="flex items-center">
                <div className="flex-shrink-0 rounded-md p-3 bg-orange-500">
                  <Calendar className="h-6 w-6 text-white" />
                </div>
                <div className="ml-5 w-0 flex-1">
                  <dl>
                    <dt className="text-sm font-medium text-gray-500 dark:text-gray-400 truncate">
                      Aktif Sefer
                    </dt>
                    <dd>
                      <div className="text-lg font-medium text-gray-900 dark:text-white">
                        {journeyStats?.activeJourneys.toLocaleString() || '0'}
                      </div>
                    </dd>
                  </dl>
                </div>
              </div>
            </div>
          </div>

          {/* Revenue Chart */}
          <div className="bg-white dark:bg-gray-800 shadow rounded-lg mb-6">
            <div className="px-4 py-5 sm:px-6">
              <h3 className="text-lg leading-6 font-medium text-gray-900 dark:text-white">
                Gelir Grafiği
              </h3>
            </div>
            <div className="p-4">
              <ResponsiveContainer width="100%" height={300}>
                <LineChart data={revenueData}>
                  <CartesianGrid strokeDasharray="3 3" stroke="#374151" />
                  <XAxis dataKey="date" stroke="#9CA3AF" />
                  <YAxis stroke="#9CA3AF" />
                  <Tooltip
                    contentStyle={{
                      backgroundColor: '#1F2937',
                      border: 'none',
                      borderRadius: '8px',
                      color: '#F9FAFB',
                    }}
                  />
                  <Legend />
                  <Line type="monotone" dataKey="revenue" stroke="#3B82F6" strokeWidth={2} name="Gelir (₺)" />
                </LineChart>
              </ResponsiveContainer>
            </div>
          </div>

          {/* Ticket Sales Chart */}
          <div className="bg-white dark:bg-gray-800 shadow rounded-lg mb-6">
            <div className="px-4 py-5 sm:px-6">
              <h3 className="text-lg leading-6 font-medium text-gray-900 dark:text-white">
                Bilet Satış Grafiği
              </h3>
            </div>
            <div className="p-4">
              <ResponsiveContainer width="100%" height={300}>
                <BarChart data={revenueData}>
                  <CartesianGrid strokeDasharray="3 3" stroke="#374151" />
                  <XAxis dataKey="date" stroke="#9CA3AF" />
                  <YAxis stroke="#9CA3AF" />
                  <Tooltip
                    contentStyle={{
                      backgroundColor: '#1F2937',
                      border: 'none',
                      borderRadius: '8px',
                      color: '#F9FAFB',
                    }}
                  />
                  <Legend />
                  <Bar dataKey="tickets" fill="#10B981" name="Bilet Satışı" />
                </BarChart>
              </ResponsiveContainer>
            </div>
          </div>

          {/* Journey Statistics */}
          <div className="bg-white dark:bg-gray-800 shadow rounded-lg">
            <div className="px-4 py-5 sm:px-6">
              <h3 className="text-lg leading-6 font-medium text-gray-900 dark:text-white">
                Sefer İstatistikleri
              </h3>
            </div>
            <div className="p-4">
              <div className="grid grid-cols-2 gap-4">
                <div className="bg-blue-50 dark:bg-blue-900/20 rounded-lg p-4">
                  <p className="text-sm text-blue-600 dark:text-blue-400 mb-1">Toplam Sefer</p>
                  <p className="text-2xl font-bold text-gray-900 dark:text-white">
                    {journeyStats?.totalJourneys.toLocaleString() || '0'}
                  </p>
                </div>
                <div className="bg-green-50 dark:bg-green-900/20 rounded-lg p-4">
                  <p className="text-sm text-green-600 dark:text-green-400 mb-1">Tamamlanan</p>
                  <p className="text-2xl font-bold text-gray-900 dark:text-white">
                    {journeyStats?.completedJourneys.toLocaleString() || '0'}
                  </p>
                </div>
                <div className="bg-orange-50 dark:bg-orange-900/20 rounded-lg p-4">
                  <p className="text-sm text-orange-600 dark:text-orange-400 mb-1">Aktif</p>
                  <p className="text-2xl font-bold text-gray-900 dark:text-white">
                    {journeyStats?.activeJourneys.toLocaleString() || '0'}
                  </p>
                </div>
                <div className="bg-red-50 dark:bg-red-900/20 rounded-lg p-4">
                  <p className="text-sm text-red-600 dark:text-red-400 mb-1">İptal</p>
                  <p className="text-2xl font-bold text-gray-900 dark:text-white">
                    {journeyStats?.cancelledJourneys.toLocaleString() || '0'}
                  </p>
                </div>
              </div>
            </div>
          </div>
        </>
      )}
    </div>
  );
}
