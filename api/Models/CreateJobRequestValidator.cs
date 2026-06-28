using FluentValidation;

namespace api.Models;

public class CreateJobRequestValidator : AbstractValidator<CreateJobRequest>
{
    public CreateJobRequestValidator()
    {
        RuleFor(x => x.JobName)
            .NotEmpty().WithMessage("Job name is required.")
            .MaximumLength(100).WithMessage("Job name must not exceed 100 characters.");

        RuleFor(x => x.ProjectId)
            .NotEmpty().WithMessage("Project ID is required.")
            .MaximumLength(50).WithMessage("Project ID must not exceed 50 characters.");

        RuleFor(x => x.ComputeType)
            .IsInEnum().WithMessage($"Compute type is required.");

        RuleFor(x => x.InputFileName)
            .NotEmpty().WithMessage("Input file name is required.")
            .Must(name => name.Contains('.'))
            .WithMessage("Input file name must include a valid file extension (e.g., .dat, .csv).");
    }
}
