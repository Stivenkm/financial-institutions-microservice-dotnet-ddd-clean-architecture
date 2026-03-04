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
        // Collect before save — EF resets state to Unchanged after SaveChangesAsync
        var aggregates = _context.ChangeTracker
            .Entries<IAggregate>()
            .Where(e => e.State is EntityState.Added or EntityState.Modified)
            .Select(e => e.Entity)
            .ToList();

        var result = await _context.SaveChangesAsync(cancellationToken);

        foreach (var aggregate in aggregates)
        {
            if (aggregate is IHaveDomainEvents hasDomainEvents)
            {
                var events = hasDomainEvents.DequeueUncommittedDomainEvents();
                foreach (var domainEvent in events)
                    await _dispatcher.DispatchAsync(domainEvent, cancellationToken);
            }

            aggregate.IncrementVersion();
        }

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