using Intec.Banking.FinancialInstitutions.Application.DTOs;
using Intec.Banking.FinancialInstitutions.Primitives;

namespace Intec.Banking.FinancialInstitutions.Application.Features.FinancialInstitutions.SearchFinancialInstitutions;

public record SearchFinancialInstitutionsQuery(
    string? Country,
    string? Name,
    string? SwiftBicCode,
    int Page,
    int PageSize
) : IQuery<IReadOnlyList<FinancialInstitutionDto>>;