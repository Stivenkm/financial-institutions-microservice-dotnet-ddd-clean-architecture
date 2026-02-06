using Intec.Banking.FinancialInstitutions.Domain.ValueObjects;
using Intec.Banking.FinancialInstitutions.Infrastructure;
using Intec.Banking.FinancialInstitutions.Primitives;

namespace Intec.Banking.FinancialInstitutions.Application.Features.FinancialInstitutions.DeleteFinancialInstitution;

public class DeleteFinancialInstitutionCommandHandler
    : ICommandHandler<DeleteFinancialInstitutionCommand, FinancialInstitutionId>
{
    private readonly IFinancialInstitutionRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteFinancialInstitutionCommandHandler(
        IFinancialInstitutionRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<FinancialInstitutionId> HandleAsync(
        DeleteFinancialInstitutionCommand command,
        CancellationToken ct = default)
    {

        var institution = await _repository.GetByIdAsync(command.Id, ct);

        if (institution is null)
            throw new KeyNotFoundException("Financial institution not found.");

        await _repository.DeleteAsync(command.Id,ct);

        await _unitOfWork.SaveChangesAsync(ct);

        return command.Id;
    }
}
