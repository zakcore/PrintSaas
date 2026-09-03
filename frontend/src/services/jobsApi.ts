import api from './api';
import type { Job, JobHistory } from '../types/Job';

export const jobsApi = {
  getAll: (status?: string, clientName?: string) =>
    api.get<Job[]>('/jobs', { params: { status, clientName } }).then((r) => r.data),

  getById: (id: number) =>
    api.get<Job>(`/jobs/${id}`).then((r) => r.data),

  send: (id: number) =>
    api.post(`/jobs/${id}/send`).then((r) => r.data),

  resend: (id: number) =>
    api.post(`/jobs/${id}/resend`).then((r) => r.data),

  confirm: (id: number, complianceNote?: string) =>
    api.post(`/jobs/${id}/confirm`, { complianceNote }).then((r) => r.data),

  getHistory: (id: number) =>
    api.get<JobHistory[]>(`/jobs/${id}/history`).then((r) => r.data),

  upload: (formData: FormData) =>
    api.post<Job>('/jobs/upload', formData, {
      headers: { 'Content-Type': 'multipart/form-data' },
    }).then((r) => r.data),
};
