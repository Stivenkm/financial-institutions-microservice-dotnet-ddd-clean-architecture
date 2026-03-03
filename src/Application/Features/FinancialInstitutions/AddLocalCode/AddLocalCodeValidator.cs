using FluentValidation;
using Intec.Banking.FinancialInstitutions.Application.Features.FinancialInstitutions.AddLocalCode;

public sealed class AddLocalCodeRequestValidator : AbstractValidator<AddLocalCodeRequest>
{
    public AddLocalCodeRequestValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty()
            .WithMessage("Bank code is required.")
            .MaximumLength(50)
            .WithMessage("Bank code cannot exceed 50 characters.");

        RuleFor(x => x.CodeType)
            .NotEmpty()
            .WithMessage("Code type is required.")
            .MaximumLength(20)
            .WithMessage("Code type cannot exceed 20 characters.");
    }
}