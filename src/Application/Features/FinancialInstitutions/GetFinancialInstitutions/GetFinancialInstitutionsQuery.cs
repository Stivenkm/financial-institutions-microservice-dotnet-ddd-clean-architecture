using Intec.Banking.FinancialInstitutions.Application.DTOs;
using Intec.Banking.FinancialInstitutions.Primitives;

namespace Intec.Banking.FinancialInstitutions.Application.Features.FinancialInstitutions.GetFinancialInstitutions;

public sealed record GetFinancialInstitutionsQuery(
    int Page,
    int PageSize
) : IQuery<IReadOnlyList<FinancialInstitutionDto>>;
