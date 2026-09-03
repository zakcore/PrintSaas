import { useEffect, useRef } from 'react';
import { HubConnectionBuilder, HubConnection, LogLevel } from '@microsoft/signalr';
import { useQueryClient } from '@tanstack/react-query';

export function usePrintStatus() {
  const connectionRef = useRef<HubConnection | null>(null);
  const queryClient = useQueryClient();

  useEffect(() => {
    const token = localStorage.getItem('token');
    if (!token) return;

    const connection = new HubConnectionBuilder()
      .withUrl('/hubs/print-status', {
        accessTokenFactory: () => token,
      })
      .withAutomaticReconnect()
      .configureLogging(LogLevel.Warning)
      .build();

    connection.on('OnJobStatusChanged', () => {
      queryClient.invalidateQueries({ queryKey: ['jobs'] });
    });

    connection.on('OnPrinterStatusChanged', () => {
      queryClient.invalidateQueries({ queryKey: ['printers'] });
    });

    connection.on('OnPrinterAlert', () => {
      queryClient.invalidateQueries({ queryKey: ['printers'] });
    });

    connection.on('OnJobConfirmationNeeded', () => {
      queryClient.invalidateQueries({ queryKey: ['jobs'] });
    });

    connection.start().catch(console.error);
    connectionRef.current = connection;

    return () => {
      connection.stop();
    };
  }, [queryClient]);

  return connectionRef;
}
