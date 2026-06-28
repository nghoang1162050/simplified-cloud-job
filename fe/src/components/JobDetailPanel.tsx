import { type FormEvent } from 'react';
import type { CompleteJobFormValues, JobModel } from '../types';

interface Props {
  job?: JobModel;
  onRefresh: () => Promise<void>;
  onComplete: (values: CompleteJobFormValues) => Promise<void>;
  isCompleting: boolean;
}

function statusChip(status: JobModel['status']) {
  switch (status) {
    case 'Completed':
      return 'border-emerald-400/30 bg-emerald-400/15 text-emerald-200';
    case 'Failed':
    case 'Cancelled':
      return 'border-rose-400/30 bg-rose-400/15 text-rose-200';
    default:
      return 'border-amber-400/30 bg-amber-400/15 text-amber-200';
  }
}

export default function JobDetailPanel({ job, onRefresh, onComplete, isCompleting }: Props) {
  async function handleComplete(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const formData = new FormData(event.currentTarget);
    const executionDuration = Number(formData.get('executionDuration') ?? 0);
    const outputFile = formData.get('outputFile');

    if (!(outputFile instanceof File)) {
      throw new Error('Pick an output file before completing the job.');
    }

    await onComplete({ executionDuration, outputFile });
    event.currentTarget.reset();
  }

  return (
    <section className="glass-panel rounded-3xl p-6 fade-in">
      <div className="flex items-start justify-between gap-4">
        <div>
          <p className="text-xs uppercase tracking-[0.35em] text-sky-300/80">Job detail</p>
          <h2 className="mt-2 text-2xl font-semibold text-white">Status, output, and billing</h2>
        </div>
        <button
          className="rounded-2xl border border-white/10 bg-white/5 px-4 py-2 text-sm text-slate-200 transition hover:bg-white/10"
          type="button"
          onClick={() => void onRefresh()}
          disabled={!job}
        >
          Refresh
        </button>
      </div>

      {!job ? (
        <div className="mt-6 rounded-2xl border border-dashed border-white/15 bg-white/5 p-8 text-sm text-slate-300">
          Select a job from the table to load its detail view.
        </div>
      ) : (
        <div className="mt-6 space-y-6">
          <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
            <Metric label="Status" value={job.status} tone={statusChip(job.status)} />
            <Metric label="Duration" value={`${job.executionDuration}s`} />
            <Metric label="Credit cost" value={job.creditCost.toFixed(2)} />
            <Metric label="Compute type" value={job.computeType} />
          </div>

          <div className="grid gap-4 lg:grid-cols-2">
            <div className="rounded-2xl border border-white/10 bg-white/5 p-4">
              <div className="text-xs uppercase tracking-[0.25em] text-slate-400">References</div>
              <dl className="mt-4 space-y-4 text-sm">
                <Row label="Input file" value={job.inputFileName} mono />
                <Row label="Output file" value={job.outputFileReference || 'Pending'} mono />
                <Row label="Job ID" value={job.jobId} mono />
                <Row label="Project ID" value={job.projectId} mono={false} />
              </dl>
            </div>

            <form className="rounded-2xl border border-white/10 bg-white/5 p-4" onSubmit={handleComplete}>
              <div className="text-xs uppercase tracking-[0.25em] text-slate-400">Manual completion</div>
              <p className="mt-2 text-sm text-slate-300">
                This simulates the EC2 callback endpoint by posting duration and an output artifact.
              </p>

              <div className="mt-4 grid gap-4">
                <label className="space-y-2">
                  <span className="text-sm font-medium text-slate-200">Execution duration in seconds</span>
                  <input
                    name="executionDuration"
                    type="number"
                    min={1}
                    defaultValue={60}
                    className="input-shell rounded-2xl px-4 py-3"
                    required
                  />
                </label>

                <label className="space-y-2">
                  <span className="text-sm font-medium text-slate-200">Output file</span>
                  <input
                    name="outputFile"
                    type="file"
                    className="input-shell rounded-2xl px-4 py-3 file:mr-4 file:rounded-xl file:border-0 file:bg-cyan-400 file:px-4 file:py-2 file:text-sm file:font-semibold file:text-slate-950"
                    required
                  />
                </label>
              </div>

              <div className="mt-5 flex items-center justify-between gap-4">
                <span className="text-xs text-slate-400">
                  Backend endpoint: POST /api/jobs/{job.jobId}/complete
                </span>
                <button
                  className="rounded-2xl bg-cyan-400 px-5 py-3 text-sm font-semibold text-slate-950 transition hover:bg-cyan-300 disabled:cursor-not-allowed disabled:opacity-60"
                  type="submit"
                  disabled={job.status === 'Completed' || isCompleting}
                >
                  {job.status === 'Completed' ? 'Already completed' : isCompleting ? 'Completing...' : 'Complete job'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </section>
  );
}

function Metric({ label, value, tone }: { label: string; value: string; tone?: string }) {
  return (
    <div className="rounded-2xl border border-white/10 bg-slate-950/35 p-4">
      <div className="text-xs uppercase tracking-[0.25em] text-slate-400">{label}</div>
      <div className={`mt-2 text-lg font-semibold text-white ${tone ?? ''}`}>{value}</div>
    </div>
  );
}

function Row({ label, value, mono = true }: { label: string; value: string; mono?: boolean }) {
  return (
    <div className="grid gap-1 sm:grid-cols-[140px_1fr] sm:items-start">
      <dt className="text-slate-400">{label}</dt>
      <dd className={mono ? 'break-all font-mono text-slate-100' : 'text-slate-100'}>{value}</dd>
    </div>
  );
}