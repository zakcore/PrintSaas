import { create } from 'zustand';

interface AuthState {
  token: string | null;
  username: string | null;
  displayName: string | null;
  role: string | null;
}

interface AppState {
  auth: AuthState;
  language: 'fr' | 'en';
  selectedPrinterId: number | null;
  login: (token: string, username: string, displayName: string, role: string) => void;
  logout: () => void;
  setLanguage: (lang: 'fr' | 'en') => void;
  setSelectedPrinter: (id: number | null) => void;
}

export const useAppStore = create<AppState>((set) => ({
  auth: {
    token: localStorage.getItem('token'),
    username: localStorage.getItem('username'),
    displayName: localStorage.getItem('displayName'),
    role: localStorage.getItem('role'),
  },
  language: (localStorage.getItem('language') as 'fr' | 'en') ?? 'fr',
  selectedPrinterId: null,

  login: (token, username, displayName, role) => {
    localStorage.setItem('token', token);
    localStorage.setItem('username', username);
    localStorage.setItem('displayName', displayName);
    localStorage.setItem('role', role);
    set({ auth: { token, username, displayName, role } });
  },

  logout: () => {
    localStorage.removeItem('token');
    localStorage.removeItem('username');
    localStorage.removeItem('displayName');
    localStorage.removeItem('role');
    set({ auth: { token: null, username: null, displayName: null, role: null } });
  },

  setLanguage: (lang) => {
    localStorage.setItem('language', lang);
    set({ language: lang });
  },

  setSelectedPrinter: (id) => set({ selectedPrinterId: id }),
}));
