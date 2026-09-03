import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { printersApi, profilesApi } from '../services/printersApi';
import type { Printer } from '../types/Printer';
import type { QueueProfile, TrayProfile } from '../types/Profile';

export function usePrinters(includeInactive = false) {
  return useQuery({
    queryKey: ['printers', includeInactive],
    queryFn: () => printersApi.getAll(includeInactive),
    refetchInterval: 30000,
  });
}

export function usePrinter(id: number) {
  return useQuery({
    queryKey: ['printer', id],
    queryFn: () => printersApi.getById(id),
    enabled: id > 0,
  });
}

export function useCreatePrinter() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (printer: Partial<Printer>) => printersApi.create(printer),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['printers'] });
    },
  });
}

export function useQueueProfiles(printerId?: number) {
  return useQuery({
    queryKey: ['queueProfiles', printerId],
    queryFn: () => profilesApi.getQueueProfiles(printerId),
  });
}

export function useCreateQueueProfile() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (profile: Partial<QueueProfile>) => profilesApi.createQueueProfile(profile),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['queueProfiles'] });
    },
  });
}

export function useTrayProfiles(printerId?: number) {
  return useQuery({
    queryKey: ['trayProfiles', printerId],
    queryFn: () => profilesApi.getTrayProfiles(printerId),
  });
}

export function useCreateTrayProfile() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (profile: Partial<TrayProfile>) => profilesApi.createTrayProfile(profile),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['trayProfiles'] });
    },
  });
}

export function useSuggestProfile() {
  return useMutation({
    mutationFn: ({ printerId, isDuplex, colorMode }: { printerId: number; isDuplex: boolean; colorMode: string }) =>
      profilesApi.suggest(printerId, isDuplex, colorMode),
  });
}
