import { useEffect, useMemo, useState } from 'react';
import BillingSummaryCard from './components/BillingSummaryCard';
import JobDetailPanel from './components/JobDetailPanel';
import JobList from './components/JobList';
import JobSubmissionForm from './components/JobSubmissionForm';
import { completeJob, createJob, fetchBillingSummary, fetchJob } from './lib/api';
import type { BillingSummaryResponse, CompleteJobFormValues, CreateJobFormValues, JobModel } from './types';

const defaultPageSize = 10;

export default function App() {
  const [summary, setSummary] = useState<BillingSummaryResponse>();
  const [selectedJob, setSelectedJob] = useState<JobModel>();
  const [selectedJobId, setSelectedJobId] = useState<string>();
  const [page, setPage] = useState(1);
  const [jobMessage, setJobMessage] = useState('');
  const [errorMessage, setErrorMessage] = useState('');
  const [isCreating, setIsCreating] = useState(false);
  const [isCompleting, setIsCompleting] = useState(false);
  const [isLoadingSummary, setIsLoadingSummary] = useState(false);
  const [isLoadingJob, setIsLoadingJob] = useState(false);

  const jobs = useMemo(() => summary?.billedJobs ?? [], [summary]);

  async function loadSummary(nextPage = page) {
    setIsLoadingSummary(true);
    setErrorMessage('');

    try {
      const nextSummary = await fetchBillingSummary(nextPage, defaultPageSize);
      setSummary(nextSummary);
      setPage(nextSummary.pagination.page);

      if (!selectedJobId && nextSummary.billedJobs.length > 0) {
        setSelectedJobId(nextSummary.billedJobs[0].jobId);
      }
    } catch (error) {
      setErrorMessage(getErrorMessage(error));
    } finally {
      setIsLoadingSummary(false);
    }
  }

  async function loadJob(jobId: string) {
    setIsLoadingJob(true);
    setErrorMessage('');

    try {
      const job = await fetchJob(jobId);
      setSelectedJob(job);
      setSelectedJobId(job.jobId);
    } catch (error) {
      setErrorMessage(getErrorMessage(error));
    } finally {
      setIsLoadingJob(false);
    }
  }

  async function handleCreateJob(values: CreateJobFormValues) {
    setIsCreating(true);
    setErrorMessage('');
    setJobMessage('');

    try {
      const job = await createJob(values);
      setJobMessage(`Job created: ${job.jobId}`);
      await Promise.all([loadSummary(1), loadJob(job.jobId)]);
    } catch (error) {
      setErrorMessage(getErrorMessage(error));
    } finally {
      setIsCreating(false);
    }
  }

  async function handleCompleteJob(values: CompleteJobFormValues) {
    if (!selectedJobId) {
      setErrorMessage('Select a job before completing it.');
      return;
    }

    setIsCompleting(true);
    setErrorMessage('');
    setJobMessage('');

    try {
      const job = await completeJob(selectedJobId, values);
      setJobMessage(`Job completed: ${job.jobId}`);
      await Promise.all([loadSummary(page), loadJob(selectedJobId)]);
    } catch (error) {
      setErrorMessage(getErrorMessage(error));
    } finally {
      setIsCompleting(false);
    }
  }

  useEffect(() => {
    void loadSummary(1);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  useEffect(() => {
    if (selectedJobId) {
      void loadJob(selectedJobId);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [selectedJobId]);

  const selectedJobFromList = selectedJob ?? jobs.find((job) => job.jobId === selectedJobId);

  return (
    <div className="min-h-screen text-slate-100 soft-grid">
      <main className="mx-auto flex w-full max-w-7xl flex-col gap-6 px-4 py-6 sm:px-6 lg:px-8">
        <header className="glass-panel rounded-[2rem] p-6 sm:p-8 fade-in">
          <div className="flex flex-col gap-4 lg:flex-row lg:items-end lg:justify-between">
            <div className="max-w-3xl">
              <p className="text-xs uppercase tracking-[0.45em] text-cyan-300/80">Simplified Cloud Job</p>
              <h1 className="mt-3 text-4xl font-semibold tracking-tight text-white sm:text-5xl">
                React dashboard for job submission, execution, and billing.
              </h1>
              <p className="mt-4 text-sm leading-7 text-slate-300 sm:text-base">
                This frontend talks to the ASP.NET Core backend through axios, uploads the job input file,
                reads billing summary data, and can manually finalize a job using the completion webhook shape.
              </p>
            </div>

            <div className="grid gap-3 sm:grid-cols-2">
              <InfoCard label="API base" value={import.meta.env.VITE_API_BASE_URL || 'relative /api proxy'} />
              <InfoCard label="List source" value="GET /api/jobs/billing-summary" />
              <InfoCard label="Detail source" value="GET /api/jobs/{jobId}" />
              <InfoCard label="Complete source" value="POST /api/jobs/{id}/complete" />
            </div>
          </div>
        </header>

        {(errorMessage || jobMessage) && (
          <section className="grid gap-3 md:grid-cols-2">
            {errorMessage && <Alert tone="danger" title="Error" message={errorMessage} />}
            {jobMessage && <Alert tone="success" title="Success" message={jobMessage} />}
          </section>
        )}

        <section className="grid gap-6 xl:grid-cols-[1.1fr_0.9fr]">
          <JobSubmissionForm onSubmit={handleCreateJob} isSubmitting={isCreating} />
          <BillingSummaryCard
            summary={summary}
            onPrevPage={() => void loadSummary(page - 1)}
            onNextPage={() => void loadSummary(page + 1)}
            onRefresh={() => loadSummary(page)}
          />
        </section>

        <section className="grid gap-6 xl:grid-cols-[1.1fr_0.9fr]">
          <JobList
            jobs={jobs}
            selectedJobId={selectedJobId}
            onSelect={(jobId) => setSelectedJobId(jobId)}
          />
          <JobDetailPanel
            job={selectedJobFromList}
            onRefresh={async () => {
              if (selectedJobId) {
                await loadJob(selectedJobId);
              }
            }}
            onComplete={handleCompleteJob}
            isCompleting={isCompleting || isLoadingJob || isLoadingSummary}
          />
        </section>

        <footer className="pb-6 text-center text-xs text-slate-400">
          Built with React 19, Tailwind CSS 4, Vite, and axios.
        </footer>
      </main>
    </div>
  );
}

function InfoCard({ label, value }: { label: string; value: string }) {
  return (
    <div className="rounded-2xl border border-white/10 bg-white/5 px-4 py-3">
      <div className="text-[11px] uppercase tracking-[0.25em] text-slate-400">{label}</div>
      <div className="mt-1 text-sm font-medium text-white">{value}</div>
    </div>
  );
}

function Alert({ tone, title, message }: { tone: 'success' | 'danger'; title: string; message: string }) {
  const styles = tone === 'success' ? 'border-emerald-400/25 bg-emerald-400/10 text-emerald-100' : 'border-rose-400/25 bg-rose-400/10 text-rose-100';

  return (
    <div className={`glass-panel rounded-2xl border px-4 py-3 ${styles}`}>
      <div className="text-xs uppercase tracking-[0.25em] opacity-80">{title}</div>
      <div className="mt-1 text-sm">{message}</div>
    </div>
  );
}

function getErrorMessage(error: unknown) {
  if (error instanceof Error) {
    return error.message;
  }

  return 'Unexpected error occurred.';
}