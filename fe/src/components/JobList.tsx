import type { JobModel } from '../types';

interface Props {
  jobs: JobModel[];
  selectedJobId?: string;
  onSelect: (jobId: string) => void;
}

function statusTone(status: JobModel['status']) {
  if (status === 'Completed') return 'bg-emerald-400/15 text-emerald-200 border-emerald-400/30';
  if (status === 'Running' || status === 'Queued' || status === 'Submitted') {
    return 'bg-amber-400/15 text-amber-200 border-amber-400/30';
  }
  if (status === 'Failed' || status === 'Cancelled') return 'bg-rose-400/15 text-rose-200 border-rose-400/30';
  return 'bg-white/10 text-slate-200 border-white/15';
}

export default function JobList({ jobs, selectedJobId, onSelect }: Props) {
  return (
    <section className="glass-panel rounded-3xl p-6 fade-in">
      <div className="mb-5 flex items-end justify-between gap-4">
        <div>
          <p className="text-xs uppercase tracking-[0.35em] text-cyan-300/80">Jobs</p>
          <h2 className="mt-2 text-2xl font-semibold text-white">Execution queue and billing list</h2>
        </div>
        <div className="text-sm text-slate-300">GET /api/jobs/billing-summary</div>
      </div>

      <div className="overflow-hidden rounded-2xl border border-white/10">
        <table className="min-w-full divide-y divide-white/10 text-left text-sm">
          <thead className="bg-white/5 text-slate-300">
            <tr>
              <th className="px-4 py-3 font-medium">Job ID</th>
              <th className="px-4 py-3 font-medium">Job name</th>
              <th className="px-4 py-3 font-medium">Status</th>
              <th className="px-4 py-3 font-medium">Compute</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-white/10 bg-slate-950/35">
            {jobs.length === 0 ? (
              <tr>
                <td className="px-4 py-8 text-slate-400" colSpan={4}>
                  No jobs have been loaded yet. Create one above or refresh the billing summary.
                </td>
              </tr>
            ) : (
              jobs.map((job) => (
                <tr
                  key={job.jobId}
                  className={`cursor-pointer transition hover:bg-white/5 ${selectedJobId === job.jobId ? 'bg-cyan-400/8' : ''}`}
                  onClick={() => onSelect(job.jobId)}
                >
                  <td className="max-w-40 px-4 py-4 font-mono text-xs text-slate-300">{job.jobId}</td>
                  <td className="px-4 py-4 font-medium text-white">{job.jobName}</td>
                  <td className="px-4 py-4">
                    <span className={`inline-flex rounded-full border px-3 py-1 text-xs font-semibold ${statusTone(job.status)}`}>
                      {job.status}
                    </span>
                  </td>
                  <td className="px-4 py-4 text-slate-200">{job.computeType}</td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>
    </section>
  );
}