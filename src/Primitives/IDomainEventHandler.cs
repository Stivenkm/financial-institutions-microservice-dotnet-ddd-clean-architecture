namespace Intec.Banking.FinancialInstitutions.Primitives;

/// <summary>
/// Defines a handler for a specific domain event.
/// Multiple handlers can be registered for the same event type.
/// </summary>
public interface IDomainEventHandler<in TEvent> where TEvent : IDomainEvent
{
    Task HandleAsync(TEvent domainEvent, CancellationToken ct = default);
}