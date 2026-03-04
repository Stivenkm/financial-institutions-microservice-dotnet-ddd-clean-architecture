using Intec.Banking.FinancialInstitutions.Domain.Events;
using Intec.Banking.FinancialInstitutions.Primitives;
using Microsoft.Extensions.Logging;

namespace Intec.Banking.FinancialInstitutions.Application.Features.FinancialInstitutions.DomainEventHandlers;

/// <summary>
/// Handles <see cref="FinancialInstitutionCreatedEvent"/>.
/// Logs the creation for observability. Additional side-effects
/// (e.g. publishing integration events, sending notifications) are added here.
/// </summary>
internal sealed class FinancialInstitutionCreatedEventHandler
    : IDomainEventHandler<FinancialInstitutionCreatedEvent>
{
    private readonly ILogger<FinancialInstitutionCreatedEventHandler> _logger;

    public FinancialInstitutionCreatedEventHandler(
        ILogger<FinancialInstitutionCreatedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task HandleAsync(FinancialInstitutionCreatedEvent domainEvent, CancellationToken ct = default)
    {
        _logger.LogInformation(
            "Financial institution created. Id: {FinancialInstitutionId}, OccurredOn: {OccurredOn}",
            domainEvent.FinancialInstitutionId,
            domainEvent.OccurredOn);

        return Task.CompletedTask;
    }
}