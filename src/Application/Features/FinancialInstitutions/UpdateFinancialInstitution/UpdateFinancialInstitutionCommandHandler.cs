using Intec.Banking.FinancialInstitutions.Domain.ValueObjects;
using Intec.Banking.FinancialInstitutions.Infrastructure;
using Intec.Banking.FinancialInstitutions.Primitives;
using Intec.Banking.FinanciialInstitutions.Application.Features.FinnacialInstitutions.UpdateFinancialIntituion;

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
        // 1️ Buscar entidad existente
        var institution = await _repository.GetByIdAsync(command.Id, ct);

        if (institution is null)
            throw new Exception("Financial Institution not found");

        // 2️ Reconstruir ValueObjects
        var country = CountryCode.Create(command.CountryCode);
        var taxId = TaxId.Create(command.TaxIdValue, country);
        var swift = command.SwiftBicCode is not null
            ? SwiftBic.Create(command.SwiftBicCode)
            : null;

        // 3️ Aplicar cambios en el Aggregate Root (DDD puro)
        institution.Update(
            command.OfficialName,
            command.TradeName,
            country,
            taxId,
            swift);

        //4️ Persistir cambios
        await _repository.UpdateAsync(institution, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return institution.Id;
    }
}

