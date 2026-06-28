import axios from 'axios';
import type { ApiResponse, BillingSummaryResponse, CompleteJobFormValues, CreateJobFormValues, JobModel } from '../types';

const api = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL || '',
  headers: {
    Accept: 'application/json',
  },
});

function unwrapResponse<T>(response: { data: ApiResponse<T> }): T {
  if (!response.data.success || !response.data.data) {
    throw new Error(response.data.message || 'Request failed');
  }

  return response.data.data;
}

export async function createJob(values: CreateJobFormValues): Promise<JobModel> {
  if (!values.file) {
    throw new Error('Select an input file before submitting the job.');
  }

  const formData = new FormData();
  formData.append('JobName', values.jobName);
  formData.append('ProjectId', values.projectId);
  formData.append('ComputeType', values.computeType);
  formData.append('File', values.file);

  const response = await api.post<ApiResponse<JobModel>>('/api/jobs', formData, {
    headers: { 'Content-Type': 'multipart/form-data' },
  });

  return unwrapResponse(response);
}

export async function fetchJob(jobId: string): Promise<JobModel> {
  const response = await api.get<ApiResponse<JobModel>>(`/api/jobs/${jobId}`);
  return unwrapResponse(response);
}

export async function completeJob(jobId: string, values: CompleteJobFormValues): Promise<JobModel> {
  if (!values.outputFile) {
    throw new Error('Select an output file before completing the job.');
  }

  const formData = new FormData();
  formData.append('ExecutionDuration', String(values.executionDuration));
  formData.append('OutputFile', values.outputFile);

  const response = await api.post<ApiResponse<JobModel>>(`/api/jobs/${jobId}/complete`, formData, {
    headers: { 'Content-Type': 'multipart/form-data' },
  });

  return unwrapResponse(response);
}

export async function fetchBillingSummary(page: number, pageSize: number): Promise<BillingSummaryResponse> {
  const response = await api.get<ApiResponse<BillingSummaryResponse>>('/api/jobs/billing-summary', {
    params: { page, pageSize },
  });

  return unwrapResponse(response);
}

export { api };