import { BrowserRouter, Routes, Route, Navigate, Link, Outlet } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { useTranslation } from 'react-i18next';
import { useAppStore } from './store/useAppStore';
import { usePrintStatus } from './hooks/usePrintStatus';
import { Login } from './pages/Login';
import { Dashboard } from './pages/Dashboard';
import { JobQueue } from './pages/JobQueue';
import { JobDetail } from './pages/JobDetail';
import { Printers } from './pages/Printers';
import { PrintersAdmin } from './pages/Admin/PrintersAdmin';
import { QueueProfiles } from './pages/Admin/QueueProfiles';
import { TrayProfiles } from './pages/Admin/TrayProfiles';
import { Users } from './pages/Admin/Users';
import { JobUpload } from './pages/JobUpload';

const queryClient = new QueryClient({
  defaultOptions: {
    queries: { staleTime: 5000, retry: 1 },
  },
});

function ProtectedLayout() {
  const { t, i18n } = useTranslation();
  const { auth, logout, language, setLanguage } = useAppStore();

  usePrintStatus();

  if (!auth.token) {
    return <Navigate to="/login" replace />;
  }

  const toggleLang = () => {
    const newLang = language === 'fr' ? 'en' : 'fr';
    setLanguage(newLang);
    i18n.changeLanguage(newLang);
  };

  const isAdmin = auth.role === 'Admin';

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Navbar */}
      <nav className="border-b border-gray-200 bg-white">
        <div className="mx-auto flex max-w-7xl items-center justify-between px-4 py-3">
          <div className="flex items-center gap-6">
            <span className="text-lg font-bold text-blue-600">PrintSaaS</span>
            <Link to="/" className="text-sm text-gray-600 hover:text-gray-900">{t('nav.dashboard')}</Link>
            <Link to="/jobs" className="text-sm text-gray-600 hover:text-gray-900">{t('nav.jobs')}</Link>
            <Link to="/printers" className="text-sm text-gray-600 hover:text-gray-900">{t('nav.printers')}</Link>
            {isAdmin && (
              <div className="relative group">
                <span className="cursor-pointer text-sm text-gray-600 hover:text-gray-900">{t('nav.admin')}</span>
                <div className="absolute left-0 top-full z-50 hidden min-w-48 rounded-md border border-gray-200 bg-white py-1 shadow-lg group-hover:block">
                  <Link to="/admin/printers" className="block px-4 py-2 text-sm text-gray-700 hover:bg-gray-100">
                    {t('nav.printers')}
                  </Link>
                  <Link to="/admin/queue-profiles" className="block px-4 py-2 text-sm text-gray-700 hover:bg-gray-100">
                    {t('nav.queueProfiles')}
                  </Link>
                  <Link to="/admin/tray-profiles" className="block px-4 py-2 text-sm text-gray-700 hover:bg-gray-100">
                    {t('nav.trayProfiles')}
                  </Link>
                  <Link to="/admin/users" className="block px-4 py-2 text-sm text-gray-700 hover:bg-gray-100">
                    {t('nav.users')}
                  </Link>
                </div>
              </div>
            )}
          </div>
          <div className="flex items-center gap-4">
            <button
              onClick={toggleLang}
              className="rounded border border-gray-300 px-2 py-1 text-xs"
            >
              {language === 'fr' ? 'EN' : 'FR'}
            </button>
            <span className="text-sm text-gray-500">{auth.displayName}</span>
            <button
              onClick={() => { logout(); window.location.href = '/login'; }}
              className="text-sm text-red-600 hover:text-red-800"
            >
              {t('nav.logout')}
            </button>
          </div>
        </div>
      </nav>

      {/* Content */}
      <main className="mx-auto max-w-7xl px-4 py-6">
        <Outlet />
      </main>
    </div>
  );
}

export default function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <BrowserRouter>
        <Routes>
          <Route path="/login" element={<Login />} />
          <Route element={<ProtectedLayout />}>
            <Route path="/" element={<Dashboard />} />
            <Route path="/jobs" element={<JobQueue />} />
            <Route path="/jobs/upload" element={<JobUpload />} />
            <Route path="/jobs/:id" element={<JobDetail />} />
            <Route path="/printers" element={<Printers />} />
            <Route path="/admin/printers" element={<PrintersAdmin />} />
            <Route path="/admin/queue-profiles" element={<QueueProfiles />} />
            <Route path="/admin/tray-profiles" element={<TrayProfiles />} />
            <Route path="/admin/users" element={<Users />} />
          </Route>
          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </BrowserRouter>
    </QueryClientProvider>
  );
}
