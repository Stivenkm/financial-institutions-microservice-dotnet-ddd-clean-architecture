using FluentValidation;
using Intec.Banking.FinancialInstitutions.Application.Features.FinancialInstitutions.SetColombianDetails;

public sealed class SetColombianDetailsRequestValidator : AbstractValidator<SetColombianDetailsRequest>
{
    public SetColombianDetailsRequestValidator()
    {
        RuleFor(x => x.AchCode)
            .NotEmpty()
            .WithMessage("ACH code is required.")
            .MaximumLength(50)
            .WithMessage("ACH code cannot exceed 50 characters.");

        RuleFor(x => x.SuperFinancialCode)
            .MaximumLength(20)
            .WithMessage("SuperFinancial code cannot exceed 20 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.SuperFinancialCode));
    }
}