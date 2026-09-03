import { useTranslation } from 'react-i18next';
import { usePrinters } from '../hooks/usePrinters';
import { PrinterStatusBadge } from '../components/PrinterStatusBadge';

export function Printers() {
  const { t } = useTranslation();
  const { data: printers, isLoading } = usePrinters();

  if (isLoading) {
    return <div className="p-8 text-gray-500">{t('common.loading')}</div>;
  }

  return (
    <div className="space-y-4">
      <h1 className="text-2xl font-bold text-gray-900">{t('nav.printers')}</h1>

      <div className="grid grid-cols-1 gap-4 md:grid-cols-2 lg:grid-cols-3">
        {printers?.map((printer) => (
          <div key={printer.id} className="rounded-lg border border-gray-200 bg-white p-5 shadow-sm">
            <div className="flex items-center justify-between">
              <h3 className="text-lg font-medium text-gray-900">{printer.name}</h3>
              <PrinterStatusBadge isOnline={printer.isOnline} />
            </div>
            <p className="mt-1 text-sm text-gray-500">{printer.model}</p>
            <div className="mt-3 space-y-1 text-sm text-gray-600">
              <p>{t('printer.ip')}: {printer.ipAddress}:{printer.port}</p>
              <p>Protocol: {printer.protocol}</p>
              <p>Color: {printer.colorCapability}</p>
            </div>
            <div className="mt-3 text-xs text-gray-400">
              {printer.queueProfiles.length} queue profile(s)
              {' | '}
              {printer.trayProfiles.length} tray profile(s)
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}
