using System.ComponentModel.DataAnnotations;

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

public class CreateJobRequest
{
    [Required]
    public string JobName { get; set; } = string.Empty;

    [Required]
    public string ProjectId { get; set; } = string.Empty;

    [Required]
    public ComputeTypeEnums ComputeType { get; set; }

    [Required]
    public IFormFile File { get; set; } = null!;
}

public class CompleteJobRequest
{
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Execution duration must be at least 1 second.")]
    public int ExecutionDuration { get; set; }

    [Required]
    public IFormFile OutputFile { get; set; } = null!;
}
