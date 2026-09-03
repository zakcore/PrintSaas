import { useTranslation } from 'react-i18next';

interface Props {
  isOnline: boolean;
  status?: string | null;
}

export function PrinterStatusBadge({ isOnline, status }: Props) {
  const { t } = useTranslation();

  return (
    <span
      className={`inline-flex items-center gap-1.5 rounded-full px-2.5 py-0.5 text-xs font-medium ${
        isOnline
          ? 'bg-green-100 text-green-800'
          : 'bg-red-100 text-red-800'
      }`}
    >
      <span
        className={`h-2 w-2 rounded-full ${isOnline ? 'bg-green-500' : 'bg-red-500'}`}
      />
      {isOnline ? t('printer.online') : t('printer.offline')}
      {status && ` — ${status}`}
    </span>
  );
}
