import { useParams } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { useJob, useSendJob, useResendJob, useConfirmJob, useJobHistory } from '../hooks/useJobs';
import { useAppStore } from '../store/useAppStore';

export function JobDetail() {
  const { t } = useTranslation();
  const { id } = useParams<{ id: string }>();
  const jobId = Number(id);
  const role = useAppStore((s) => s.auth.role);

  const { data: job, isLoading } = useJob(jobId);
  const { data: history } = useJobHistory(jobId);
  const sendJob = useSendJob();
  const resendJob = useResendJob();
  const confirmJob = useConfirmJob();

  if (isLoading || !job) {
    return <div className="p-8 text-gray-500">{t('common.loading')}</div>;
  }

  const canOperate = role === 'Admin' || role === 'Operator';

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold text-gray-900">{job.jobName}</h1>
        <span className="rounded-full bg-blue-100 px-3 py-1 text-sm font-medium text-blue-800">
          {t(`status.${job.status}`)}
        </span>
      </div>

      {/* Job Info */}
      <div className="grid grid-cols-2 gap-4 rounded-lg border border-gray-200 bg-white p-6">
        <div>
          <p className="text-sm text-gray-500">{t('job.client')}</p>
          <p className="font-medium">{job.clientName}</p>
        </div>
        <div>
          <p className="text-sm text-gray-500">{t('job.type')}</p>
          <p className="font-medium">{job.jobType}</p>
        </div>
        <div>
          <p className="text-sm text-gray-500">{t('job.printer')}</p>
          <p className="font-medium">{job.printer?.name ?? '—'}</p>
        </div>
        <div>
          <p className="text-sm text-gray-500">{t('job.profile')}</p>
          <p className="font-medium">{job.queueProfile?.displayName ?? '—'}</p>
        </div>
        <div>
          <p className="text-sm text-gray-500">{t('job.pages')}</p>
          <p className="font-medium">{job.parameters?.totalPageCount ?? '—'}</p>
        </div>
        <div>
          <p className="text-sm text-gray-500">{t('job.created')}</p>
          <p className="font-medium">{new Date(job.createdAt).toLocaleString()}</p>
        </div>
      </div>

      {/* Parameters */}
      {job.parameters && (
        <div className="rounded-lg border border-gray-200 bg-white p-6">
          <h2 className="mb-3 font-semibold">Parameters</h2>
          <div className="grid grid-cols-3 gap-3 text-sm">
            <div>Duplex: <strong>{job.parameters.isDuplex ? 'Yes' : 'No'}</strong></div>
            <div>Color: <strong>{job.parameters.colorMode}</strong></div>
            <div>Copies: <strong>{job.parameters.copies}</strong></div>
            <div>Paper: <strong>{job.parameters.paperType}</strong></div>
            {job.parameters.isPayrollJob && (
              <>
                <div>Employees: <strong>{job.parameters.employeeCount ?? '—'}</strong></div>
                <div>Pages/Employee: <strong>{job.parameters.pagesPerEmployee ?? '—'}</strong></div>
              </>
            )}
          </div>
        </div>
      )}

      {/* Actions */}
      {canOperate && (
        <div className="flex gap-3">
          {(job.status === 'Received' || job.status === 'Pending') && (
            <button
              onClick={() => sendJob.mutate(job.id)}
              disabled={sendJob.isPending}
              className="rounded-md bg-blue-600 px-4 py-2 text-white hover:bg-blue-700 disabled:opacity-50"
            >
              {t('job.send')}
            </button>
          )}
          {job.status === 'Retained' && (
            <button
              onClick={() => resendJob.mutate(job.id)}
              disabled={resendJob.isPending}
              className="rounded-md bg-yellow-600 px-4 py-2 text-white hover:bg-yellow-700 disabled:opacity-50"
            >
              {t('job.resend')}
            </button>
          )}
          {job.status === 'AwaitingOperatorConfirmation' && (
            <button
              onClick={() => confirmJob.mutate({ id: job.id })}
              disabled={confirmJob.isPending}
              className="rounded-md bg-green-600 px-4 py-2 text-white hover:bg-green-700 disabled:opacity-50"
            >
              {t('job.confirm')}
            </button>
          )}
        </div>
      )}

      {/* History */}
      {history && history.length > 0 && (
        <div className="rounded-lg border border-gray-200 bg-white p-6">
          <h2 className="mb-3 font-semibold">History</h2>
          <div className="space-y-2">
            {history.map((h) => (
              <div key={h.id} className="flex justify-between text-sm">
                <span>{h.action}</span>
                <span className="text-gray-400">{new Date(h.timestamp).toLocaleString()}</span>
              </div>
            ))}
          </div>
        </div>
      )}
    </div>
  );
}
