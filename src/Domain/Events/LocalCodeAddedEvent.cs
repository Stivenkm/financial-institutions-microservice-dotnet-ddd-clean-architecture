using Intec.Banking.FinancialInstitutions.Primitives;

namespace Intec.Banking.FinancialInstitutions.Domain.Events;

public sealed class LocalCodeAddedEvent : DomainEvent
{
    public Guid FinancialInstitutionId { get; }
    public string LocalCode { get; }

    public LocalCodeAddedEvent(Guid financialInstitutionId, string localCode)
    {
        FinancialInstitutionId = financialInstitutionId;
        LocalCode = localCode;
    }
}
