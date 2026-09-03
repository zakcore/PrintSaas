import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { useTrayProfiles, useCreateTrayProfile, usePrinters } from '../../hooks/usePrinters';
import { profilesApi } from '../../services/printersApi';
import type { TrayProfile } from '../../types/Profile';

export function TrayProfiles() {
  const { t } = useTranslation();
  const { data: profiles, isLoading, refetch } = useTrayProfiles();
  const { data: printers } = usePrinters();
  const createProfile = useCreateTrayProfile();

  const [showForm, setShowForm] = useState(false);
  const [form, setForm] = useState<Partial<TrayProfile>>({
    printerId: 0, trayNumber: 1, paperType: 'Plain', paperSize: 'Letter', paperWeight: '75gsm',
  });

  const handleCreate = async () => {
    await createProfile.mutateAsync(form);
    setShowForm(false);
    setForm({ printerId: 0, trayNumber: 1, paperType: 'Plain', paperSize: 'Letter', paperWeight: '75gsm' });
    refetch();
  };

  const handleDeactivate = async (id: number) => {
    await profilesApi.deactivateTrayProfile(id);
    refetch();
  };

  if (isLoading) return <div className="p-8 text-gray-500">{t('common.loading')}</div>;

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold text-gray-900">{t('nav.trayProfiles')}</h1>
        <button onClick={() => setShowForm(!showForm)}
          className="rounded-md bg-blue-600 px-4 py-2 text-sm text-white hover:bg-blue-700">
          + Add Tray Profile
        </button>
      </div>

      {showForm && (
        <div className="rounded-lg border border-gray-200 bg-white p-4 space-y-3">
          <div className="grid grid-cols-3 gap-3">
            <select value={form.printerId}
              onChange={(e) => setForm({ ...form, printerId: Number(e.target.value) })}
              className="rounded-md border border-gray-300 px-3 py-2 text-sm">
              <option value={0}>Select Printer</option>
              {printers?.map((p) => (
                <option key={p.id} value={p.id}>{p.name}</option>
              ))}
            </select>
            <input placeholder="Tray Number" type="number" value={form.trayNumber}
              onChange={(e) => setForm({ ...form, trayNumber: Number(e.target.value) })}
              className="rounded-md border border-gray-300 px-3 py-2 text-sm" />
            <select value={form.paperType}
              onChange={(e) => setForm({ ...form, paperType: e.target.value })}
              className="rounded-md border border-gray-300 px-3 py-2 text-sm">
              <option value="Plain">Plain</option>
              <option value="Bond">Bond</option>
              <option value="MICR">MICR</option>
            </select>
            <select value={form.paperSize}
              onChange={(e) => setForm({ ...form, paperSize: e.target.value })}
              className="rounded-md border border-gray-300 px-3 py-2 text-sm">
              <option value="Letter">Letter</option>
              <option value="Legal">Legal</option>
            </select>
            <input placeholder="Paper Weight" value={form.paperWeight}
              onChange={(e) => setForm({ ...form, paperWeight: e.target.value })}
              className="rounded-md border border-gray-300 px-3 py-2 text-sm" />
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

      <div className="overflow-x-auto rounded-lg border border-gray-200">
        <table className="w-full text-left text-sm">
          <thead className="bg-gray-50 text-xs uppercase text-gray-500">
            <tr>
              <th className="px-4 py-3">{t('job.printer')}</th>
              <th className="px-4 py-3">Tray #</th>
              <th className="px-4 py-3">Paper Type</th>
              <th className="px-4 py-3">Size</th>
              <th className="px-4 py-3">Weight</th>
              <th className="px-4 py-3">Active</th>
              <th className="px-4 py-3">Actions</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-200 bg-white">
            {profiles?.map((p) => (
              <tr key={p.id} className={!p.isActive ? 'opacity-50' : ''}>
                <td className="px-4 py-3">{printers?.find((pr) => pr.id === p.printerId)?.name}</td>
                <td className="px-4 py-3">{p.trayNumber}</td>
                <td className="px-4 py-3">{p.paperType}</td>
                <td className="px-4 py-3">{p.paperSize}</td>
                <td className="px-4 py-3">{p.paperWeight}</td>
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
  );
}
