namespace api.Models;

public record BillingSummaryResponse(
    double TotalCreditsUsed,
    int TotalCompletedJobs,
    IReadOnlyCollection<JobModel> BilledJobs,
    PagedMetadata Pagination
);
