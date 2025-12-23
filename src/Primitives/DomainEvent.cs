namespace Intec.Banking.FinancialInstitutions.Primitives;

/// <summary>
/// Base class for all domain events
/// </summary>
public abstract class DomainEvent : IDomainEvent
{
    protected DomainEvent()
    {
        EventId = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
    }

    public Guid EventId { get; }
    public DateTime OccurredOn { get; }
}
