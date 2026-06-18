import { useState, useEffect } from 'react';
import { Bus, Search, Clock, User, MapPin, CheckCircle, XCircle } from 'lucide-react';
import { apiService } from '../../lib/api';

interface JourneyCheckpoint {
  id: string;
  checkpointType: 'board' | 'alight' | 'location';
  passengerName: string;
  stopName: string;
  latitude?: number;
  longitude?: number;
  timestamp: Date;
  fare?: number;
}

interface JourneySession {
  id: string;
  busCode: string;
  passengerName: string;
  boardingStop: string;
  destinationStop: string;
  checkpoints: JourneyCheckpoint[];
  status: 'active' | 'completed' | 'cancelled';
  createdAt: Date;
}

interface BackendJourneyResponse {
  id?: string;
  vehicleId?: string;
  routeCode?: string;
  isActive?: boolean;
  passengerCount?: number;
  currentLatitude?: number;
  currentLongitude?: number;
  startedAt?: string;
  endedAt?: string | null;
  checkpoints?: Array<{
    id?: string;
    eventType?: string;
    cardId?: string | null;
    stopCode?: string | null;
    latitude?: number | null;
    longitude?: number | null;
    occurredAt?: string;
  }>;
}

function mapJourneyResponse(response: BackendJourneyResponse): JourneySession {
  const checkpoints = (response.checkpoints || []).map((checkpoint) => {
    const mappedCheckpoint: JourneyCheckpoint = {
      id: checkpoint.id || crypto.randomUUID(),
      checkpointType: checkpoint.eventType === 'CHECK_OUT' ? 'alight' : checkpoint.eventType === 'CHECK_IN' ? 'board' : 'location',
      passengerName: checkpoint.cardId || 'Sistem',
      stopName: checkpoint.stopCode || 'Konum',
      timestamp: new Date(checkpoint.occurredAt || response.startedAt || Date.now()),
    };

    if (checkpoint.latitude !== null && checkpoint.latitude !== undefined) {
      mappedCheckpoint.latitude = checkpoint.latitude;
    }

    if (checkpoint.longitude !== null && checkpoint.longitude !== undefined) {
      mappedCheckpoint.longitude = checkpoint.longitude;
    }

    return mappedCheckpoint;
  });

  return {
    id: response.id || crypto.randomUUID(),
    busCode: response.vehicleId || response.routeCode || 'BUR-000',
    passengerName: `${response.passengerCount ?? 0} yolcu`,
    boardingStop: response.currentLatitude && response.currentLongitude
      ? `${response.currentLatitude.toFixed(4)}, ${response.currentLongitude.toFixed(4)}`
      : 'Bilinmiyor',
    destinationStop: response.routeCode || 'Rota',
    checkpoints,
    status: response.isActive === false ? 'completed' : 'active',
    createdAt: new Date(response.startedAt || Date.now()),
  };
}

export default function Journeys() {
  const [journeys, setJourneys] = useState<JourneySession[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [searchTerm, setSearchTerm] = useState('');

  const fetchJourneys = async (busCode?: string) => {
    try {
      setIsLoading(true);

      if (busCode) {
        const response = await apiService.getActiveJourneys(busCode);
        setJourneys([mapJourneyResponse(response)]);
      } else {
        setJourneys([
          {
            id: '1',
            busCode: 'BUR-001',
            passengerName: 'Ahmet Yılmaz',
            boardingStop: 'Çarşı',
            destinationStop: 'Otogar',
            checkpoints: [
              { id: 'c1', checkpointType: 'board', passengerName: 'Ahmet Yılmaz', stopName: 'Çarşı', timestamp: new Date('2024-01-15T08:30:00'), fare: 15 },
              { id: 'c2', checkpointType: 'location', passengerName: 'Ahmet Yılmaz', stopName: 'Merkez', latitude: 40.1885, longitude: 29.0610, timestamp: new Date('2024-01-15T08:45:00') },
            ],
            status: 'active',
            createdAt: new Date('2024-01-15T08:30:00'),
          },
        ]);
      }
    } catch (error) {
      console.error('Error fetching journeys:', error);
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    void fetchJourneys();
  }, []);

  const filteredJourneys = journeys.filter(journey =>
    journey.passengerName.toLowerCase().includes(searchTerm.toLowerCase()) ||
    journey.busCode.toLowerCase().includes(searchTerm.toLowerCase())
  );

  const getStatusBadge = (status: string) => {
    switch (status) {
      case 'active':
        return (
          <span className="flex items-center px-2 py-1 text-xs font-semibold text-green-800 bg-green-100 dark:bg-green-900/30 dark:text-green-400 rounded-full">
            <CheckCircle className="h-3 w-3 mr-1" aria-hidden="true" />
            Aktif
          </span>
        );
      case 'completed':
        return (
          <span className="flex items-center px-2 py-1 text-xs font-semibold text-blue-800 bg-blue-100 dark:bg-blue-900/30 dark:text-blue-400 rounded-full">
            <CheckCircle className="h-3 w-3 mr-1" aria-hidden="true" />
            Tamamlandı
          </span>
        );
      case 'cancelled':
        return (
          <span className="flex items-center px-2 py-1 text-xs font-semibold text-red-800 bg-red-100 dark:bg-red-900/30 dark:text-red-400 rounded-full">
            <XCircle className="h-3 w-3 mr-1" aria-hidden="true" />
            İptal
          </span>
        );
      default:
        return null;
    }
  };

  const formatTimestamp = (date: Date) => {
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
          Sefer Yönetimi
        </h1>
      </div>

      <div className="bg-white dark:bg-gray-800 shadow rounded-lg mb-6">
        <div className="p-4 border-b border-gray-200 dark:border-gray-700">
          <div className="relative">
            <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 h-5 w-5 text-gray-400" aria-hidden="true" />
            <input
              type="text"
              placeholder="Yolcu adı veya otobüs kodu ara..."
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
        ) : filteredJourneys.length === 0 ? (
          <div className="p-8 text-center">
            <Bus className="h-12 w-12 mx-auto text-gray-400 mb-4" aria-hidden="true" />
            <p className="text-gray-500 dark:text-gray-400">Henüz sefer kaydı yok</p>
          </div>
        ) : (
          <div className="divide-y divide-gray-200 dark:divide-gray-700">
            {filteredJourneys.map((journey) => (
              <div key={journey.id} className="p-4 hover:bg-gray-50 dark:hover:bg-gray-700/50">
                <div className="flex items-start justify-between">
                  <div className="flex-1">
                    <div className="flex items-center gap-3 mb-2">
                      {getStatusBadge(journey.status)}
                      <span className="text-sm font-medium text-gray-900 dark:text-white">
                        {journey.busCode}
                      </span>
                    </div>

                    <div className="space-y-2 text-sm">
                      <div className="flex items-center gap-2 text-gray-600 dark:text-gray-400">
                        <User className="h-4 w-4" aria-hidden="true" />
                        <span>{journey.passengerName}</span>
                      </div>

                      <div className="grid grid-cols-2 gap-4 mt-3">
                        <div className="flex items-start gap-2">
                          <MapPin className="h-4 w-4 mt-0.5 text-green-600" aria-hidden="true" />
                          <div>
                            <p className="text-xs text-gray-500 dark:text-gray-400">Kalkış</p>
                            <p className="font-medium">{journey.boardingStop}</p>
                          </div>
                        </div>

                        <div className="flex items-start gap-2">
                          <MapPin className="h-4 w-4 mt-0.5 text-red-600" aria-hidden="true" />
                          <div>
                            <p className="text-xs text-gray-500 dark:text-gray-400">Varış</p>
                            <p className="font-medium">{journey.destinationStop}</p>
                          </div>
                        </div>
                      </div>

                      <div className="flex items-center gap-2 mt-2 text-xs text-gray-500 dark:text-gray-400">
                        <Clock className="h-3 w-3" aria-hidden="true" />
                        <span>{formatTimestamp(journey.createdAt)}</span>
                      </div>
                    </div>
                  </div>
                </div>

                {journey.checkpoints.length > 0 && (
                  <div className="mt-4 pt-4 border-t border-gray-200 dark:border-gray-700">
                    <p className="text-xs font-medium text-gray-500 dark:text-gray-400 mb-2">Kontrol Noktaları</p>
                    <div className="space-y-2">
                      {journey.checkpoints.map((checkpoint) => (
                        <div key={checkpoint.id} className="flex items-center gap-3 text-sm">
                          <div className={`w-2 h-2 rounded-full ${
                            checkpoint.checkpointType === 'board' ? 'bg-green-500' :
                            checkpoint.checkpointType === 'alight' ? 'bg-red-500' :
                            'bg-blue-500'
                          }`} />
                          <span className="flex-1">{checkpoint.stopName}</span>
                          <span className="text-xs text-gray-500 dark:text-gray-400">
                            {formatTimestamp(checkpoint.timestamp)}
                          </span>
                          {checkpoint.fare && (
                            <span className="text-sm font-medium">₺{checkpoint.fare}</span>
                          )}
                        </div>
                      ))}
                    </div>
                  </div>
                )}
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
}
