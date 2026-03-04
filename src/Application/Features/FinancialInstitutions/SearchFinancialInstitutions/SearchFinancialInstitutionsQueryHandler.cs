using Intec.Banking.FinancialInstitutions.Application.Common;
using Intec.Banking.FinancialInstitutions.Application.DTOs;
using Intec.Banking.FinancialInstitutions.Infrastructure;
using Intec.Banking.FinancialInstitutions.Primitives;

namespace Intec.Banking.FinancialInstitutions.Application.Features.FinancialInstitutions.SearchFinancialInstitutions;

public sealed class SearchFinancialInstitutionsQueryHandler
    : IQueryHandler<SearchFinancialInstitutionsQuery, IReadOnlyList<FinancialInstitutionDto>>
{
    private readonly IFinancialInstitutionRepository _repository;

    public SearchFinancialInstitutionsQueryHandler(IFinancialInstitutionRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<FinancialInstitutionDto>> HandleAsync(
        SearchFinancialInstitutionsQuery query,
        CancellationToken ct = default)
    {
        var pagination = new PaginationParams(query.Page, query.PageSize);

        var institutions = await _repository.SearchAsync(
            query.CountryCode,
            query.Name,
            query.SwiftBicCode,
            pagination.Page,
            pagination.PageSize,
            ct);

        return institutions.Select(x => new FinancialInstitutionDto(
            x.Id.Value,
            x.OfficialName,
            x.TradeName,
            x.Country.ToString(),
            x.OriginalVersion)).ToList();
    }
}