using Intec.Banking.FinancialInstitutions.Domain.ValueObjects;
using Intec.Banking.FinancialInstitutions.Primitives;

namespace Intec.Banking.FinancialInstitutions.Application.Features.FinancialInstitutions.DeleteFinancialInstitution;
public class DeleteFinancialInstitutionCommand : ICommand<FinancialInstitutionId>
{
    public FinancialInstitutionId Id { get; set; } = default!;
}
