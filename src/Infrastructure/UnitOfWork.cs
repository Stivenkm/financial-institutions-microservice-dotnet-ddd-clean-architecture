using Intec.Banking.FinancialInstitutions.Infrastructure.DomainEvents;
using Intec.Banking.FinancialInstitutions.Primitives;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Intec.Banking.FinancialInstitutions.Infrastructure;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly FinancialInstitutionsDbContext _context;
    private readonly DomainEventDispatcher _dispatcher;
    private IDbContextTransaction? _transaction;

    public UnitOfWork(
        FinancialInstitutionsDbContext context,
        DomainEventDispatcher dispatcher)
    {
        _context = context;
        _dispatcher = dispatcher;
    }

    /// <summary>
    /// Persists all pending changes, dispatches accumulated domain events,
    /// then increments the aggregate version for each added or modified aggregate.
    /// Order is intentional: persist first so handlers operate on committed state.
    /// </summary>
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Collect aggregates with pending domain events BEFORE saving.
        // We cannot filter by EF state (Added/Modified) because some operations
        // (AddLocalCode, SetColombianDetails) only mutate owned child entities —
        // the parent aggregate remains Unchanged in the change tracker even though
        // it has accumulated domain events that must be dispatched.
        var aggregatesWithEvents = _context.ChangeTracker
            .Entries<IAggregate>()
            .Select(e => e.Entity)
            .Where(a => a is IHaveDomainEvents h && h.HasUncommittedDomainEvents())
            .ToList();

        // Collect aggregates that were added or modified for version increment.
        // EF resets state to Unchanged after SaveChangesAsync, so we capture now.
        var aggregatesForVersioning = _context.ChangeTracker
            .Entries<IAggregate>()
            .Where(e => e.State is EntityState.Added or EntityState.Modified)
            .Select(e => e.Entity)
            .ToList();

        var result = await _context.SaveChangesAsync(cancellationToken);

        // Dispatch domain events from all aggregates that had pending events.
        foreach (var aggregate in aggregatesWithEvents)
        {
            if (aggregate is IHaveDomainEvents hasDomainEvents)
            {
                var events = hasDomainEvents.DequeueUncommittedDomainEvents();
                foreach (var domainEvent in events)
                    await _dispatcher.DispatchAsync(domainEvent, cancellationToken);
            }
        }

        // Increment version only for aggregates that were persisted as Added or Modified.
        foreach (var aggregate in aggregatesForVersioning)
            aggregate.IncrementVersion();

        return result;
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        _transaction ??= await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction is null) return;

        await _transaction.CommitAsync(cancellationToken);
        await _transaction.DisposeAsync();
        _transaction = null;
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction is null) return;

        await _transaction.RollbackAsync(cancellationToken);
        await _transaction.DisposeAsync();
        _transaction = null;
    }
}