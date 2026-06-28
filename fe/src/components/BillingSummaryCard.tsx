import type { BillingSummaryResponse } from '../types';

interface Props {
  summary?: BillingSummaryResponse;
  onPrevPage: () => void;
  onNextPage: () => void;
  onRefresh: () => Promise<void>;
}

export default function BillingSummaryCard({ summary, onPrevPage, onNextPage, onRefresh }: Props) {
  return (
    <section className="glass-panel rounded-3xl p-6 fade-in">
      <div className="flex items-start justify-between gap-4">
        <div>
          <p className="text-xs uppercase tracking-[0.35em] text-emerald-300/80">Billing</p>
          <h2 className="mt-2 text-2xl font-semibold text-white">Usage and pagination</h2>
        </div>
        <button
          className="rounded-2xl border border-white/10 bg-white/5 px-4 py-2 text-sm text-slate-200 transition hover:bg-white/10"
          type="button"
          onClick={() => void onRefresh()}
        >
          Refresh
        </button>
      </div>

      {!summary ? (
        <div className="mt-6 rounded-2xl border border-dashed border-white/15 bg-white/5 p-6 text-sm text-slate-300">
          No billing summary loaded yet.
        </div>
      ) : (
        <div className="mt-6 space-y-4">
          <div className="grid gap-4 sm:grid-cols-3">
            <Stat label="Total credits" value={summary.totalCreditsUsed.toFixed(2)} accent="text-cyan-200" />
            <Stat label="Completed jobs" value={String(summary.totalCompletedJobs)} accent="text-emerald-200" />
            <Stat label="Visible rows" value={String(summary.billedJobs.length)} accent="text-slate-100" />
          </div>

          <div className="rounded-2xl border border-white/10 bg-slate-950/35 p-4 text-sm text-slate-200">
            <div className="flex flex-wrap items-center justify-between gap-3">
              <div>
                <div className="text-xs uppercase tracking-[0.25em] text-slate-400">Pagination</div>
                <div className="mt-1">
                  Page {summary.pagination.page} of {summary.pagination.totalPages} · Page size {summary.pagination.pageSize} · Total {summary.pagination.totalCount}
                </div>
              </div>
              <div className="flex gap-2">
                <button
                  className="rounded-xl border border-white/10 bg-white/5 px-4 py-2 disabled:cursor-not-allowed disabled:opacity-50"
                  type="button"
                  onClick={onPrevPage}
                  disabled={!summary.pagination.hasPreviousPage}
                >
                  Previous
                </button>
                <button
                  className="rounded-xl border border-white/10 bg-white/5 px-4 py-2 disabled:cursor-not-allowed disabled:opacity-50"
                  type="button"
                  onClick={onNextPage}
                  disabled={!summary.pagination.hasNextPage}
                >
                  Next
                </button>
              </div>
            </div>
          </div>
        </div>
      )}
    </section>
  );
}

function Stat({ label, value, accent }: { label: string; value: string; accent: string }) {
  return (
    <div className="rounded-2xl border border-white/10 bg-white/5 p-4">
      <div className="text-xs uppercase tracking-[0.25em] text-slate-400">{label}</div>
      <div className={`mt-2 text-2xl font-semibold ${accent}`}>{value}</div>
    </div>
  );
}