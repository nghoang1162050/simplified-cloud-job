namespace api.Models;

public record BillingSummaryResponse(
    double TotalCreditsUsed,
    int TotalCompletedJobs,
    List<JobModel> BilledJobs
);
