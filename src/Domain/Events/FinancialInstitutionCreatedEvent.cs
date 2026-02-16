using Intec.Banking.FinancialInstitutions.Primitives;

namespace Intec.Banking.FinancialInstitutions.Domain.Events;

public sealed class FinancialInstitutionCreatedEvent : DomainEvent
{
    public Guid FinancialInstitutionId { get; }

    public FinancialInstitutionCreatedEvent(Guid financialInstitutionId)
    {
        FinancialInstitutionId = financialInstitutionId;
    }
}
