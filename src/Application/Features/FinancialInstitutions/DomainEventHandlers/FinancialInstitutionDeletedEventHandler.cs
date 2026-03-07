using Intec.Banking.FinancialInstitutions.Domain.Events;
using Intec.Banking.FinancialInstitutions.Primitives;
using Microsoft.Extensions.Logging;

namespace Intec.Banking.FinancialInstitutions.Application.Features.FinancialInstitutions.DomainEventHandlers;

/// <summary>
/// Handles <see cref="FinancialInstitutionDeletedEvent"/>.
/// Logs the deletion for observability. Additional side-effects
/// (e.g. publishing integration events, audit trails) are added here.
/// </summary>
internal sealed class FinancialInstitutionDeletedEventHandler
    : IDomainEventHandler<FinancialInstitutionDeletedEvent>
{
    private readonly ILogger<FinancialInstitutionDeletedEventHandler> _logger;

    public FinancialInstitutionDeletedEventHandler(
        ILogger<FinancialInstitutionDeletedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task HandleAsync(FinancialInstitutionDeletedEvent domainEvent, CancellationToken ct = default)
    {
        _logger.LogInformation(
            "Financial institution deleted. Id: {FinancialInstitutionId}, OccurredOn: {OccurredOn}",
            domainEvent.FinancialInstitutionId,
            domainEvent.OccurredOn);

        return Task.CompletedTask;
    }
}