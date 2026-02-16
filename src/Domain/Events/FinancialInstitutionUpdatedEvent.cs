using Intec.Banking.FinancialInstitutions.Primitives;

namespace Intec.Banking.FinancialInstitutions.Domain.Events;

public sealed class FinancialInstitutionUpdatedEvent : DomainEvent
{
    public Guid FinancialInstitutionId { get; }

    public FinancialInstitutionUpdatedEvent(Guid financialInstitutionId)
    {
        FinancialInstitutionId = financialInstitutionId;
    }
}
