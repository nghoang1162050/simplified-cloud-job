using api.Models;

namespace api.Entities;

public class JobEntity
{
    public string JobId { get; set; } = string.Empty;
    public string JobName { get; set; } = string.Empty;
    public string ProjectId { get; set; } = string.Empty;
    public ComputeTypeEnums ComputeType { get; set; }
    public string Status { get; set; } = JobStatusEnums.Submitted.ToString();
    public string? OutputFileReference { get; set; }
    public string InputFileName { get; set; } = string.Empty;
    public int ExecutionDuration { get; set; }
    public double CreditCost { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
