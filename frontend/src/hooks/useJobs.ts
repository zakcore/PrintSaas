import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { jobsApi } from '../services/jobsApi';

export function useJobs(status?: string, clientName?: string) {
  return useQuery({
    queryKey: ['jobs', status, clientName],
    queryFn: () => jobsApi.getAll(status, clientName),
    refetchInterval: 10000,
  });
}

export function useJob(id: number) {
  return useQuery({
    queryKey: ['job', id],
    queryFn: () => jobsApi.getById(id),
    enabled: id > 0,
  });
}

export function useSendJob() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: number) => jobsApi.send(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['jobs'] });
    },
  });
}

export function useResendJob() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: number) => jobsApi.resend(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['jobs'] });
    },
  });
}

export function useConfirmJob() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, complianceNote }: { id: number; complianceNote?: string }) =>
      jobsApi.confirm(id, complianceNote),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['jobs'] });
    },
  });
}

export function useJobHistory(id: number) {
  return useQuery({
    queryKey: ['jobHistory', id],
    queryFn: () => jobsApi.getHistory(id),
    enabled: id > 0,
  });
}
