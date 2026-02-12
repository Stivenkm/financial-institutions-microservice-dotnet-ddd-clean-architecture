using Intec.Banking.FinancialInstitutions.Application.DTOs;
using Intec.Banking.FinancialInstitutions.Application.Features.FinancialInstitutions.SearchFinancialInstitutions;
using Intec.Banking.FinancialInstitutions.Infrastructure;
using Intec.Banking.FinancialInstitutions.Primitives;

public class SearchFinancialInstitutionsQueryHandler
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
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);

        var institutions = await _repository.SearchAsync(
            query.Country,
            query.Name,
            query.SwiftBicCode,
            query.Page,
            query.PageSize,
            ct);

        return institutions.Select(x => new FinancialInstitutionDto(
            x.Id.Value,
            x.OfficialName,
            x.TradeName,
            x.Country.ToString()
        )).ToList();
    }
}
