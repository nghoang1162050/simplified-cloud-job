export type ComputeType = 'CpuSmall' | 'CpuLarge' | 'Gpu';

export type JobStatus = 'Submitted' | 'Queued' | 'Running' | 'Completed' | 'Failed' | 'Cancelled';

export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data?: T;
  errors?: unknown;
}

export interface JobModel {
  jobId: string;
  jobName: string;
  projectId: string;
  computeType: ComputeType;
  inputFileName: string;
  status: JobStatus;
  outputFileReference?: string | null;
  executionDuration: number;
  creditCost: number;
  createdAt: string;
}

export interface BillingSummaryResponse {
  totalCreditsUsed: number;
  totalCompletedJobs: number;
  billedJobs: JobModel[];
  pagination: {
    page: number;
    pageSize: number;
    totalCount: number;
    totalPages: number;
    hasNextPage: boolean;
    hasPreviousPage: boolean;
  };
}

export interface CreateJobFormValues {
  jobName: string;
  projectId: string;
  computeType: ComputeType;
  file: File | null;
}

export interface CompleteJobFormValues {
  executionDuration: number;
  outputFile: File | null;
}