using Intec.Banking.FinancialInstitutions.Domain;
using Intec.Banking.FinancialInstitutions.Domain.ValueObjects;
using Intec.Banking.FinancialInstitutions.Infrastructure;
using Intec.Banking.FinancialInstitutions.Primitives;

namespace Intec.Banking.FinancialInstitutions.Application.Features.FinancialInstitutions.CreateFinancialInstitution;

public class CreateFinancialInstitutionCommandHandler
    : ICommandHandler<CreateFinancialInstitutionCommand, FinancialInstitutionId>
{
    private readonly IFinancialInstitutionRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateFinancialInstitutionCommandHandler(
        IFinancialInstitutionRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<FinancialInstitutionId> HandleAsync(
        CreateFinancialInstitutionCommand command,
        CancellationToken ct = default)
    {
        var country = CountryCode.Create(command.CountryCode);
        var taxId = TaxId.Create(command.TaxIdValue, country);
        var swiftBic = string.IsNullOrWhiteSpace(command.SwiftBicCode)
            ? null
            : SwiftBic.Create(command.SwiftBicCode);

        var institution = FinancialInstitution.CreateBank(
            officialName: command.OfficialName,
            tradeName: command.TradeName,
            country: country,
            taxId: taxId,
            swiftBic: swiftBic
        );

        await _repository.AddAsync(institution, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return institution.Id;
    }
}
