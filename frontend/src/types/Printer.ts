import type { QueueProfile, TrayProfile } from './Profile';

export interface MachineQueueName {
  id: number;
  printerId: number;
  queueName: string;
  description: string | null;
  discoveredAt: string;
}

export interface Printer {
  id: number;
  name: string;
  model: string;
  ipAddress: string;
  port: number;
  protocol: string;
  defaultQueueName: string | null;
  colorCapability: string;
  isActive: boolean;
  isOnline: boolean;
  status: string | null;
  lastSeenOnline: string | null;
  queueProfiles: QueueProfile[];
  trayProfiles: TrayProfile[];
  machineQueueNames: MachineQueueName[];
}
