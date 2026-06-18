import { useState } from 'react';
import { Settings as SettingsIcon, Save, Moon, Sun, Bell, Shield } from 'lucide-react';
import { useAuth } from '../auth';

export default function Settings() {
  const { user, tenantId, refreshCurrentUser } = useAuth();
  const [isLoading, setIsLoading] = useState(false);
  const [successMessage, setSuccessMessage] = useState('');
  const [theme, setTheme] = useState<'light' | 'dark'>('light');
  const [notificationsEnabled, setNotificationsEnabled] = useState(true);
  const [settings, setSettings] = useState({
    language: 'tr',
    dateFormat: 'dd.MM.yyyy',
    currency: 'TRY',
    timezone: 'Europe/Istanbul',
  });

  const handleSaveSettings = async () => {
    try {
      setIsLoading(true);
      setSuccessMessage('Ayarlar kaydedildi');
      setTimeout(() => setSuccessMessage(''), 3000);
    } catch (error) {
      console.error('Error saving settings:', error);
    } finally {
      setIsLoading(false);
    }
  };

  const handleRefreshUser = async () => {
    try {
      setIsLoading(true);
      await refreshCurrentUser();
      setSuccessMessage('Kullanıcı bilgileri güncellendi');
      setTimeout(() => setSuccessMessage(''), 3000);
    } catch (error) {
      console.error('Error refreshing user:', error);
    } finally {
      setIsLoading(false);
    }
  };

  const toggleTheme = () => {
    const newTheme = theme === 'light' ? 'dark' : 'light';
    setTheme(newTheme);
    document.documentElement.classList.toggle('dark');
  };

  return (
    <div className="max-w-4xl">
      <h1 className="text-2xl font-bold text-gray-900 dark:text-white mb-6">
        Ayarlar
      </h1>

      {successMessage && (
        <div className="rounded-md bg-green-50 dark:bg-green-900/20 p-4 mb-6">
          <p className="text-sm text-green-800 dark:text-green-400">{successMessage}</p>
        </div>
      )}

      {/* Profile Settings */}
      <div className="bg-white dark:bg-gray-800 shadow rounded-lg mb-6">
        <div className="px-4 py-5 sm:px-6 border-b border-gray-200 dark:border-gray-700">
          <h2 className="text-lg font-medium text-gray-900 dark:text-white flex items-center">
            <Shield className="h-5 w-5 mr-2" />
            Profil Ayarları
          </h2>
        </div>
        <div className="px-4 py-5 sm:p-6">
                    <dl className="grid grid-cols-1 gap-x-4 gap-y-6 sm:grid-cols-2">
            <div>
              <dt className="text-sm font-medium text-gray-500 dark:text-gray-400">Kullanıcı Adı</dt>
              <dd className="mt-1 text-sm text-gray-900 dark:text-white">{user?.username}</dd>
            </div>
            <div>
              <dt className="text-sm font-medium text-gray-500 dark:text-gray-400">E-posta</dt>
              <dd className="mt-1 text-sm text-gray-900 dark:text-white">{user?.email}</dd>
            </div>
            <div>
              <dt className="text-sm font-medium text-gray-500 dark:text-gray-400">Rol</dt>
              <dd className="mt-1 text-sm text-gray-900 dark:text-white">
                {user?.role === 'admin' ? 'Yönetici' : user?.role === 'operator' ? 'Operatör' : 'İzleyici'}
              </dd>
            </div>
            <div>
              <dt className="text-sm font-medium text-gray-500 dark:text-gray-400">Tenant</dt>
              <dd className="mt-1 text-sm text-gray-900 dark:text-white">{tenantId}</dd>
            </div>
            <div>
              <dt className="text-sm font-medium text-gray-500 dark:text-gray-400">Kayıt Tarihi</dt>
              <dd className="mt-1 text-sm text-gray-900 dark:text-white">
                {user?.createdAt ? new Date(user.createdAt).toLocaleString('tr-TR') : '-'}
              </dd>
            </div>
          </dl>
                    <div className="mt-6 flex gap-3">
            <button
              type="button"
              onClick={handleRefreshUser}
              disabled={isLoading}
              className="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md text-white bg-blue-600 hover:bg-blue-700 disabled:opacity-50 disabled:cursor-not-allowed"
              aria-label="Kullanıcı bilgilerini yenile"
            >
                            Bilgileri Yenile
            </button>
          </div>
        </div>
      </div>

      {/* System Settings */}
      <div className="bg-white dark:bg-gray-800 shadow rounded-lg mb-6">
        <div className="px-4 py-5 sm:px-6 border-b border-gray-200 dark:border-gray-700">
          <h2 className="text-lg font-medium text-gray-900 dark:text-white flex items-center">
            <SettingsIcon className="h-5 w-5 mr-2" />
            Sistem Ayarları
          </h2>
        </div>
        <div className="px-4 py-5 sm:p-6">
          <div className="space-y-6">
                        {/* Theme */}
            <div className="sm:grid sm:grid-cols-3 sm:gap-4 sm:items-center sm:border-t sm:border-gray-200 sm:pt-5">
              <label htmlFor="settings-theme" className="flex items-center text-sm font-medium text-gray-900 dark:text-white sm:col-span-1">
                {theme === 'light' ? (
                  <Sun className="h-5 w-5 text-gray-500 mr-2" />
                ) : (
                  <Moon className="h-5 w-5 text-gray-500 mr-2" />
                )}
                Tema
              </label>
              <div className="sm:col-span-2 flex items-center justify-end">
                            <button
                type="button"
                onClick={toggleTheme}
                className="relative inline-flex h-6 w-11 flex-shrink-0 cursor-pointer rounded-full border-2 border-transparent transition-colors duration-200 ease-in-out focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2 bg-gray-200"
                aria-label="Tema değiştir"
              >
                <span
                  className={`${
                    theme === 'dark' ? 'translate-x-5' : 'translate-x-0'
                  } pointer-events-none inline-block h-5 w-5 transform rounded-full bg-white shadow ring-0 transition duration-200 ease-in-out`}
                />
              </button>
              </div>
            </div>

                        {/* Notifications */}
            <div className="sm:grid sm:grid-cols-3 sm:gap-4 sm:items-center sm:border-t sm:border-gray-200 sm:pt-5">
              <label htmlFor="settings-notifications" className="flex items-center text-sm font-medium text-gray-900 dark:text-white sm:col-span-1">
                <Bell className="h-5 w-5 text-gray-500 mr-2" />
                Bildirimler
              </label>
              <div className="sm:col-span-2 flex items-center justify-end">
                            <button
                type="button"
                onClick={() => setNotificationsEnabled(!notificationsEnabled)}
                className={`relative inline-flex h-6 w-11 flex-shrink-0 cursor-pointer rounded-full border-2 border-transparent transition-colors duration-200 ease-in-out focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2 ${
                  notificationsEnabled ? 'bg-blue-600' : 'bg-gray-200'
                }`}
                                aria-label="Bildirimleri aç/kapat"
              >
                <span
                  className={`${
                    notificationsEnabled ? 'translate-x-5' : 'translate-x-0'
                  } pointer-events-none inline-block h-5 w-5 transform rounded-full bg-white shadow ring-0 transition duration-200 ease-in-out`}
                />
                            </button>
                            </div>
            </div>

            {/* Language */}
            <div className="sm:grid sm:grid-cols-3 sm:gap-4 sm:items-start sm:border-t sm:border-gray-200 sm:pt-5">
              <label htmlFor="settings-language" className="text-sm font-medium text-gray-900 dark:text-white sm:col-span-1">
                Dil
              </label>
              <div className="mt-1 sm:col-span-2 sm:mt-0">
                <select
                  id="settings-language"
                  name="settings-language"
                  value={settings.language}
                  onChange={(e) => setSettings({ ...settings, language: e.target.value })}
                  className="block w-full pl-3 pr-10 py-2 text-base border-gray-300 dark:border-gray-600 focus:outline-none focus:ring-blue-500 focus:border-blue-500 sm:text-sm rounded-md dark:bg-gray-700 dark:text-white"
                >
                                    <option value="tr">Türkçe</option>
                  <option value="en">English</option>
                </select>
              </div>
            </div>

                        {/* Date Format */}
            <div className="sm:grid sm:grid-cols-3 sm:gap-4 sm:items-start sm:border-t sm:border-gray-200 sm:pt-5">
              <label htmlFor="settings-dateFormat" className="text-sm font-medium text-gray-900 dark:text-white sm:col-span-1">
                Tarih Formatı
              </label>
              <div className="mt-1 sm:col-span-2 sm:mt-0">
                <select
                  id="settings-dateFormat"
                  name="settings-dateFormat"
                  value={settings.dateFormat}
                  onChange={(e) => setSettings({ ...settings, dateFormat: e.target.value })}
                  className="block w-full pl-3 pr-10 py-2 text-base border-gray-300 dark:border-gray-600 focus:outline-none focus:ring-blue-500 focus:border-blue-500 sm:text-sm rounded-md dark:bg-gray-700 dark:text-white"
                >
                  <option value="dd.MM.yyyy">dd.MM.yyyy</option>
                  <option value="yyyy-MM-dd">yyyy-MM-dd</option>
                                    <option value="MM/dd/yyyy">MM/dd/yyyy</option>
                </select>
              </div>
            </div>

                        {/* Currency */}
            <div className="sm:grid sm:grid-cols-3 sm:gap-4 sm:items-start sm:border-t sm:border-gray-200 sm:pt-5">
              <label htmlFor="settings-currency" className="text-sm font-medium text-gray-900 dark:text-white sm:col-span-1">
                Para Birimi
              </label>
              <div className="mt-1 sm:col-span-2 sm:mt-0">
                <select
                  id="settings-currency"
                  name="settings-currency"
                  value={settings.currency}
                  onChange={(e) => setSettings({ ...settings, currency: e.target.value })}
                  className="block w-full pl-3 pr-10 py-2 text-base border-gray-300 dark:border-gray-600 focus:outline-none focus:ring-blue-500 focus:border-blue-500 sm:text-sm rounded-md dark:bg-gray-700 dark:text-white"
                >
                  <option value="TRY">₺ Türk Lirası</option>
                  <option value="USD">$ Dolar</option>
                                    <option value="EUR">€ Euro</option>
                </select>
              </div>
            </div>

                        {/* Timezone */}
            <div className="sm:grid sm:grid-cols-3 sm:gap-4 sm:items-start sm:border-t sm:border-gray-200 sm:pt-5">
              <label htmlFor="settings-timezone" className="text-sm font-medium text-gray-900 dark:text-white sm:col-span-1">
                Saat Dilimi
              </label>
              <div className="mt-1 sm:col-span-2 sm:mt-0">
                <select
                  id="settings-timezone"
                  name="settings-timezone"
                  value={settings.timezone}
                  onChange={(e) => setSettings({ ...settings, timezone: e.target.value })}
                  className="block w-full pl-3 pr-10 py-2 text-base border-gray-300 dark:border-gray-600 focus:outline-none focus:ring-blue-500 focus:border-blue-500 sm:text-sm rounded-md dark:bg-gray-700 dark:text-white"
                >
                  <option value="Europe/Istanbul">Europe/Istanbul (GMT+3)</option>
                  <option value="UTC">UTC (GMT+0)</option>
                                    <option value="America/New_York">America/New_York (GMT-5)</option>
                </select>
              </div>
            </div>
          </div>

                    <div className="mt-6 border-t border-gray-200 dark:border-gray-700 pt-6">
            <button
              type="button"
              onClick={handleSaveSettings}
              disabled={isLoading}
              className="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md text-white bg-blue-600 hover:bg-blue-700 disabled:opacity-50 disabled:cursor-not-allowed"
              aria-label="Ayarları kaydet"
            >
              <Save className="h-4 w-4 mr-2" />
                            Ayarları Kaydet
            </button>
          </div>
        </div>
      </div>

      {/* About */}
      <div className="bg-white dark:bg-gray-800 shadow rounded-lg">
        <div className="px-4 py-5 sm:px-6">
          <h2 className="text-lg font-medium text-gray-900 dark:text-white">Hakkında</h2>
        </div>
        <div className="px-4 py-5 sm:p-6">
          <div className="text-sm text-gray-500 dark:text-gray-400">
            <p className="mb-2">Belediye Bilet Sistemi - Web Dashboard</p>
            <p className="mb-2">Versiyon: 1.0.0</p>
            <p>Geliştirilmiş: 2024</p>
          </div>
        </div>
      </div>
    </div>
  );
}
