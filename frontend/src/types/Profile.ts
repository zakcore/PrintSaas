export interface QueueProfile {
  id: number;
  displayName: string;
  machineQueueName: string;
  printerId: number;
  isDuplex: boolean;
  colorMode: string;
  priority: number;
  holdOnArrival: boolean;
  isActive: boolean;
  trayProfileId: number | null;
}

export interface TrayProfile {
  id: number;
  printerId: number;
  trayNumber: number;
  paperType: string;
  paperSize: string;
  paperWeight: string;
  isActive: boolean;
}
