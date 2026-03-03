using FluentValidation;
using Intec.Banking.FinancialInstitutions.Application.Common;

namespace Intec.Banking.FinancialInstitutions.Application.Features.FinancialInstitutions.SearchFinancialInstitutions;

public sealed class SearchFinancialInstitutionsQueryValidator: AbstractValidator<SearchFinancialInstitutionsQuery>
{
    public SearchFinancialInstitutionsQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(PaginationParams.DefaultMinPage)
            .WithMessage($"Page must be greater than or equal to {PaginationParams.DefaultMinPage}.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(PaginationParams.DefaultMinPageSize, PaginationParams.DefaultMaxPageSize)
            .WithMessage($"PageSize must be between {PaginationParams.DefaultMinPageSize} and {PaginationParams.DefaultMaxPageSize}.");

        // Country is optional — validate only when provided
        RuleFor(x => x.CountryCode)
            .Length(2, 3)
            .WithMessage("Country code must be 2 or 3 characters (ISO 3166).")
            .When(x => !string.IsNullOrWhiteSpace(x.CountryCode));

        RuleFor(x => x.SwiftBicCode)
            .Matches(@"^[A-Z]{6}[A-Z0-9]{2}([A-Z0-9]{3})?$")
            .WithMessage("SWIFT/BIC must be 8 or 11 uppercase characters: 6 letters + 2 alphanumeric + optional 3 alphanumeric.")
            .When(x => !string.IsNullOrWhiteSpace(x.SwiftBicCode));
    }
}