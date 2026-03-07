using Intec.Banking.FinancialInstitutions.Domain.Events;
using Intec.Banking.FinancialInstitutions.Primitives;
using Microsoft.Extensions.Logging;

namespace Intec.Banking.FinancialInstitutions.Application.Features.FinancialInstitutions.DomainEventHandlers;

/// <summary>
/// Handles <see cref="ColombianDetailsSetEvent"/>.
/// Logs the Colombian banking details registration for observability. Additional side-effects
/// (e.g. notifying regulatory systems, updating ACH routing tables) are added here.
/// </summary>
internal sealed class ColombianDetailsSetEventHandler
    : IDomainEventHandler<ColombianDetailsSetEvent>
{
    private readonly ILogger<ColombianDetailsSetEventHandler> _logger;

    public ColombianDetailsSetEventHandler(
        ILogger<ColombianDetailsSetEventHandler> logger)
    {
        _logger = logger;
    }

    public Task HandleAsync(ColombianDetailsSetEvent domainEvent, CancellationToken ct = default)
    {
        _logger.LogInformation(
            "Colombian banking details set. FinancialInstitutionId: {FinancialInstitutionId}, OccurredOn: {OccurredOn}",
            domainEvent.FinancialInstitutionId,
            domainEvent.OccurredOn);

        return Task.CompletedTask;
    }
}