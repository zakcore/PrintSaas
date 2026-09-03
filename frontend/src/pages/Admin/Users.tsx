import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import api from '../../services/api';

interface UserView {
  id: number;
  username: string;
  displayName: string;
  role: string;
  isActive: boolean;
  createdAt: string;
}

export function Users() {
  const { t } = useTranslation();
  const queryClient = useQueryClient();

  const { data: users, isLoading } = useQuery<UserView[]>({
    queryKey: ['users'],
    queryFn: () => api.get('/admin/users?includeInactive=true').then((r) => r.data),
  });

  const createUser = useMutation({
    mutationFn: (data: { username: string; password: string; displayName: string; role: string }) =>
      api.post('/admin/users', data),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['users'] }),
  });

  const deactivateUser = useMutation({
    mutationFn: (id: number) => api.delete(`/admin/users/${id}`),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['users'] }),
  });

  const [showForm, setShowForm] = useState(false);
  const [form, setForm] = useState({ username: '', password: '', displayName: '', role: 'Operator' });

  const handleCreate = async () => {
    await createUser.mutateAsync(form);
    setShowForm(false);
    setForm({ username: '', password: '', displayName: '', role: 'Operator' });
  };

  if (isLoading) return <div className="p-8 text-gray-500">{t('common.loading')}</div>;

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold text-gray-900">{t('nav.users')}</h1>
        <button onClick={() => setShowForm(!showForm)}
          className="rounded-md bg-blue-600 px-4 py-2 text-sm text-white hover:bg-blue-700">
          + Add User
        </button>
      </div>

      {showForm && (
        <div className="rounded-lg border border-gray-200 bg-white p-4 space-y-3">
          <div className="grid grid-cols-2 gap-3">
            <input placeholder={t('login.username')} value={form.username}
              onChange={(e) => setForm({ ...form, username: e.target.value })}
              className="rounded-md border border-gray-300 px-3 py-2 text-sm" />
            <input placeholder={t('login.password')} type="password" value={form.password}
              onChange={(e) => setForm({ ...form, password: e.target.value })}
              className="rounded-md border border-gray-300 px-3 py-2 text-sm" />
            <input placeholder="Display Name" value={form.displayName}
              onChange={(e) => setForm({ ...form, displayName: e.target.value })}
              className="rounded-md border border-gray-300 px-3 py-2 text-sm" />
            <select value={form.role}
              onChange={(e) => setForm({ ...form, role: e.target.value })}
              className="rounded-md border border-gray-300 px-3 py-2 text-sm">
              <option value="Admin">Admin</option>
              <option value="Operator">Operator</option>
              <option value="Viewer">Viewer</option>
            </select>
          </div>
          <div className="flex gap-2">
            <button onClick={handleCreate} disabled={createUser.isPending}
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
              <th className="px-4 py-3">{t('login.username')}</th>
              <th className="px-4 py-3">Display Name</th>
              <th className="px-4 py-3">Role</th>
              <th className="px-4 py-3">Active</th>
              <th className="px-4 py-3">Created</th>
              <th className="px-4 py-3">Actions</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-200 bg-white">
            {users?.map((u) => (
              <tr key={u.id} className={!u.isActive ? 'opacity-50' : ''}>
                <td className="px-4 py-3 font-medium">{u.username}</td>
                <td className="px-4 py-3">{u.displayName}</td>
                <td className="px-4 py-3">
                  <span className={`rounded-full px-2 py-0.5 text-xs font-medium ${
                    u.role === 'Admin' ? 'bg-red-100 text-red-800' :
                    u.role === 'Operator' ? 'bg-blue-100 text-blue-800' :
                    'bg-gray-100 text-gray-800'
                  }`}>
                    {u.role}
                  </span>
                </td>
                <td className="px-4 py-3">{u.isActive ? 'Yes' : 'No'}</td>
                <td className="px-4 py-3 text-gray-500">{new Date(u.createdAt).toLocaleDateString()}</td>
                <td className="px-4 py-3">
                  {u.isActive && u.id !== 1 && (
                    <button onClick={() => deactivateUser.mutate(u.id)}
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
