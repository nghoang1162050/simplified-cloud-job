namespace api.Models;

public class BillingSummaryFilterRequest
{
    public string? ProjectId { get; set; }
    public ComputeTypeEnums? ComputeType { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
