using FluentValidation;

namespace api.Models;

public class BillingSummaryFilterRequestValidator : AbstractValidator<BillingSummaryFilterRequest>
{
    public BillingSummaryFilterRequestValidator()
    {
        RuleFor(x => x.ProjectId)
            .Must(id => id == null || !string.IsNullOrWhiteSpace(id))
            .WithMessage("Project ID cannot be empty or whitespaces if provided.");

        RuleFor(x => x.ComputeType)
            .IsInEnum()
            .When(x => x.ComputeType.HasValue)
            .WithMessage("Compute type is invalid. Valid options are: cpu-small, cpu-large, gpu.");
    }
}
