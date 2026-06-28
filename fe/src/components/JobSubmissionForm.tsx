import { useState, type FormEvent } from 'react';
import type { ComputeType, CreateJobFormValues } from '../types';

const computeOptions: Array<{ label: string; value: ComputeType; helper: string }> = [
  { label: 'CPU Small', value: 'CpuSmall', helper: '1 credit/min equivalent' },
  { label: 'CPU Large', value: 'CpuLarge', helper: '3 credits/min equivalent' },
  { label: 'GPU', value: 'Gpu', helper: '8 credits/min equivalent' },
];

const initialValues: CreateJobFormValues = {
  jobName: '',
  projectId: '',
  computeType: 'CpuSmall',
  file: null,
};

interface Props {
  onSubmit: (values: CreateJobFormValues) => Promise<void>;
  isSubmitting: boolean;
}

export default function JobSubmissionForm({ onSubmit, isSubmitting }: Props) {
  const [values, setValues] = useState<CreateJobFormValues>(initialValues);

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    await onSubmit(values);
    setValues(initialValues);
    event.currentTarget.reset();
  }

  return (
    <section className="glass-panel-strong rounded-3xl p-6 sm:p-8 fade-in">
      <div className="mb-6 flex items-start justify-between gap-4">
        <div>
          <p className="text-xs uppercase tracking-[0.35em] text-sky-300/80">Create job</p>
          <h2 className="mt-2 text-2xl font-semibold text-white">Submit a cloud execution request</h2>
          <p className="mt-2 max-w-2xl text-sm text-slate-300">
            This form posts multipart data to the backend create endpoint and uploads the input file.
          </p>
        </div>
        <div className="rounded-2xl border border-sky-400/20 bg-sky-400/10 px-4 py-3 text-right text-xs text-sky-100">
          <div className="font-semibold uppercase tracking-[0.2em]">Backend</div>
          <div className="mt-1">POST /api/jobs</div>
        </div>
      </div>

      <form className="grid gap-4 sm:grid-cols-2" onSubmit={handleSubmit}>
        <label className="space-y-2 sm:col-span-1">
          <span className="text-sm font-medium text-slate-200">Job name</span>
          <input
            className="input-shell rounded-2xl px-4 py-3"
            placeholder="Render nightly dataset"
            value={values.jobName}
            onChange={(event) => setValues((current) => ({ ...current, jobName: event.target.value }))}
            required
          />
        </label>

        <label className="space-y-2 sm:col-span-1">
          <span className="text-sm font-medium text-slate-200">Project ID</span>
          <input
            className="input-shell rounded-2xl px-4 py-3"
            placeholder="PRJ-001"
            value={values.projectId}
            onChange={(event) => setValues((current) => ({ ...current, projectId: event.target.value }))}
            required
          />
        </label>

        <label className="space-y-2 sm:col-span-1">
          <span className="text-sm font-medium text-slate-200">Compute type</span>
          <select
            className="input-shell rounded-2xl px-4 py-3"
            value={values.computeType}
            onChange={(event) => setValues((current) => ({ ...current, computeType: event.target.value as ComputeType }))}
          >
            {computeOptions.map((option) => (
              <option key={option.value} value={option.value}>
                {option.label}
              </option>
            ))}
          </select>
        </label>

        <label className="space-y-2 sm:col-span-1">
          <span className="text-sm font-medium text-slate-200">Input file</span>
          <input
            className="input-shell rounded-2xl px-4 py-3 file:mr-4 file:rounded-xl file:border-0 file:bg-sky-400 file:px-4 file:py-2 file:text-sm file:font-semibold file:text-slate-950"
            type="file"
            onChange={(event) => setValues((current) => ({ ...current, file: event.target.files?.[0] ?? null }))}
            required
          />
        </label>

        <div className="sm:col-span-2 flex flex-wrap items-center justify-between gap-4 pt-2">
          <p className="text-sm text-slate-300">
            Input uploads to S3 on submit. The backend starts the EC2 trigger and returns the created job record.
          </p>
          <button
            className="inline-flex items-center justify-center rounded-2xl bg-sky-400 px-5 py-3 text-sm font-semibold text-slate-950 transition hover:bg-sky-300 disabled:cursor-not-allowed disabled:opacity-60"
            type="submit"
            disabled={isSubmitting}
          >
            {isSubmitting ? 'Submitting...' : 'Submit job'}
          </button>
        </div>
      </form>

      <div className="mt-6 grid gap-3 sm:grid-cols-3">
        {computeOptions.map((option) => (
          <div key={option.value} className="rounded-2xl border border-white/10 bg-white/5 px-4 py-3">
            <div className="text-sm font-semibold text-white">{option.label}</div>
            <div className="mt-1 text-xs text-slate-300">{option.helper}</div>
          </div>
        ))}
      </div>
    </section>
  );
}