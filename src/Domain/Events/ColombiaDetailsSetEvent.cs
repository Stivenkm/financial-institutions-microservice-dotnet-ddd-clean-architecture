using Intec.Banking.FinancialInstitutions.Primitives;

namespace Intec.Banking.FinancialInstitutions.Domain.Events;

public sealed class ColombianDetailsSetEvent : DomainEvent
{
    public Guid FinancialInstitutionId { get; }

    public ColombianDetailsSetEvent(Guid financialInstitutionId)
    {
        FinancialInstitutionId = financialInstitutionId;
    }
}