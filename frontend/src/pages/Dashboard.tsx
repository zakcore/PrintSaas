import { useTranslation } from 'react-i18next';
import { useNavigate } from 'react-router-dom';
import { usePrinters } from '../hooks/usePrinters';
import { useJobs } from '../hooks/useJobs';
import { PrinterStatusBadge } from '../components/PrinterStatusBadge';
import { JobCard } from '../components/JobCard';

export function Dashboard() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const { data: printers, isLoading: printersLoading } = usePrinters();
  const { data: jobs, isLoading: jobsLoading } = useJobs();

  if (printersLoading || jobsLoading) {
    return <div className="p-8 text-center text-gray-500">{t('common.loading')}</div>;
  }

  const recentJobs = jobs?.slice(0, 10) ?? [];

  return (
    <div className="space-y-6">
      <h1 className="text-2xl font-bold text-gray-900">{t('nav.dashboard')}</h1>

      {/* Printer Fleet */}
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
        {printers?.map((printer) => (
          <div
            key={printer.id}
            className="rounded-lg border border-gray-200 bg-white p-4 shadow-sm"
          >
            <div className="flex items-center justify-between">
              <h3 className="font-medium text-gray-900">{printer.name}</h3>
              <PrinterStatusBadge isOnline={printer.isOnline} status={printer.status} />
            </div>
            <p className="mt-1 text-sm text-gray-500">{printer.model}</p>
            <p className="text-xs text-gray-400">{printer.ipAddress}:{printer.port}</p>
            <div className="mt-2 text-xs text-gray-400">
              {printer.queueProfiles.length} {t('nav.queueProfiles').toLowerCase()}
            </div>
          </div>
        ))}
      </div>

      {/* Recent Jobs */}
      <div>
        <h2 className="mb-3 text-lg font-semibold text-gray-900">{t('nav.jobs')}</h2>
        {recentJobs.length === 0 ? (
          <p className="text-gray-500">{t('common.noData')}</p>
        ) : (
          <div className="space-y-2">
            {recentJobs.map((job) => (
              <JobCard
                key={job.id}
                job={job}
                onClick={() => navigate(`/jobs/${job.id}`)}
              />
            ))}
          </div>
        )}
      </div>
    </div>
  );
}
