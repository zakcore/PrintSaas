/import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { usePrinters, useCreatePrinter } from '../../hooks/usePrinters';
import { printersApi } from '../../services/printersApi';
import { PrinterStatusBadge } from '../../components/PrinterStatusBadge';
import type { Printer } from '../../types/Printer';

export function PrintersAdmin() {
  const { t } = useTranslation();
  const { data: printers, isLoading, refetch } = usePrinters(true);
  const createPrinter = useCreatePrinter();
  const [showForm, setShowForm] = useState(false);
  const [form, setForm] = useState<Partial<Printer>>({
    name: '', model: '', ipAddress: '', port: 443, protocol: 'IPPS', colorCapability: 'BW',
  });
  const [testResult, setTestResult] = useState<Record<number, string>>({});
  const [syncResult, setSyncResult] = useState<Record<number, string>>({});

  const handleCreate = async () => {
    await createPrinter.mutateAsync(form);
    setShowForm(false);
    setForm({ name: '', model: '', ipAddress: '', port: 443, protocol: 'IPPS', colorCapability: 'BW' });
  };

  const handleTest = async (id: number) => {
    try {
      const result = await printersApi.testConnection(id);
      setTestResult((prev) => ({ ...prev, [id]: result.message }));
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : 'Connection failed';
      setTestResult((prev) => ({ ...prev, [id]: msg }));
    }
  };

  const handleSync = async (id: number) => {
    try {
      const result = await printersApi.syncQueues(id);
      setSyncResult((prev) => ({ ...prev, [id]: result.message }));
      refetch();
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : 'Sync failed';
      setSyncResult((prev) => ({ ...prev, [id]: msg }));
    }
  };

  const handleDeactivate = async (id: number) => {
    await printersApi.deactivate(id);
    refetch();
  };

  if (isLoading) return <div className="p-8 text-gray-500">{t('common.loading')}</div>;

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold text-gray-900">{t('nav.printers')} — Admin</h1>
        <button
          onClick={() => setShowForm(!showForm)}
          className="rounded-md bg-blue-600 px-4 py-2 text-sm text-white hover:bg-blue-700"
        >
          {t('printer.add')}
        </button>
      </div>

      {showForm && (
        <div className="rounded-lg border border-gray-200 bg-white p-4 space-y-3">
          <div className="grid grid-cols-3 gap-3">
            <input placeholder={t('printer.name')} value={form.name}
              onChange={(e) => setForm({ ...form, name: e.target.value })}
              className="rounded-md border border-gray-300 px-3 py-2 text-sm" />
            <input placeholder={t('printer.model')} value={form.model}
              onChange={(e) => setForm({ ...form, model: e.target.value })}
              className="rounded-md border border-gray-300 px-3 py-2 text-sm" />
            <input placeholder={t('printer.ip')} value={form.ipAddress}
              onChange={(e) => setForm({ ...form, ipAddress: e.target.value })}
              className="rounded-md border border-gray-300 px-3 py-2 text-sm" />
            <input placeholder={t('printer.port')} type="number" value={form.port}
              onChange={(e) => setForm({ ...form, port: Number(e.target.value) })}
              className="rounded-md border border-gray-300 px-3 py-2 text-sm" />
            <select value={form.protocol}
              onChange={(e) => setForm({ ...form, protocol: e.target.value })}
              className="rounded-md border border-gray-300 px-3 py-2 text-sm">
              <option value="IPPS">IPPS</option>
              <option value="IPP">IPP</option>
            </select>
            <select value={form.colorCapability}
              onChange={(e) => setForm({ ...form, colorCapability: e.target.value })}
              className="rounded-md border border-gray-300 px-3 py-2 text-sm">
              <option value="BW">B&W</option>
              <option value="Color">Color</option>
              <option value="Both">Both</option>
            </select>
          </div>
          <div className="flex gap-2">
            <button onClick={handleCreate} disabled={createPrinter.isPending}
              className="rounded-md bg-green-600 px-4 py-2 text-sm text-white hover:bg-green-700 disabled:opacity-50">
              {t('common.save')}
            </button>
            <button onClick={() => setShowForm(false)}
              className="rounded-md border border-gray-300 px-4 py-2 text-sm hover:bg-gray-50">
              {t('common.cancel')}
            </button>
          </div>
        </div>
      )}

      <div className="overflow-x-auto rounded-lg border border-gray-200">
        <table className="w-full text-left text-sm">
          <thead className="bg-gray-50 text-xs uppercase text-gray-500">
            <tr>
              <th className="px-4 py-3">{t('printer.name')}</th>
              <th className="px-4 py-3">{t('printer.model')}</th>
              <th className="px-4 py-3">{t('printer.ip')}</th>
              <th className="px-4 py-3">{t('printer.status')}</th>
              <th className="px-4 py-3">Color</th>
              <th className="px-4 py-3">Actions</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-200 bg-white">
            {printers?.map((printer) => (
              <tr key={printer.id} className={!printer.isActive ? 'opacity-50' : ''}>
                <td className="px-4 py-3 font-medium">{printer.name}</td>
                <td className="px-4 py-3">{printer.model}</td>
                <td className="px-4 py-3">{printer.ipAddress}:{printer.port}</td>
                <td className="px-4 py-3">
                  <PrinterStatusBadge isOnline={printer.isOnline} status={printer.status} />
                </td>
                <td className="px-4 py-3">{printer.colorCapability}</td>
                <td className="px-4 py-3">
                  <div className="flex flex-col gap-1">
                    <div className="flex gap-2">
                      <button onClick={() => handleTest(printer.id)}
                        className="rounded bg-blue-100 px-2 py-1 text-xs text-blue-700 hover:bg-blue-200">
                        {t('printer.test')}
                      </button>
                      <button onClick={() => handleSync(printer.id)}
                        className="rounded bg-purple-100 px-2 py-1 text-xs text-purple-700 hover:bg-purple-200">
                        {t('printer.sync')}
                      </button>
                      {printer.isActive && (
                        <button onClick={() => handleDeactivate(printer.id)}
                          className="rounded bg-red-100 px-2 py-1 text-xs text-red-700 hover:bg-red-200">
                          {t('common.delete')}
                        </button>
                      )}
                    </div>
                    {testResult[printer.id] && (
                      <span className="text-xs text-gray-500">{testResult[printer.id]}</span>
                    )}
                    {syncResult[printer.id] && (
                      <span className="text-xs text-gray-500">{syncResult[printer.id]}</span>
                    )}
                  </div>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
}
