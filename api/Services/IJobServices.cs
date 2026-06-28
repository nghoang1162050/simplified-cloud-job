using api.Models;

namespace api.Services;

public interface IJobServices
{
    Task<ApiResponse<JobModel>> SubmitJobAsync(CreateJobRequest request);
    Task<ApiResponse<JobModel>> GetJobStatusAsync(string jobId);
    Task<ApiResponse<JobModelBase>> CompleteJobAsync(string jobId, CompleteJobRequest request);
    Task<ApiResponse<BillingSummaryResponse>> GetBillingSummaryAsync(BillingSummaryFilterRequest filter);
}
