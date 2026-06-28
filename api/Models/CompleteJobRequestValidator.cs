using FluentValidation;

namespace api.Models;

public class CompleteJobRequestValidator : AbstractValidator<CompleteJobRequest>
{
    public CompleteJobRequestValidator()
    {
        RuleFor(x => x.ExecutionDuration)
            .GreaterThan(0).WithMessage("Execution duration must be greater than 0 seconds.");

        RuleFor(x => x.OutputFileReference)
            .NotEmpty().WithMessage("Output file reference is required.");
    }
}
