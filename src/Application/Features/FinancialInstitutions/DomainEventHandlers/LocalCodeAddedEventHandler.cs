using Intec.Banking.FinancialInstitutions.Domain.Events;
using Intec.Banking.FinancialInstitutions.Primitives;
using Microsoft.Extensions.Logging;

namespace Intec.Banking.FinancialInstitutions.Application.Features.FinancialInstitutions.DomainEventHandlers;

/// <summary>
/// Handles <see cref="LocalCodeAddedEvent"/>.
/// Logs the local code registration for observability. Additional side-effects
/// (e.g. notifying payment processors, updating routing tables) are added here.
/// </summary>
internal sealed class LocalCodeAddedEventHandler
    : IDomainEventHandler<LocalCodeAddedEvent>
{
    private readonly ILogger<LocalCodeAddedEventHandler> _logger;

    public LocalCodeAddedEventHandler(
        ILogger<LocalCodeAddedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task HandleAsync(LocalCodeAddedEvent domainEvent, CancellationToken ct = default)
    {
        _logger.LogInformation(
            "Local code added. FinancialInstitutionId: {FinancialInstitutionId}, Code: {LocalCode}, OccurredOn: {OccurredOn}",
            domainEvent.FinancialInstitutionId,
            domainEvent.LocalCode,
            domainEvent.OccurredOn);

        return Task.CompletedTask;
    }
}