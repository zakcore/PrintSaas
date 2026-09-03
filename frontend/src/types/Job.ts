export type JobStatus =
  | 'Received'
  | 'Pending'
  | 'Processing'
  | 'Printing'
  | 'Done'
  | 'Error'
  | 'Retained'
  | 'AwaitingOperatorConfirmation'
  | 'BlockedSecurityViolation';

export type JobType =
  | 'BankStatement'
  | 'SalaryCheque'
  | 'InsuranceDocument'
  | 'Bill'
  | 'TaxDocument'
  | 'ColorDocument';

export interface JobParameters {
  id: number;
  jobId: number;
  isDuplex: boolean;
  colorMode: string;
  copies: number;
  totalPageCount: number;
  isPayrollJob: boolean;
  employeeCount: number | null;
  pagesPerEmployee: number | null;
  containsCheques: boolean;
  containsStubs: boolean;
  paperType: string;
}

export interface JobHistory {
  id: number;
  jobId: number;
  action: string;
  operatorId: number | null;
  timestamp: string;
  details: string | null;
}

export interface Job {
  id: number;
  jobName: string;
  clientName: string;
  fileLocation: string;
  fileSize: number;
  status: JobStatus;
  jobType: JobType;
  printerId: number | null;
  queueProfileId: number | null;
  operatorId: number | null;
  printerJobId: string | null;
  createdAt: string;
  sentAt: string | null;
  completedAt: string | null;
  printer: import('./Printer').Printer | null;
  queueProfile: import('./Profile').QueueProfile | null;
  parameters: JobParameters | null;
  history: JobHistory[];
}
