import { useState, useRef } from 'react';
import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { jobsApi } from '../services/jobsApi';
import { usePrinters, useQueueProfiles } from '../hooks/usePrinters';

const jobTypes = [
  'BankStatement', 'SalaryCheque', 'InsuranceDocument', 'Bill', 'TaxDocument', 'ColorDocument',
];

export function JobUpload() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const fileRef = useRef<HTMLInputElement>(null);

  const { data: printers } = usePrinters();
  const [selectedPrinterId, setSelectedPrinterId] = useState<number | undefined>();
  const { data: queueProfiles } = useQueueProfiles(selectedPrinterId);

  const [form, setForm] = useState({
    jobName: '',
    clientName: '',
    jobType: 'BankStatement',
    isDuplex: true,
    colorMode: 'BW',
    copies: 1,
    isPayrollJob: false,
    employeeCount: '',
    pagesPerEmployee: '',
    containsCheques: false,
    containsStubs: false,
    paperType: 'Plain',
    printerId: '',
    queueProfileId: '',
  });

  const [file, setFile] = useState<File | null>(null);
  const [dragOver, setDragOver] = useState(false);
  const [error, setError] = useState('');

  const upload = useMutation({
    mutationFn: (formData: FormData) => jobsApi.upload(formData),
    onSuccess: (job) => {
      queryClient.invalidateQueries({ queryKey: ['jobs'] });
      navigate(`/jobs/${job.id}`);
    },
    onError: () => setError('Upload failed'),
  });

  const handleFileChange = (files: FileList | null) => {
    if (!files?.length) return;
    const f = files[0];
    if (!f.name.toLowerCase().endsWith('.pdf')) {
      setError('Only PDF files are accepted / Seuls les fichiers PDF sont acceptés');
      return;
    }
    setError('');
    setFile(f);
    if (!form.jobName) {
      setForm((prev) => ({ ...prev, jobName: f.name.replace('.pdf', '') }));
    }
  };

  const handleDrop = (e: React.DragEvent) => {
    e.preventDefault();
    setDragOver(false);
    handleFileChange(e.dataTransfer.files);
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!file) { setError('Please select a PDF file'); return; }

    const formData = new FormData();
    formData.append('file', file);
    formData.append('jobName', form.jobName);
    formData.append('clientName', form.clientName);
    formData.append('jobType', form.jobType);
    formData.append('isDuplex', String(form.isDuplex));
    formData.append('colorMode', form.colorMode);
    formData.append('copies', String(form.copies));
    formData.append('isPayrollJob', String(form.isPayrollJob));
    formData.append('employeeCount', form.employeeCount || '0');
    formData.append('pagesPerEmployee', form.pagesPerEmployee || '0');
    formData.append('containsCheques', String(form.containsCheques));
    formData.append('containsStubs', String(form.containsStubs));
    formData.append('paperType', form.paperType);
    if (form.printerId) formData.append('printerId', form.printerId);
    if (form.queueProfileId) formData.append('queueProfileId', form.queueProfileId);

    upload.mutate(formData);
  };

  const handlePrinterChange = (printerId: string) => {
    setForm((prev) => ({ ...prev, printerId, queueProfileId: '' }));
    setSelectedPrinterId(printerId ? Number(printerId) : undefined);
  };

  return (
    <div className="mx-auto max-w-3xl space-y-6">
      <h1 className="text-2xl font-bold text-gray-900">
        {t('job.send')} — Upload PDF
      </h1>

      <form onSubmit={handleSubmit} className="space-y-6">
        {/* File Drop Zone */}
        <div
          onDragOver={(e) => { e.preventDefault(); setDragOver(true); }}
          onDragLeave={() => setDragOver(false)}
          onDrop={handleDrop}
          onClick={() => fileRef.current?.click()}
          className={`cursor-pointer rounded-lg border-2 border-dashed p-8 text-center transition ${
            dragOver ? 'border-blue-500 bg-blue-50' : 'border-gray-300 hover:border-gray-400'
          }`}
        >
          <input
            ref={fileRef}
            type="file"
            accept=".pdf"
            className="hidden"
            onChange={(e) => handleFileChange(e.target.files)}
          />
          {file ? (
            <div>
              <p className="text-lg font-medium text-gray-900">{file.name}</p>
              <p className="text-sm text-gray-500">
                {(file.size / 1024 / 1024).toFixed(2)} MB
              </p>
              <p className="mt-1 text-xs text-green-600">
                Click or drag to replace / Cliquer ou glisser pour remplacer
              </p>
            </div>
          ) : (
            <div>
              <p className="text-gray-500">
                Drag & drop a PDF file here, or click to browse
              </p>
              <p className="text-sm text-gray-400">
                Glissez-déposez un fichier PDF ici, ou cliquez pour parcourir
              </p>
            </div>
          )}
        </div>

        {/* Job Info */}
        <div className="rounded-lg border border-gray-200 bg-white p-6 space-y-4">
          <h2 className="font-semibold text-gray-900">Job Information / Informations du travail</h2>

          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium text-gray-700">{t('job.name')}</label>
              <input value={form.jobName}
                onChange={(e) => setForm({ ...form, jobName: e.target.value })}
                className="mt-1 w-full rounded-md border border-gray-300 px-3 py-2 text-sm"
                required />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700">{t('job.client')}</label>
              <input value={form.clientName}
                onChange={(e) => setForm({ ...form, clientName: e.target.value })}
                className="mt-1 w-full rounded-md border border-gray-300 px-3 py-2 text-sm"
                required />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700">{t('job.type')}</label>
              <select value={form.jobType}
                onChange={(e) => setForm({ ...form, jobType: e.target.value })}
                className="mt-1 w-full rounded-md border border-gray-300 px-3 py-2 text-sm">
                {jobTypes.map((jt) => <option key={jt} value={jt}>{jt}</option>)}
              </select>
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700">Paper / Papier</label>
              <select value={form.paperType}
                onChange={(e) => setForm({ ...form, paperType: e.target.value })}
                className="mt-1 w-full rounded-md border border-gray-300 px-3 py-2 text-sm">
                <option value="Plain">Plain</option>
                <option value="Bond">Bond</option>
                <option value="MICR">MICR</option>
              </select>
            </div>
          </div>

          {/* Print Settings */}
          <div className="grid grid-cols-3 gap-4">
            <div>
              <label className="block text-sm font-medium text-gray-700">Color / Couleur</label>
              <select value={form.colorMode}
                onChange={(e) => setForm({ ...form, colorMode: e.target.value })}
                className="mt-1 w-full rounded-md border border-gray-300 px-3 py-2 text-sm">
                <option value="BW">B&W / N&B</option>
                <option value="Color">Color / Couleur</option>
              </select>
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700">Copies</label>
              <input type="number" min={1} value={form.copies}
                onChange={(e) => setForm({ ...form, copies: Number(e.target.value) })}
                className="mt-1 w-full rounded-md border border-gray-300 px-3 py-2 text-sm" />
            </div>
            <div className="flex items-end">
              <label className="flex items-center gap-2 text-sm">
                <input type="checkbox" checked={form.isDuplex}
                  onChange={(e) => setForm({ ...form, isDuplex: e.target.checked })} />
                Duplex / Recto-verso
              </label>
            </div>
          </div>

          {/* Payroll Section */}
          <div className="border-t border-gray-200 pt-4">
            <label className="flex items-center gap-2 text-sm font-medium text-gray-700">
              <input type="checkbox" checked={form.isPayrollJob}
                onChange={(e) => setForm({ ...form, isPayrollJob: e.target.checked })} />
              Payroll Job / Travail de paie
            </label>

            {form.isPayrollJob && (
              <div className="mt-3 grid grid-cols-2 gap-4">
                <div>
                  <label className="block text-sm text-gray-600">Employee Count / Nombre d'employés</label>
                  <input type="number" value={form.employeeCount}
                    onChange={(e) => setForm({ ...form, employeeCount: e.target.value })}
                    className="mt-1 w-full rounded-md border border-gray-300 px-3 py-2 text-sm" />
                </div>
                <div>
                  <label className="block text-sm text-gray-600">Pages per Employee / Pages par employé</label>
                  <input type="number" value={form.pagesPerEmployee}
                    onChange={(e) => setForm({ ...form, pagesPerEmployee: e.target.value })}
                    className="mt-1 w-full rounded-md border border-gray-300 px-3 py-2 text-sm" />
                </div>
                <label className="flex items-center gap-2 text-sm">
                  <input type="checkbox" checked={form.containsCheques}
                    onChange={(e) => setForm({ ...form, containsCheques: e.target.checked })} />
                  Contains cheques / Contient des chèques
                </label>
                <label className="flex items-center gap-2 text-sm">
                  <input type="checkbox" checked={form.containsStubs}
                    onChange={(e) => setForm({ ...form, containsStubs: e.target.checked })} />
                  Contains stubs / Contient des talons
                </label>
              </div>
            )}
          </div>
        </div>

        {/* Printer & Profile Selection */}
        <div className="rounded-lg border border-gray-200 bg-white p-6 space-y-4">
          <h2 className="font-semibold text-gray-900">
            {t('job.printer')} & {t('job.profile')}
          </h2>
          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium text-gray-700">{t('job.printer')}</label>
              <select value={form.printerId}
                onChange={(e) => handlePrinterChange(e.target.value)}
                className="mt-1 w-full rounded-md border border-gray-300 px-3 py-2 text-sm">
                <option value="">— Select / Sélectionner —</option>
                {printers?.map((p) => (
                  <option key={p.id} value={p.id}>{p.name} ({p.colorCapability})</option>
                ))}
              </select>
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700">{t('job.profile')}</label>
              <select value={form.queueProfileId}
                onChange={(e) => setForm({ ...form, queueProfileId: e.target.value })}
                className="mt-1 w-full rounded-md border border-gray-300 px-3 py-2 text-sm">
                <option value="">— Auto-suggest / Auto —</option>
                {queueProfiles?.map((p) => (
                  <option key={p.id} value={p.id}>
                    {p.displayName} ({p.machineQueueName})
                  </option>
                ))}
              </select>
            </div>
          </div>
        </div>

        {/* Error & Submit */}
        {error && <p className="text-sm text-red-600">{error}</p>}

        <div className="flex gap-3">
          <button type="submit" disabled={upload.isPending || !file}
            className="rounded-md bg-blue-600 px-6 py-2 text-white hover:bg-blue-700 disabled:opacity-50">
            {upload.isPending ? t('common.loading') : 'Upload & Create Job / Téléverser'}
          </button>
          <button type="button" onClick={() => navigate('/jobs')}
            className="rounded-md border border-gray-300 px-6 py-2 hover:bg-gray-50">
            {t('common.cancel')}
          </button>
        </div>
      </form>
    </div>
  );
}
