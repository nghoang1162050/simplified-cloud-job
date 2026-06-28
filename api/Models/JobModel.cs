namespace api.Models;

public class JobModelBase
{
    public required string JobId { get; set; }
    public int ExecutionDuration { get; set; }
    public string? OutputFileReference { get; set; }
}

public class JobModel : JobModelBase
{
    public required string JobName { get; set; }
    public required string ProjectId { get; set; }
    public required ComputeTypeEnums ComputeType { get; set; }
    public required string InputFileName { get; set; }
    public required string Status { get; set; }
    public double CreditCost { get; set; }
    public DateTime CreatedAt { get; set; }
}

public record CreateJobRequest(
    string JobName,
    string ProjectId,
    ComputeTypeEnums ComputeType,
    string InputFileName
);

public record CompleteJobRequest(
    int ExecutionDuration,
    string OutputFileReference
);
