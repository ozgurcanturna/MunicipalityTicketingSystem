import { useState, useEffect } from 'react';
import { Users as UsersIcon, Search, Plus, Mail, Phone, Edit2, Trash2 } from 'lucide-react';
import { apiService } from '../../lib/api';
import type { User } from '../../types/auth.types';

interface ExtendedUser extends User {
  phone?: string;
  address?: string;
}

const DEFAULT_USERS: ExtendedUser[] = [
  {
    id: '1',
    username: 'admin',
    email: 'admin@bursa.local',
    fullName: 'Bursa Admin',
    role: 'admin',
    tenantId: '7f4c8c0f-1d7b-4d52-8a4d-000000000001',
    permissions: ['*'],
    phone: '+90 555 123 4567',
    address: 'Çiçek Mahallesi, Bursa',
    createdAt: new Date('2024-01-01T09:00:00'),
  },
  {
    id: '2',
    username: 'operator',
    email: 'operator@bursa.local',
    fullName: 'Bursa Operator',
    role: 'operator',
    tenantId: '7f4c8c0f-1d7b-4d52-8a4d-000000000001',
    permissions: ['journeys:read'],
    phone: '+90 555 123 4568',
    address: 'Merkez, Bursa',
    createdAt: new Date('2024-01-02T09:00:00'),
  },
];

export default function Users() {
  const [users, setUsers] = useState<ExtendedUser[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [searchTerm, setSearchTerm] = useState('');

  const fetchUsers = async () => {
    try {
      setIsLoading(true);
      const response = await apiService.getTenants();

      setUsers(response.map((user) => ({
        ...user,
        phone: '+90 555 123 4567',
        address: 'Çiçek Mahallesi, Bursa',
      })));
    } catch (error) {
      console.error('Error fetching users:', error);
      setUsers(DEFAULT_USERS);
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    void fetchUsers();
  }, []);

  const filteredUsers = users.filter((user) =>
    user.fullName?.toLowerCase().includes(searchTerm.toLowerCase()) ||
    user.email.toLowerCase().includes(searchTerm.toLowerCase()) ||
    user.username.toLowerCase().includes(searchTerm.toLowerCase())
  );

  const getRoleBadge = (role?: string) => {
    const roleLower = role?.toLowerCase() || '';
    if (roleLower === 'admin') {
      return (
        <span className="px-2 py-1 text-xs font-semibold text-purple-800 bg-purple-100 dark:bg-purple-900/30 dark:text-purple-400 rounded-full">
          Yönetici
        </span>
      );
    }
    if (roleLower === 'operator') {
      return (
        <span className="px-2 py-1 text-xs font-semibold text-blue-800 bg-blue-100 dark:bg-blue-900/30 dark:text-blue-400 rounded-full">
          Operatör
        </span>
      );
    }
    if (roleLower === 'user') {
      return (
        <span className="px-2 py-1 text-xs font-semibold text-gray-800 bg-gray-100 dark:bg-gray-700 dark:text-gray-300 rounded-full">
          Yolcu
        </span>
      );
    }
    return (
      <span className="px-2 py-1 text-xs font-semibold text-gray-800 bg-gray-100 dark:bg-gray-700 dark:text-gray-300 rounded-full">
        {role || 'Kullanıcı'}
      </span>
    );
  };

  const formatTimestamp = (date?: string | Date) => {
    if (!date) return '-';
    return new Date(date).toLocaleString('tr-TR', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    });
  };

  return (
    <div>
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-2xl font-bold text-gray-900 dark:text-white">
          Kullanıcı Yönetimi
        </h1>
        <button
          type="button"
          onClick={() => alert('Yeni kullanıcı ekleme formu açılacak')}
          className="flex items-center px-4 py-2 text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 rounded-md"
          aria-label="Yeni kullanıcı ekle"
        >
          <Plus className="h-4 w-4 mr-2" aria-hidden="true" />
          Yeni Kullanıcı
        </button>
      </div>

      <div className="bg-white dark:bg-gray-800 shadow rounded-lg">
        <div className="p-4 border-b border-gray-200 dark:border-gray-700">
          <div className="relative">
            <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 h-5 w-5 text-gray-400" aria-hidden="true" />
            <input
              type="text"
              placeholder="Kullanıcı ara..."
              value={searchTerm}
              onChange={(event) => setSearchTerm(event.target.value)}
              className="pl-10 pr-4 py-2 w-full border border-gray-300 dark:border-gray-600 rounded-md focus:ring-blue-500 focus:border-blue-500 dark:bg-gray-700 dark:text-white"
            />
          </div>
        </div>

        {isLoading ? (
          <div className="p-8 text-center">
            <p className="text-gray-500 dark:text-gray-400">Yükleniyor...</p>
          </div>
        ) : filteredUsers.length === 0 ? (
          <div className="p-8 text-center">
            <UsersIcon className="h-12 w-12 mx-auto text-gray-400 mb-4" aria-hidden="true" />
            <p className="text-gray-500 dark:text-gray-400">Henüz kullanıcı kaydı yok</p>
          </div>
        ) : (
          <div className="overflow-x-auto">
            <table className="min-w-full divide-y divide-gray-200 dark:divide-gray-700">
              <thead className="bg-gray-50 dark:bg-gray-700">
                <tr>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-300 uppercase">
                    Kullanıcı
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-300 uppercase">
                    E-posta
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-300 uppercase">
                    Rol
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-300 uppercase">
                    Telefon
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-300 uppercase">
                    Son Giriş
                  </th>
                  <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 dark:text-gray-300 uppercase">
                    İşlemler
                  </th>
                </tr>
              </thead>
              <tbody className="bg-white dark:bg-gray-800 divide-y divide-gray-200 dark:divide-gray-700">
                {filteredUsers.map((user) => (
                  <tr key={user.id}>
                    <td className="px-6 py-4 whitespace-nowrap">
                      <div>
                        <div className="text-sm font-medium text-gray-900 dark:text-white">
                          {user.fullName}
                        </div>
                        <div className="text-sm text-gray-500 dark:text-gray-400">
                          @{user.username}
                        </div>
                      </div>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      <div className="flex items-center gap-1 text-sm text-gray-900 dark:text-white">
                        <Mail className="h-3 w-3" aria-hidden="true" />
                        <span className="truncate max-w-xs">{user.email}</span>
                      </div>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      {getRoleBadge(user.role)}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900 dark:text-white">
                      <div className="flex items-center gap-1">
                        <Phone className="h-3 w-3" aria-hidden="true" />
                        {user.phone || '-'}
                      </div>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500 dark:text-gray-400">
                      {formatTimestamp(user.lastLoginAt)}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                      <button
                        type="button"
                        className="text-blue-600 hover:text-blue-900 dark:text-blue-400 dark:hover:text-blue-300 mr-3"
                        aria-label="Kullanıcıyı düzenle"
                      >
                        <Edit2 className="h-4 w-4" aria-hidden="true" />
                      </button>
                      <button
                        type="button"
                        className="text-red-600 hover:text-red-900 dark:text-red-400 dark:hover:text-red-300"
                        aria-label="Kullanıcıyı sil"
                      >
                        <Trash2 className="h-4 w-4" aria-hidden="true" />
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>
    </div>
  );
}
