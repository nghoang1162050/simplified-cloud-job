using api.Entities;
using api.Models;
using api.Repositories;
using System.Security.Cryptography;
using System.Text;

namespace api.Services;

public class JobServices(IJobRepository jobRepository, IStorageService storageService) : IJobServices
{
    private readonly IJobRepository _jobRepository = jobRepository;
    private readonly IStorageService _storageService = storageService;

    public async Task<ApiResponse<JobModel>> SubmitJobAsync(CreateJobRequest request)
    {
        string originalFileName = $"input_{request.File.FileName}";
        string rawData = $"{request.ProjectId}_{request.ComputeType}_{originalFileName}";
        string requestHash = ComputeRequestHash(rawData);

        // Check for existing job with the same hash to prevent duplicate processing
        var existingJob = await _jobRepository.GetByHashAsync(requestHash);
        if (existingJob != null)
        {
            var existingJobModel = MapToJobModel(existingJob);
            return ApiResponse<JobModel>.SuccessResponse(
                existingJobModel,
                "An identical job request is already processing or completed. Returned existing job."
            );
        }

        // Upload the file to S3 and get the storage reference
        Stream fileStream = request.File.OpenReadStream();
        string fileStorageReference = await _storageService.UploadFileAsync(fileStream, originalFileName, requestHash);

        var newJobEntity = new JobEntity
        {
            JobId = Guid.NewGuid().ToString(),
            JobName = request.JobName,
            ProjectId = request.ProjectId,
            ComputeType = request.ComputeType,
            Status = JobStatusEnums.Submitted.ToString(),
            OutputFileReference = null,
            InputFileName = fileStorageReference,
            ExecutionDuration = 0,
            CreditCost = 0,
            CreatedAt = DateTime.UtcNow,
            RequestHash = requestHash
        };

        await _jobRepository.AddAsync(newJobEntity);

        var jobModel = new JobModel
        {
            JobId = newJobEntity.JobId,
            JobName = newJobEntity.JobName,
            ProjectId = newJobEntity.ProjectId,
            ComputeType = newJobEntity.ComputeType,
            InputFileName = fileStorageReference,
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
        // TODO: will make a function to calculate the credit cost based on execution duration and compute type in the future
        const double CreditRatePerSecond = 0.05;

        var jobEntity = await _jobRepository.GetByIdAsync(jobId);
        if (jobEntity == null)
        {
            return ApiResponse<JobModelBase>.ErrorResponse($"Job with ID '{jobId}' was not found.");
        }

        if (jobEntity.Status == JobStatusEnums.Completed.ToString())
        {
            return ApiResponse<JobModelBase>.ErrorResponse($"Job with ID '{jobId}' is already finalized. Cannot complete again.");
        }

        string originalOutputName = $"output_{request.OutputFile.FileName}";
        string s3OutputKey;

        using (var outputStream = request.OutputFile.OpenReadStream())
        {
            s3OutputKey = await _storageService.UploadFileAsync(outputStream, originalOutputName, jobEntity.RequestHash);
        }

        jobEntity.ExecutionDuration = request.ExecutionDuration;
        jobEntity.OutputFileReference = s3OutputKey;
        jobEntity.Status = JobStatusEnums.Completed.ToString();
        jobEntity.CreditCost = Math.Round(request.ExecutionDuration * CreditRatePerSecond, 2);

        await _jobRepository.UpdateAsync(jobEntity);

        var resultPayload = new JobModelBase
        {
            JobId = jobEntity.JobId,
            ExecutionDuration = jobEntity.ExecutionDuration,
            OutputFileReference = jobEntity.OutputFileReference
        };

        return ApiResponse<JobModelBase>.SuccessResponse(resultPayload, "Job successfully finalized and cloud credit billing applied.");
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

    #region Helper Methods

    private static string ComputeRequestHash(string rawData)
    {
        using var sha256 = SHA256.Create();
        byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawData));
        return Convert.ToHexString(bytes);
    }

    private static JobModel MapToJobModel(JobEntity entity)
    {
        return new JobModel
        {
            JobId = entity.JobId,
            JobName = entity.JobName,
            ProjectId = entity.ProjectId,
            ComputeType = entity.ComputeType,
            InputFileName = entity.InputFileName,
            Status = entity.Status.ToString(),
            CreatedAt = entity.CreatedAt
        };
    }

    #endregion
}
