using FluentValidation;

namespace Intec.Banking.FinancialInstitutions.Application.Features.FinancialInstitutions.SearchFinancialInstitutions;

public class SearchFinancialInstitutionsQueryValidator : AbstractValidator<SearchFinancialInstitutionsQuery>
{
    public SearchFinancialInstitutionsQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}