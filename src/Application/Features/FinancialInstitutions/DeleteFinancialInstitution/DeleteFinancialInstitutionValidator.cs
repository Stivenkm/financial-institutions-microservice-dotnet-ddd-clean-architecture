using FluentValidation;

namespace Intec.Banking.FinancialInstitutions.Application.Features.FinancialInstitutions.DeleteFinancialInstitution;

public class DeleteFinancialInstitutionValidator : AbstractValidator<DeleteFinancialInstitutionCommand>
{
    public DeleteFinancialInstitutionValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Financial institution id is required");
    }
}