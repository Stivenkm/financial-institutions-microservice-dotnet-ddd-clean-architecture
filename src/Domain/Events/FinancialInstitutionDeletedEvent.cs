using Intec.Banking.FinancialInstitutions.Primitives;

namespace Intec.Banking.FinancialInstitutions.Domain.Events;

public sealed class FinancialInstitutionDeletedEvent : DomainEvent
{
    public Guid FinancialInstitutionId { get; }

    public FinancialInstitutionDeletedEvent(Guid financialInstitutionId)
    {
        FinancialInstitutionId = financialInstitutionId;
    }
}