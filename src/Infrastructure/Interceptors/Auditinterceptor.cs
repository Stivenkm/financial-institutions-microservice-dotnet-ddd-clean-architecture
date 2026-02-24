using Intec.Banking.FinancialInstitutions.Primitives;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Intec.Banking.FinancialInstitutions.Infrastructure.Interceptors;

/// <summary>
/// Interceptor that automatically populates audit fields (CreatedAt, LastModified)
/// and handles soft delete (IsDeleted, Deleted, DeletedBy) on every SaveChanges.
///
/// This keeps audit concerns out of the domain and application layers —
/// the Aggregate Root exposes internal setters that only this interceptor calls.
/// </summary>
public sealed class AuditInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUserService _currentUserService;

    public AuditInterceptor(ICurrentUserService currentUserService)
    {
        _currentUserService = currentUserService;
    }
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        ApplyAudit(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        ApplyAudit(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void ApplyAudit(DbContext? context)
    {
        if (context is null) return;

        var now = DateTime.UtcNow;

        var currentUserId = _currentUserService.UserId;

        foreach (var entry in context.ChangeTracker.Entries<IAggregate>())
        {

            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.SetCreated(now, currentUserId);
                    break;

                case EntityState.Modified:

                    if (entry.Entity is IHaveSoftDelete sd && sd.IsDeleted && sd.Deleted is null)
                    {
                        entry.Entity.SetDeleted(now, currentUserId);
                    }
                    else
                    {
                        entry.Entity.SetLastModified(now, currentUserId);
                    }
                    break;
                }
            }
        }
}