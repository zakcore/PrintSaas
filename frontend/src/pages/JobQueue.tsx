import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { useJobs } from '../hooks/useJobs';
import type { JobStatus } from '../types/Job';

const statuses: (JobStatus | '')[] = [
  '', 'Received', 'Pending', 'Processing', 'Printing', 'Done', 'Error', 'Retained', 'AwaitingOperatorConfirmation',
];

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

export function JobQueue() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const [statusFilter, setStatusFilter] = useState('');
  const [clientFilter, setClientFilter] = useState('');
  const { data: jobs, isLoading } = useJobs(statusFilter || undefined, clientFilter || undefined);

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold text-gray-900">{t('nav.jobs')}</h1>
        <button
          onClick={() => navigate('/jobs/upload')}
          className="rounded-md bg-blue-600 px-4 py-2 text-sm text-white hover:bg-blue-700"
        >
          + Upload PDF
        </button>
      </div>

      {/* Filters */}
      <div className="flex gap-4">
        <select
          value={statusFilter}
          onChange={(e) => setStatusFilter(e.target.value)}
          className="rounded-md border border-gray-300 px-3 py-2 text-sm"
        >
          <option value="">{t('job.status')} — All</option>
          {statuses.filter(Boolean).map((s) => (
            <option key={s} value={s}>{t(`status.${s}`)}</option>
          ))}
        </select>
        <input
          type="text"
          placeholder={t('job.client')}
          value={clientFilter}
          onChange={(e) => setClientFilter(e.target.value)}
          className="rounded-md border border-gray-300 px-3 py-2 text-sm"
        />
      </div>

      {isLoading ? (
        <p className="text-gray-500">{t('common.loading')}</p>
      ) : !jobs?.length ? (
        <p className="text-gray-500">{t('common.noData')}</p>
      ) : (
        <div className="overflow-x-auto rounded-lg border border-gray-200">
          <table className="w-full text-left text-sm">
            <thead className="bg-gray-50 text-xs uppercase text-gray-500">
              <tr>
                <th className="px-4 py-3">{t('job.name')}</th>
                <th className="px-4 py-3">{t('job.client')}</th>
                <th className="px-4 py-3">{t('job.pages')}</th>
                <th className="px-4 py-3">{t('job.status')}</th>
                <th className="px-4 py-3">{t('job.printer')}</th>
                <th className="px-4 py-3">{t('job.created')}</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-200 bg-white">
              {jobs.map((job) => (
                <tr
                  key={job.id}
                  onClick={() => navigate(`/jobs/${job.id}`)}
                  className="cursor-pointer hover:bg-gray-50"
                >
                  <td className="px-4 py-3 font-medium text-gray-900">{job.jobName}</td>
                  <td className="px-4 py-3">{job.clientName}</td>
                  <td className="px-4 py-3">{job.parameters?.totalPageCount ?? '—'}</td>
                  <td className="px-4 py-3">
                    <span className={`rounded-full px-2 py-0.5 text-xs font-medium ${statusColors[job.status] ?? ''}`}>
                      {t(`status.${job.status}`)}
                    </span>
                  </td>
                  <td className="px-4 py-3">{job.printer?.name ?? '—'}</td>
                  <td className="px-4 py-3 text-gray-500">
                    {new Date(job.createdAt).toLocaleDateString()}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}
