using Ardalis.GuardClauses;
using Intec.Banking.FinancialInstitutions.Application.Features.FinancialInstitutions.UpdateFinancialInstituion;
using Intec.Banking.FinancialInstitutions.Domain;
using Intec.Banking.FinancialInstitutions.Domain.ValueObjects;
using Intec.Banking.FinancialInstitutions.Infrastructure;
using Intec.Banking.FinancialInstitutions.Primitives;
using Microsoft.EntityFrameworkCore;

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
        var institution = await _repository.GetByIdAsync(command.Id, ct);
        if (institution is null)
            throw new NotFoundException(command.Id.Value.ToString(), nameof(FinancialInstitution));

        // Optimistic concurrency — verify version matches what the client read.
        // If versions differ, another user already modified this institution.
        // OriginalVersion is required — clients must always send the version they read.
        if (institution.OriginalVersion != command.OriginalVersion)
            throw new DbUpdateConcurrencyException(
               $"Concurrency conflict on field 'originalVersion': client sent {command.OriginalVersion}, " +
               $"current value is {institution.OriginalVersion}. Fetch the latest version and retry.");

        var country = CountryCode.Create(command.CountryCode);
        var taxId = TaxId.Create(command.TaxIdValue, country);
        var swift = command.SwiftBicCode switch
        {
            null => null,
            "" => institution.SwiftBic,
            _ => SwiftBic.Create(command.SwiftBicCode)
        };

        // Apply changes via Aggregate Root — domain validates all invariants:
        // - OfficialName cannot be empty
        // - TaxId country must match institution country
        // - SWIFT required for non-Colombian institutions
        institution.Update(
            officialName: command.OfficialName,
            tradeName: command.TradeName,
            country: country,
            taxId: taxId,
            swiftBic: swift);

        await _unitOfWork.SaveChangesAsync(ct);

        return institution.Id;
    }
}