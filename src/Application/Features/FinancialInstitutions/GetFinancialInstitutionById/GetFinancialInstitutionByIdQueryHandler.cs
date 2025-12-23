using Intec.Banking.FinancialInstitutions.Application.DTOs;
using Intec.Banking.FinancialInstitutions.Infrastructure;
using Intec.Banking.FinancialInstitutions.Primitives;

namespace Intec.Banking.FinancialInstitutions.Application.Features.FinancialInstitutions.GetFinancialInstitutionById;

public class GetFinancialInstitutionByIdQueryHandler
    : IQueryHandler<GetFinancialInstitutionByIdQuery, FinancialInstitutionDto?>
{
    private readonly IFinancialInstitutionRepository _repository;

    public GetFinancialInstitutionByIdQueryHandler(IFinancialInstitutionRepository repository)
    {
        _repository = repository;
    }

    public async Task<FinancialInstitutionDto?> HandleAsync(
        GetFinancialInstitutionByIdQuery query,
        CancellationToken ct = default)
    {
        var institution = await _repository.GetByIdAsync(query.Id, ct);

        if (institution is null)
            return null;

        return new FinancialInstitutionDto(
            institution.Id.Value,
            institution.OfficialName,
            institution.TradeName,
            institution.Country.ToString()
        );
    }
}
