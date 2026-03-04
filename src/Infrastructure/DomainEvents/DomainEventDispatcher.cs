using Intec.Banking.FinancialInstitutions.Primitives;
using Microsoft.Extensions.DependencyInjection;

namespace Intec.Banking.FinancialInstitutions.Infrastructure.DomainEvents;

/// <summary>
/// Resolves and invokes all registered handlers for a given domain event.
/// Supports multiple handlers per event type (fan-out).
/// Mirrors the CommandDispatcher pattern — no external dependencies required.
/// </summary>
public sealed class DomainEventDispatcher
{
    private readonly IServiceProvider _serviceProvider;

    public DomainEventDispatcher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Dispatches <paramref name="domainEvent"/> to all registered
    /// <see cref="IDomainEventHandler{TEvent}"/> implementations.
    /// If no handlers are registered the call is a no-op.
    /// </summary>
    public async Task DispatchAsync(IDomainEvent domainEvent, CancellationToken ct = default)
    {
        var eventType = domainEvent.GetType();
        var handlerType = typeof(IDomainEventHandler<>).MakeGenericType(eventType);
        var enumerableType = typeof(IEnumerable<>).MakeGenericType(handlerType);

        var handlers = _serviceProvider.GetService(enumerableType);
        if (handlers is not IEnumerable<object> handlerList) return;

        var method = handlerType.GetMethod(nameof(IDomainEventHandler<IDomainEvent>.HandleAsync))!;

        foreach (var handler in handlerList)
        {
            var task = (Task)method.Invoke(handler, new object[] { domainEvent, ct })!;
            await task;
        }
    }
}