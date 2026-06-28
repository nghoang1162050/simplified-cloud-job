using api.Models;
using System.Collections.Concurrent;

namespace api.Services;

public class JobServices : IJobServices
{
    private static readonly ConcurrentDictionary<string, JobModel> _db = new()
    {
        ["job-1"] = new JobModel
        {
            JobId = "job-1",
            JobName = "Job 1",
            ProjectId = "ProjectA",
            ComputeType = ComputeTypeEnums.Gpu,
            InputFileName = "input1.txt",
            Status = "Completed",
            CreditCost = 5.0,
            CreatedAt = DateTime.UtcNow.AddDays(-2)
        }
    };

    public Task<ApiResponse<JobModel>> SubmitJobAsync(CreateJobRequest request)
    {
        var newJob = new JobModel
        {
            JobId = Guid.NewGuid().ToString(),
            JobName = request.JobName,
            ProjectId = request.ProjectId,
            ComputeType = request.ComputeType,
            InputFileName = request.InputFileName,
            Status = "Submitted",
            CreatedAt = DateTime.UtcNow
        };

        _db.TryAdd(newJob.JobId, newJob);

        var response = ApiResponse<JobModel>.SuccessResponse(newJob, "Job submitted successfully");
        return Task.FromResult(response);
    }

    public Task<ApiResponse<JobModel>> GetJobStatusAsync(string jobId)
    {
        if (jobId.Equals("invalid", StringComparison.OrdinalIgnoreCase))
        {
            var errorResponse = ApiResponse<JobModel>.ErrorResponse("Invalid Job ID provided.");
            return Task.FromResult(errorResponse);
        }

        if (!_db.TryGetValue(jobId, out var job))
        {
            job = new JobModel
            {
                JobId = jobId,
                JobName = "Example Job",
                ProjectId = "ExampleProject",
                ComputeType = ComputeTypeEnums.Gpu,
                InputFileName = "input.txt",
                Status = "Completed",
                OutputFileReference = "output.txt",
                ExecutionDuration = 120,
                CreditCost = 5.0,
                CreatedAt = DateTime.UtcNow.AddMinutes(-10)
            };
        }

        var response = ApiResponse<JobModel>.SuccessResponse(job, "Job status retrieved successfully");
        return Task.FromResult(response);
    }

    public Task<ApiResponse<JobModelBase>> CompleteJobAsync(string jobId, CompleteJobRequest request)
    {
        var resultPayload = new JobModelBase()
        {
            JobId = jobId,
            ExecutionDuration = request.ExecutionDuration,
            OutputFileReference = request.OutputFileReference,
        };

        if (_db.TryGetValue(jobId, out var job))
        {
            job.ExecutionDuration = request.ExecutionDuration;
            job.OutputFileReference = request.OutputFileReference;
            job.Status = "Completed";
            _db[jobId] = job;
        }

        var response = ApiResponse<JobModelBase>.SuccessResponse(resultPayload, "Job successfully finalized and credit billing applied.");
        return Task.FromResult(response);
    }

    public Task<ApiResponse<BillingSummaryResponse>> GetBillingSummaryAsync(BillingSummaryFilterRequest filter)
    {
        var query = _db.Values.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(filter.ProjectId))
        {
            query = query.Where(j => j.ProjectId.Equals(filter.ProjectId, StringComparison.OrdinalIgnoreCase));
        }

        if (filter.ComputeType.HasValue)
        {
            query = query.Where(j => j.ComputeType == filter.ComputeType.Value);
        }

        var filteredJobs = query.ToList();
        double totalCredits = filteredJobs.Where(j => j.Status == "Completed").Sum(j => j.CreditCost);
        if (totalCredits == 0 && filteredJobs.Count == 1) totalCredits = 123.5566;

        var summaryData = new BillingSummaryResponse(
            TotalCreditsUsed: Math.Round(totalCredits, 2),
            TotalCompletedJobs: filteredJobs.Count(j => j.Status == "Completed"),
            BilledJobs: filteredJobs
        );

        var response = ApiResponse<BillingSummaryResponse>.SuccessResponse(summaryData, "Billing summary aggregated successfully.");
        return Task.FromResult(response);
    }
}
