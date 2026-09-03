import api from './api';
import type { Printer, MachineQueueName } from '../types/Printer';
import type { QueueProfile, TrayProfile } from '../types/Profile';

export const printersApi = {
  getAll: (includeInactive = false) =>
    api.get<Printer[]>('/printers', { params: { includeInactive } }).then((r) => r.data),

  getById: (id: number) =>
    api.get<Printer>(`/printers/${id}`).then((r) => r.data),

  create: (printer: Partial<Printer>) =>
    api.post<Printer>('/printers', printer).then((r) => r.data),

  update: (id: number, printer: Partial<Printer>) =>
    api.put(`/printers/${id}`, printer).then((r) => r.data),

  deactivate: (id: number) =>
    api.delete(`/printers/${id}`).then((r) => r.data),

  getStatus: (id: number) =>
    api.get(`/printers/${id}/status`).then((r) => r.data),

  testConnection: (id: number) =>
    api.post(`/printers/${id}/test`).then((r) => r.data),

  syncQueues: (id: number) =>
    api.post(`/printers/${id}/sync-queues`).then((r) => r.data),

  getMachineQueues: (id: number) =>
    api.get<MachineQueueName[]>(`/printers/${id}/machine-queues`).then((r) => r.data),
};

export const profilesApi = {
  getQueueProfiles: (printerId?: number) =>
    api.get<QueueProfile[]>('/profiles/queue', { params: { printerId } }).then((r) => r.data),

  getQueueProfile: (id: number) =>
    api.get<QueueProfile>(`/profiles/queue/${id}`).then((r) => r.data),

  createQueueProfile: (profile: Partial<QueueProfile>) =>
    api.post<QueueProfile>('/profiles/queue', profile).then((r) => r.data),

  updateQueueProfile: (id: number, profile: Partial<QueueProfile>) =>
    api.put(`/profiles/queue/${id}`, profile).then((r) => r.data),

  deactivateQueueProfile: (id: number) =>
    api.delete(`/profiles/queue/${id}`).then((r) => r.data),

  suggest: (printerId: number, isDuplex: boolean, colorMode: string) =>
    api.post<QueueProfile>('/profiles/suggest', { printerId, isDuplex, colorMode }).then((r) => r.data),

  getTrayProfiles: (printerId?: number) =>
    api.get<TrayProfile[]>('/profiles/tray', { params: { printerId } }).then((r) => r.data),

  getTrayProfile: (id: number) =>
    api.get<TrayProfile>(`/profiles/tray/${id}`).then((r) => r.data),

  createTrayProfile: (profile: Partial<TrayProfile>) =>
    api.post<TrayProfile>('/profiles/tray', profile).then((r) => r.data),

  updateTrayProfile: (id: number, profile: Partial<TrayProfile>) =>
    api.put(`/profiles/tray/${id}`, profile).then((r) => r.data),

  deactivateTrayProfile: (id: number) =>
    api.delete(`/profiles/tray/${id}`).then((r) => r.data),
};
