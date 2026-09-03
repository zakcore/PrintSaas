import { useTranslation } from 'react-i18next';
import type { Job } from '../types/Job';

const statusColors: Record<string, string> = {
  Received: 'bg-blue-100 text-blue-800',
  Pending: 'bg-yellow-100 text-yellow-800',
  Processing: 'bg-indigo-100 text-indigo-800',
  Printing: 'bg-purple-100 text-purple-800',
  Done: 'bg-green-100 text-green-800',
  Error: 'bg-red-100 text-red-800',
  Retained: 'bg-gray-100 text-gray-800',
  AwaitingOperatorConfirmation: 'bg-orange-100 text-orange-800',
  BlockedSecurityViolation: 'bg-red-200 text-red-900',
};

interface Props {
  job: Job;
  onClick?: () => void;
}

export function JobCard({ job, onClick }: Props) {
  const { t } = useTranslation();

  return (
    <div
      onClick={onClick}
      className="cursor-pointer rounded-lg border border-gray-200 bg-white p-4 shadow-sm transition hover:shadow-md"
    >
      <div className="flex items-center justify-between">
        <h3 className="font-medium text-gray-900">{job.jobName}</h3>
        <span
          className={`rounded-full px-2.5 py-0.5 text-xs font-medium ${statusColors[job.status] ?? 'bg-gray-100'}`}
        >
          {t(`status.${job.status}`)}
        </span>
      </div>
      <div className="mt-2 text-sm text-gray-500">
        <p>{job.clientName}</p>
        <div className="mt-1 flex gap-4">
          <span>{job.parameters?.totalPageCount ?? '—'} {t('job.pages').toLowerCase()}</span>
          {job.printer && <span>{job.printer.name}</span>}
        </div>
      </div>
    </div>
  );
}
