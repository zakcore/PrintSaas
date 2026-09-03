import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { useQueueProfiles, useCreateQueueProfile, usePrinters, useTrayProfiles } from '../../hooks/usePrinters';
import { printersApi, profilesApi } from '../../services/printersApi';
import type { QueueProfile } from '../../types/Profile';
import type { MachineQueueName } from '../../types/Printer';

export function QueueProfiles() {
  const { t } = useTranslation();
  const { data: profiles, isLoading, refetch } = useQueueProfiles();
  const { data: printers } = usePrinters();
  const { data: trayProfiles } = useTrayProfiles();
  const createProfile = useCreateQueueProfile();

  const [showForm, setShowForm] = useState(false);
  const [selectedPrinterId, setSelectedPrinterId] = useState<number | null>(null);
  const [machineQueues, setMachineQueues] = useState<MachineQueueName[]>([]);
  const [form, setForm] = useState<Partial<QueueProfile>>({
    displayName: '', machineQueueName: '', printerId: 0,
    isDuplex: false, colorMode: 'BW', priority: 1, holdOnArrival: false,
  });

  const handlePrinterChange = async (printerId: number) => {
    setSelectedPrinterId(printerId);
    setForm({ ...form, printerId });
    try {
      const queues = await printersApi.getMachineQueues(printerId);
      setMachineQueues(queues);
    } catch {
      setMachineQueues([]);
    }
  };

  const handleCreate = async () => {
    await createProfile.mutateAsync(form);
    setShowForm(false);
    setForm({ displayName: '', machineQueueName: '', printerId: 0, isDuplex: false, colorMode: 'BW', priority: 1 });
    refetch();
  };

  const handleDeactivate = async (id: number) => {
    await profilesApi.deactivateQueueProfile(id);
    refetch();
  };

  if (isLoading) return <div className="p-8 text-gray-500">{t('common.loading')}</div>;

  // Group profiles by printer
  const grouped = (profiles ?? []).reduce<Record<string, QueueProfile[]>>((acc, p) => {
    const printerName = printers?.find((pr) => pr.id === p.printerId)?.name ?? `Printer ${p.printerId}`;
    (acc[printerName] ??= []).push(p);
    return acc;
  }, {});

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold text-gray-900">{t('nav.queueProfiles')}</h1>
        <button onClick={() => setShowForm(!showForm)}
          className="rounded-md bg-blue-600 px-4 py-2 text-sm text-white hover:bg-blue-700">
          + Add Profile
        </button>
      </div>

      {showForm && (
        <div className="rounded-lg border border-gray-200 bg-white p-4 space-y-3">
          <div className="grid grid-cols-2 gap-3">
            <input placeholder="Display Name" value={form.displayName}
              onChange={(e) => setForm({ ...form, displayName: e.target.value })}
              className="rounded-md border border-gray-300 px-3 py-2 text-sm" />

            <select value={selectedPrinterId ?? ''}
              onChange={(e) => handlePrinterChange(Number(e.target.value))}
              className="rounded-md border border-gray-300 px-3 py-2 text-sm">
              <option value="">Select Printer</option>
              {printers?.map((p) => (
                <option key={p.id} value={p.id}>{p.name}</option>
              ))}
            </select>

            {machineQueues.length > 0 ? (
              <select value={form.machineQueueName}
                onChange={(e) => setForm({ ...form, machineQueueName: e.target.value })}
                className="rounded-md border border-gray-300 px-3 py-2 text-sm">
                <option value="">Select Machine Queue</option>
                {machineQueues.map((q) => (
                  <option key={q.id} value={q.queueName}>{q.queueName}</option>
                ))}
              </select>
            ) : (
              <div>
                <input placeholder="Machine Queue Name (manual)" value={form.machineQueueName}
                  onChange={(e) => setForm({ ...form, machineQueueName: e.target.value })}
                  className="rounded-md border border-yellow-300 bg-yellow-50 px-3 py-2 text-sm w-full" />
                {selectedPrinterId && (
                  <p className="text-xs text-yellow-600 mt-1">
                    No discovered queues. Sync first or enter manually.
                  </p>
                )}
              </div>
            )}

            <select value={form.colorMode}
              onChange={(e) => setForm({ ...form, colorMode: e.target.value })}
              className="rounded-md border border-gray-300 px-3 py-2 text-sm">
              <option value="BW">B&W</option>
              <option value="Color">Color</option>
            </select>

            <label className="flex items-center gap-2 text-sm">
              <input type="checkbox" checked={form.isDuplex}
                onChange={(e) => setForm({ ...form, isDuplex: e.target.checked })} />
              Duplex
            </label>

            <input placeholder="Priority" type="number" value={form.priority}
              onChange={(e) => setForm({ ...form, priority: Number(e.target.value) })}
              className="rounded-md border border-gray-300 px-3 py-2 text-sm" />

            <select value={form.trayProfileId ?? ''}
              onChange={(e) => setForm({ ...form, trayProfileId: e.target.value ? Number(e.target.value) : undefined })}
              className="rounded-md border border-gray-300 px-3 py-2 text-sm">
              <option value="">No Tray Profile</option>
              {trayProfiles?.filter((tp) => !selectedPrinterId || tp.printerId === selectedPrinterId)
                .map((tp) => (
                  <option key={tp.id} value={tp.id}>Tray {tp.trayNumber} — {tp.paperType} {tp.paperSize}</option>
                ))}
            </select>
          </div>

          <div className="flex gap-2">
            <button onClick={handleCreate} disabled={createProfile.isPending}
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

      {Object.entries(grouped).map(([printerName, profiles]) => (
        <div key={printerName} className="space-y-2">
          <h2 className="text-lg font-semibold text-gray-700">{printerName}</h2>
          <div className="overflow-x-auto rounded-lg border border-gray-200">
            <table className="w-full text-left text-sm">
              <thead className="bg-gray-50 text-xs uppercase text-gray-500">
                <tr>
                  <th className="px-4 py-3">Display Name</th>
                  <th className="px-4 py-3">Machine Queue</th>
                  <th className="px-4 py-3">Duplex</th>
                  <th className="px-4 py-3">Color</th>
                  <th className="px-4 py-3">Priority</th>
                  <th className="px-4 py-3">Active</th>
                  <th className="px-4 py-3">Actions</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-200 bg-white">
                {profiles.map((p) => (
                  <tr key={p.id} className={!p.isActive ? 'opacity-50' : ''}>
                    <td className="px-4 py-3 font-medium">{p.displayName}</td>
                    <td className="px-4 py-3 font-mono text-xs">{p.machineQueueName}</td>
                    <td className="px-4 py-3">{p.isDuplex ? 'Yes' : 'No'}</td>
                    <td className="px-4 py-3">{p.colorMode}</td>
                    <td className="px-4 py-3">{p.priority}</td>
                    <td className="px-4 py-3">{p.isActive ? 'Yes' : 'No'}</td>
                    <td className="px-4 py-3">
                      {p.isActive && (
                        <button onClick={() => handleDeactivate(p.id)}
                          className="rounded bg-red-100 px-2 py-1 text-xs text-red-700 hover:bg-red-200">
                          {t('common.delete')}
                        </button>
                      )}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      ))}
    </div>
  );
}
