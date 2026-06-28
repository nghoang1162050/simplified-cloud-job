using FluentValidation;

namespace api.Models;

public class BillingSummaryFilterRequestValidator : AbstractValidator<BillingSummaryFilterRequest>
{
    public BillingSummaryFilterRequestValidator()
    {
        RuleFor(x => x.ProjectId)
            .Must(id => !string.IsNullOrWhiteSpace(id))
            .When(x => x.ProjectId != null)
            .WithMessage("ProjectId cannot be empty or whitespace if provided.");

        RuleFor(x => x.ComputeType)
            .IsInEnum()
            .When(x => x.ComputeType.HasValue)
            .WithMessage("The provided ComputeType is invalid or not supported.");

        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Page number must be greater than or equal to 1.");

        RuleFor(x => x.PageSize)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Page size must be at least 1.")
            .LessThanOrEqualTo(100)
            .WithMessage("Page size cannot exceed 100 records per request.");
    }
}
