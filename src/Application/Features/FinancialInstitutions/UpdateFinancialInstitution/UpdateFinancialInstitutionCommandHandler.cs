using Ardalis.GuardClauses;
using Intec.Banking.FinancialInstitutions.Application.Features.FinancialInstitutions.UpdateFinancialInstituion;
using Intec.Banking.FinancialInstitutions.Domain;
using Intec.Banking.FinancialInstitutions.Domain.ValueObjects;
using Intec.Banking.FinancialInstitutions.Infrastructure;
using Intec.Banking.FinancialInstitutions.Primitives;

namespace Intec.Banking.FinancialInstitutions.Application.Features.FinancialInstitutions.UpdateFinancialInstitution;

public sealed class UpdateFinancialInstitutionCommandHandler
    : ICommandHandler<UpdateFinancialInstitutionCommand, FinancialInstitutionId>
{
    private readonly IFinancialInstitutionRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateFinancialInstitutionCommandHandler(
        IFinancialInstitutionRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<FinancialInstitutionId> HandleAsync(
        UpdateFinancialInstitutionCommand command,
        CancellationToken ct)
    {
        // Buscar entidad existente
        var institution = await _repository.GetByIdAsync(command.Id, ct);

        if (institution is null)
            throw new NotFoundException(command.Id.Value.ToString(), nameof(FinancialInstitution));

        // Construir estado final (DDD correcto: aggregate recibe estado completo)

        var finalOfficialName = string.IsNullOrWhiteSpace(command.OfficialName)
                ? institution.OfficialName
                : command.OfficialName.Trim();

        var finalTradeName = command.TradeName ?? institution.TradeName;

        var finalCountry = !string.IsNullOrWhiteSpace(command.CountryCode)
                ? CountryCode.Create(command.CountryCode)
                : institution.Country;

        var finalTaxId = !string.IsNullOrWhiteSpace(command.TaxIdValue)
                ? TaxId.Create(command.TaxIdValue, finalCountry)
                : institution.TaxId;

        var finalSwift =!string.IsNullOrWhiteSpace(command.SwiftBicCode)
                ? SwiftBic.Create(command.SwiftBicCode)
                : institution.SwiftBic;

        // Aplicar cambios en el Aggregate Root
        institution.Update(
            finalOfficialName,
            finalTradeName,
            finalCountry,
            finalTaxId,
            finalSwift);

        // Persistir cambios
        await _unitOfWork.SaveChangesAsync(ct);

        return institution.Id;
    }
}

