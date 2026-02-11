using FluentValidation;

namespace Intec.Banking.FinancialInstitutions.Application.Features.FinancialInstitutions.GetFinancialInstitutions;

public class GetFinancialInstitutionsQueryValidator
    : AbstractValidator<GetFinancialInstitutionsQuery>
{
    public GetFinancialInstitutionsQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Page must be greater than or equal to 1.");

        RuleFor(x => x.PageSize)
            .GreaterThanOrEqualTo(1)
            .WithMessage("PageSize must be greater than or equal to 1.")
            .LessThanOrEqualTo(100)
            .WithMessage("PageSize cannot exceed 100.");
    }
}