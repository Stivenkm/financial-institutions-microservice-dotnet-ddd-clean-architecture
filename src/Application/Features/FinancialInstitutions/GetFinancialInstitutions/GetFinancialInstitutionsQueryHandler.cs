using Intec.Banking.FinancialInstitutions.Application.Common;
using Intec.Banking.FinancialInstitutions.Application.DTOs;
using Intec.Banking.FinancialInstitutions.Infrastructure;
using Intec.Banking.FinancialInstitutions.Primitives;

namespace Intec.Banking.FinancialInstitutions.Application.Features.FinancialInstitutions.GetFinancialInstitutions;

public sealed class GetFinancialInstitutionsQueryHandler
    : IQueryHandler<GetFinancialInstitutionsQuery, IReadOnlyList<FinancialInstitutionDto>>
{
    private readonly IFinancialInstitutionRepository _repository;

    public GetFinancialInstitutionsQueryHandler(IFinancialInstitutionRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<FinancialInstitutionDto>> HandleAsync(
        GetFinancialInstitutionsQuery query,
        CancellationToken ct = default)
    {
        var pagination = new PaginationParams(query.Page, query.PageSize);

        var institutions = await _repository.GetPagedAsync(
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