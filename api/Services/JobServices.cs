using api.Entities;
using api.Models;
using api.Repositories;

namespace api.Services;

public class JobServices(IJobRepository jobRepository) : IJobServices
{
    private readonly IJobRepository _jobRepository = jobRepository;

    public async Task<ApiResponse<JobModel>> SubmitJobAsync(CreateJobRequest request)
    {
        // TODO: check same request already exists in db, if yes return existing job id and status, hash request then check if hash exists in memory

        // TODO: put file to s3

        var newJobEntity = new JobEntity
        {
            JobId = Guid.NewGuid().ToString(),
            JobName = request.JobName,
            ProjectId = request.ProjectId,
            ComputeType = request.ComputeType,
            Status = JobStatusEnums.Submitted.ToString(),
            OutputFileReference = null,
            InputFileName = request.InputFileName,
            ExecutionDuration = 0,
            CreditCost = 0,
            CreatedAt = DateTime.UtcNow
        };

        await _jobRepository.AddAsync(newJobEntity);

        var jobModel = new JobModel
        {
            JobId = newJobEntity.JobId,
            JobName = newJobEntity.JobName,
            ProjectId = newJobEntity.ProjectId,
            ComputeType = newJobEntity.ComputeType,
            InputFileName = request.InputFileName,
            Status = newJobEntity.Status.ToString(),
            CreatedAt = newJobEntity.CreatedAt
        };

        return ApiResponse<JobModel>.SuccessResponse(jobModel, "Job submitted successfully");
    }

    public async Task<ApiResponse<JobModel>> GetJobStatusAsync(string jobId)
    {
        var jobEntity = await _jobRepository.GetByIdAsync(jobId);

        if (jobEntity == null)
        {
            return ApiResponse<JobModel>.ErrorResponse($"Job with ID '{jobId}' was not found.");
        }

        var jobModel = new JobModel
        {
            JobId = jobEntity.JobId,
            JobName = jobEntity.JobName,
            ProjectId = jobEntity.ProjectId,
            ComputeType = jobEntity.ComputeType,
            InputFileName = jobEntity.InputFileName,
            Status = jobEntity.Status.ToString(),
            OutputFileReference = jobEntity.OutputFileReference,
            ExecutionDuration = jobEntity.ExecutionDuration,
            CreditCost = jobEntity.CreditCost,
            CreatedAt = jobEntity.CreatedAt
        };

        return ApiResponse<JobModel>.SuccessResponse(jobModel, "Job status retrieved successfully");
    }

    public async Task<ApiResponse<JobModelBase>> CompleteJobAsync(string jobId, CompleteJobRequest request)
    {
        var jobEntity = await _jobRepository.GetByIdAsync(jobId);

        if (jobEntity == null)
        {
            return ApiResponse<JobModelBase>.ErrorResponse($"Job with ID '{jobId}' was not found.");
        }

        jobEntity.ExecutionDuration = request.ExecutionDuration;
        jobEntity.OutputFileReference = request.OutputFileReference;
        jobEntity.Status = JobStatusEnums.Completed.ToString();

        // Example Business Logic: Calculate credit cost dynamically (e.g., 0.05 credits per second)
        jobEntity.CreditCost = request.ExecutionDuration * 0.05;

        await _jobRepository.UpdateAsync(jobEntity);

        var resultPayload = new JobModelBase
        {
            JobId = jobEntity.JobId,
            ExecutionDuration = jobEntity.ExecutionDuration,
            OutputFileReference = jobEntity.OutputFileReference
        };

        return ApiResponse<JobModelBase>.SuccessResponse(resultPayload, "Job successfully finalized and credit billing applied.");
    }

    public async Task<ApiResponse<BillingSummaryResponse>> GetBillingSummaryAsync(BillingSummaryFilterRequest filter)
    {
        var pagedEntities = await _jobRepository.GetFilteredJobsAsync(
            filter.ProjectId,
            filter.ComputeType?.ToString(),
            filter.Page,
            filter.PageSize);

        var jobModels = pagedEntities.Items.Select(entity => new JobModel
        {
            JobId = entity.JobId,
            JobName = entity.JobName,
            ProjectId = entity.ProjectId,
            ComputeType = entity.ComputeType,
            InputFileName = entity.InputFileName,
            Status = entity.Status.ToString(),
            OutputFileReference = entity.OutputFileReference,
            ExecutionDuration = entity.ExecutionDuration,
            CreditCost = entity.CreditCost,
            CreatedAt = entity.CreatedAt
        }).ToList();

        var completedJobs = jobModels.Where(j => j.Status == JobStatusEnums.Completed.ToString()).ToList();
        double totalCredits = completedJobs.Sum(j => j.CreditCost);

        var summaryData = new BillingSummaryResponse(
            TotalCreditsUsed: Math.Round(totalCredits, 2),
            TotalCompletedJobs: completedJobs.Count,
            BilledJobs: jobModels.AsReadOnly(),
            Pagination: new PagedMetadata(
                Page: pagedEntities.Page,
                PageSize: pagedEntities.PageSize,
                TotalCount: pagedEntities.TotalCount,
                TotalPages: pagedEntities.TotalPages,
                HasNextPage: pagedEntities.HasNextPage,
                HasPreviousPage: pagedEntities.HasPreviousPage
            )
        );

        return ApiResponse<BillingSummaryResponse>.SuccessResponse(summaryData, "Billing summary aggregated and paged successfully.");
    }
}
