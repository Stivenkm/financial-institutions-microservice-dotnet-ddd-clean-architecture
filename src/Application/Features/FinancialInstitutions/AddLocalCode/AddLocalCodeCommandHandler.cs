using Ardalis.GuardClauses;
using Intec.Banking.FinancialInstitutions.Domain;
using Intec.Banking.FinancialInstitutions.Domain.ValueObjects;
using Intec.Banking.FinancialInstitutions.Infrastructure;
using Intec.Banking.FinancialInstitutions.Primitives;

namespace Intec.Banking.FinancialInstitutions.Application.Features.FinancialInstitutions.AddLocalCode;

public sealed class AddLocalCodeCommandHandler
    : ICommandHandler<AddLocalCodeCommand, FinancialInstitutionId>
{
    private readonly IFinancialInstitutionRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public AddLocalCodeCommandHandler(
        IFinancialInstitutionRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<FinancialInstitutionId> HandleAsync(
        AddLocalCodeCommand command,
        CancellationToken ct)
    {
        var institution = await _repository.GetByIdAsync(command.Id, ct);

        if (institution is null)
            throw new NotFoundException(command.Id.Value.ToString(),nameof(FinancialInstitution));

        // Crear LocalBankCode
        var localCode = LocalBankCode.Create(command.Code, command.CodeType, institution.Country);

        institution.AddLocalCode(localCode);

        await _unitOfWork.SaveChangesAsync(ct);

        return institution.Id;
    }
}
