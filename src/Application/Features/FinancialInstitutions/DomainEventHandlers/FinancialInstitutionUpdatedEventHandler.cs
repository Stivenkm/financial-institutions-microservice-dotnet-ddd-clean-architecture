using Intec.Banking.FinancialInstitutions.Domain.Events;
using Intec.Banking.FinancialInstitutions.Primitives;
using Microsoft.Extensions.Logging;

namespace Intec.Banking.FinancialInstitutions.Application.Features.FinancialInstitutions.DomainEventHandlers;

/// <summary>
/// Handles <see cref="FinancialInstitutionUpdatedEvent"/>.
/// Logs the update for observability. Additional side-effects
/// (e.g. publishing integration events, cache invalidation) are added here.
/// </summary>
internal sealed class FinancialInstitutionUpdatedEventHandler
    : IDomainEventHandler<FinancialInstitutionUpdatedEvent>
{
    private readonly ILogger<FinancialInstitutionUpdatedEventHandler> _logger;

    public FinancialInstitutionUpdatedEventHandler(
        ILogger<FinancialInstitutionUpdatedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task HandleAsync(FinancialInstitutionUpdatedEvent domainEvent, CancellationToken ct = default)
    {
        _logger.LogInformation(
            "Financial institution updated. Id: {FinancialInstitutionId}, OccurredOn: {OccurredOn}",
            domainEvent.FinancialInstitutionId,
            domainEvent.OccurredOn);

        return Task.CompletedTask;
    }
}