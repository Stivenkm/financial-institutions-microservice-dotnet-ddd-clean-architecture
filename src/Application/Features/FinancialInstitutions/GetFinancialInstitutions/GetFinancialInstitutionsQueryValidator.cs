using FluentValidation;
using Intec.Banking.FinancialInstitutions.Application.Common;

namespace Intec.Banking.FinancialInstitutions.Application.Features.FinancialInstitutions.GetFinancialInstitutions;

public sealed class GetFinancialInstitutionsQueryValidator
    : AbstractValidator<GetFinancialInstitutionsQuery>
{
    public GetFinancialInstitutionsQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(PaginationParams.DefaultMinPage)
            .WithMessage($"Page must be greater than or equal to {PaginationParams.DefaultMinPage}.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(PaginationParams.DefaultMinPageSize, PaginationParams.DefaultMaxPageSize)
            .WithMessage($"PageSize must be between {PaginationParams.DefaultMinPageSize} and {PaginationParams.DefaultMaxPageSize}.");
    }
}