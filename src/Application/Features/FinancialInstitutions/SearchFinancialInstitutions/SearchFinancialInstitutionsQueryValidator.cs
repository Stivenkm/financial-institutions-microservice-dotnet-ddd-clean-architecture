using FluentValidation;

namespace Intec.Banking.FinancialInstitutions.Application.Features.FinancialInstitutions.SearchFinancialInstitutions;

public class SearchFinancialInstitutionsQueryValidator : AbstractValidator<SearchFinancialInstitutionsQuery>
{
    public SearchFinancialInstitutionsQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);

        // Country is optional — validate only when provided
        RuleFor(x => x.Country)
            .Length(2, 3)
            .WithMessage("Country code must be 2 or 3 characters (ISO 3166).")
            .When(x => !string.IsNullOrWhiteSpace(x.Country));

        // SwiftBic is optional — validate format when provided
        RuleFor(x => x.SwiftBicCode)
            .Length(8, 11)
            .WithMessage("SWIFT/BIC code must be between 8 and 11 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.SwiftBicCode));
    }
}