namespace api.Models;

public class BillingSummaryFilterRequest
{
    public string? ProjectId { get; set; }
    public ComputeTypeEnums? ComputeType { get; set; }
}
