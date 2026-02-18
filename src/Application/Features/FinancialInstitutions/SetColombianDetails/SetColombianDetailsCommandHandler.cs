using Ardalis.GuardClauses;
using Intec.Banking.FinancialInstitutions.Domain;
using Intec.Banking.FinancialInstitutions.Domain.ValueObjects;
using Intec.Banking.FinancialInstitutions.Infrastructure;
using Intec.Banking.FinancialInstitutions.Primitives;

namespace Intec.Banking.FinancialInstitutions.Application.Features.FinancialInstitutions.SetColombianDetails;

internal sealed class SetColombianDetailsCommandHandler
    : ICommandHandler<SetColombianDetailsCommand, FinancialInstitutionId>
{
    private readonly IFinancialInstitutionRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public SetColombianDetailsCommandHandler(
        IFinancialInstitutionRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<FinancialInstitutionId> HandleAsync(
        SetColombianDetailsCommand command,
        CancellationToken ct)
    {
        var institution = await _repository.GetByIdAsync(command.Id, ct);

        if (institution is null)
            throw new NotFoundException(command.Id.Value.ToString(), nameof(FinancialInstitution));

        // Validamos primero el AGGREGATE
        if (!institution.Country.IsColombia())
            throw new DomainException(
                "Colombian details only allowed for Colombian institutions.");

        var achBankCode = LocalBankCode.Create(
            command.AchCode,
            "ACH",
            institution.Country);

        var details = ColombianBankingDetails.Create(achBankCode, command.SuperFinancialCode);

        institution.SetColombianDetails(details);

        await _unitOfWork.SaveChangesAsync(ct);

        return institution.Id;
    }
}

