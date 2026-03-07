using FluentValidation;

namespace Intec.Banking.FinancialInstitutions.Application.Features.FinancialInstitutions.GetFinancialInstitutionById;

public sealed class GetFinancialInstitutionByIdQueryValidator : AbstractValidator<GetFinancialInstitutionByIdQuery>
{
    public GetFinancialInstitutionByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotNull()
            .WithMessage("Id is required.");
    }
}