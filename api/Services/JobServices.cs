using api.Configurations;
using api.Entities;
using api.Models;
using api.Repositories;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;

namespace api.Services;

public class JobServices(
    IJobRepository jobRepository,
    IStorageService storageService,
    IComputeExecutionService computeExecutionService,
    IOptions<AwsSettings> awsOptions) : IJobServices
{
    private readonly IJobRepository _jobRepository = jobRepository;
    private readonly IStorageService _storageService = storageService;
    private readonly IComputeExecutionService _computeExecutionService = computeExecutionService;
    private readonly AwsSettings _awsSettings = awsOptions.Value;

    public async Task<ApiResponse<JobModel>> SubmitJobAsync(CreateJobRequest request)
    {
        // 1. Initialize and compute unique request identification data
        string originalFileName = $"input_{request.File.FileName}";
        string requestHash = GenerateRequestHash(request.ProjectId, request.ComputeType, originalFileName);

        // 2. State Guard Clause: Enforce Idempotency to prevent duplicate job processing
        var existingJob = await _jobRepository.GetByHashAsync(requestHash);
        if (existingJob != null)
        {
            return ApiResponse<JobModel>.SuccessResponse(
                MapToJobModel(existingJob),
                "An identical job request is already processing or completed. Returned existing job."
            );
        }

        // 3. Offload binary payload stream storage to AWS S3 Cloud
        string fileStorageReference = await UploadInputFileToS3Async(request.File, originalFileName, requestHash);

        // 4. Factory initialization and persistence to relational Database
        var newJobEntity = CreateInitialJobEntity(request, requestHash, fileStorageReference);
        await _jobRepository.AddAsync(newJobEntity);

        // 5. Trigger remote infrastructure compute workload asynchronously via AWS SSM
        var (IsSuccess, ErrorMessage) = await TryTriggerEc2AutomationAsync(newJobEntity.JobId);
        if (!IsSuccess)
        {
            return ApiResponse<JobModel>.ErrorResponse(ErrorMessage);
        }

        // 6. Return a clean, mapped Data Transfer Object (DTO) payload to the client
        return ApiResponse<JobModel>.SuccessResponse(MapToJobModel(newJobEntity), "Job submitted and automation loop triggered.");
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
        // 1. Retrieve the target job entity from the database
        var jobEntity = await _jobRepository.GetByIdAsync(jobId);
        if (jobEntity == null)
        {
            return ApiResponse<JobModelBase>.ErrorResponse($"Job with ID '{jobId}' was not found.");
        }

        // 2. State Guard Clause: Prevent processing if the job has already been completed
        if (IsJobAlreadyCompleted(jobEntity))
        {
            return ApiResponse<JobModelBase>.ErrorResponse($"Job with ID '{jobId}' is already finalized. Cannot complete again.");
        }

        // 3. Process and upload the output binary artifact to AWS S3
        string originalOutputName = $"output_{request.OutputFile.FileName}";
        string s3OutputKey = await UploadOutputFileToS3Async(request.OutputFile, originalOutputName, jobEntity.RequestHash);

        // 4. Update state mutation and calculate billing costs
        FinalizeJobEntityState(jobEntity, request.ExecutionDuration, s3OutputKey);
        await _jobRepository.UpdateAsync(jobEntity);

        // 5. Construct and return the finalized clean DTO payload
        var resultPayload = MapToJobModelBase(jobEntity);
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

    /// <summary>
    /// Computes a unique SHA256 request hash based on business boundary data.
    /// </summary>
    private static string GenerateRequestHash(string projectId, ComputeTypeEnums computeType, string fileName)
    {
        string rawData = $"{projectId}_{computeType}_{fileName}";
        return ComputeRequestHash(rawData);
    }

    /// <summary>
    /// Safely opens the file stream and uploads the input payload to AWS S3.
    /// </summary>
    private async Task<string> UploadInputFileToS3Async(IFormFile file, string fileName, string requestHash)
    {
        using var fileStream = file.OpenReadStream();
        return await _storageService.UploadFileAsync(fileStream, fileName, requestHash);
    }

    /// <summary>
    /// Factory method to isolate the initialization of a fresh JobEntity.
    /// </summary>
    private static JobEntity CreateInitialJobEntity(CreateJobRequest request, string requestHash, string storageReference)
    {
        return new JobEntity
        {
            JobId = Guid.NewGuid().ToString(),
            JobName = request.JobName,
            ProjectId = request.ProjectId,
            ComputeType = request.ComputeType,
            Status = JobStatusEnums.Submitted.ToString(),
            OutputFileReference = null,
            InputFileName = storageReference,
            ExecutionDuration = 0,
            CreditCost = 0,
            CreatedAt = DateTime.UtcNow,
            RequestHash = requestHash
        };
    }

    /// <summary>
    /// Wraps the remote EC2 SSM trigger invocation with structured error handling.
    /// </summary>
    private async Task<(bool IsSuccess, string ErrorMessage)> TryTriggerEc2AutomationAsync(string jobId)
    {
        string bashCommand = BuildEc2UtcTimestampAndCompleteCommand(jobId, _awsSettings.ApiBaseUrl);

        try
        {
            await _computeExecutionService.TriggerRemoteCommandAsync(bashCommand);
            return (true, string.Empty);
        }
        catch (Exception ex)
        {
            // Return a clean descriptive tuple state instead of throwing or swallowing errors abruptly
            return (false, $"Job created, but EC2 execution loop failed: {ex.Message}");
        }
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

    /// <summary>
    /// Evaluates whether the job entity has already transitioned to a Completed state.
    /// </summary>
    private static bool IsJobAlreadyCompleted(JobEntity jobEntity)
    {
        return jobEntity.Status == JobStatusEnums.Completed.ToString();
    }

    /// <summary>
    /// Safely opens the output file stream and offloads it to the chronological S3 bucket infrastructure.
    /// </summary>
    private async Task<string> UploadOutputFileToS3Async(IFormFile file, string fileName, string requestHash)
    {
        using var outputStream = file.OpenReadStream();
        return await _storageService.UploadFileAsync(outputStream, fileName, requestHash);
    }

    /// <summary>
    /// Mutates the entity's state to a finalized state and dynamically computes the billing credit costs.
    /// </summary>
    private static void FinalizeJobEntityState(JobEntity jobEntity, int executionDuration, string s3OutputKey)
    {
        int cost;
        int durationInMinutes = CalculateDurationInMinutes(executionDuration);

        if (jobEntity.ComputeType == ComputeTypeEnums.CpuSmall)
        {
            cost = 1 * durationInMinutes;
        }
        else if (jobEntity.ComputeType == ComputeTypeEnums.CpuLarge)
        {
            cost = 2 * durationInMinutes;
        }
        else if (jobEntity.ComputeType == ComputeTypeEnums.Gpu)
        {
            cost = 8 * durationInMinutes;
        }

        jobEntity.ExecutionDuration = executionDuration;
        jobEntity.OutputFileReference = s3OutputKey;
        jobEntity.Status = JobStatusEnums.Completed.ToString();

        // TODO
        jobEntity.CreditCost = 111;
    }

    /// <summary>
    /// returns the execution duration in minutes, rounded up to the nearest whole number.
    /// </summary>
    /// <param name="executionDurationInSeconds">seconds</param>
    /// <returns></returns>
    private static int CalculateDurationInMinutes(int executionDurationInSeconds)
    {
        int modPart = executionDurationInSeconds % 60;
        int basePart = executionDurationInSeconds / 60;
        int minutes = basePart;

        if (modPart > 0)
        {
            minutes = 1 + basePart;
        }

        return minutes;
    }

    /// <summary>
    /// Direct mapping factory from the updated Domain Entity to the lightweight JobModelBase DTO.
    /// </summary>
    private static JobModelBase MapToJobModelBase(JobEntity jobEntity)
    {
        return new JobModelBase
        {
            JobId = jobEntity.JobId,
            ExecutionDuration = jobEntity.ExecutionDuration,
            OutputFileReference = jobEntity.OutputFileReference
        };
    }

    private static string BuildEc2UtcTimestampAndCompleteCommand(string jobId, string apiBaseUrl)
    {
        string targetFilePath = $"/home/ubuntu/job_{jobId}.txt";

        string logCommand = $"date -u +'Hi there, it''s %d/%m/%Y %H:%M:%S UTC now!' > {targetFilePath}";

        string curlCommand = $"curl -X POST \"{apiBaseUrl.TrimEnd('/')}/api/jobs/{jobId}/complete\" " +
                             $"-F \"ExecutionDuration=5\" " +
                             $"-F \"OutputFile=@{targetFilePath}\"";

        return $"{logCommand} && {curlCommand}";
    }

    #endregion
}
