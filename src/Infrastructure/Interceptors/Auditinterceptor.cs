using Intec.Banking.FinancialInstitutions.Infrastructure.Services;
using Intec.Banking.FinancialInstitutions.Primitives;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Intec.Banking.FinancialInstitutions.Infrastructure.Interceptors;

/// <summary>
/// Interceptor that automatically handles:
/// - Audit fields (CreatedAt, UpdatedAt) via IHaveAudit
/// - Soft delete (IsDeleted, DeletedAt) via IHaveSoftDelete
/// - Optimistic concurrency (Version++) via IHaveAggregateVersion
/// - Multi-tenancy (TenantId) via IHaveTenant
///
/// Runs on every SaveChanges — domain and application layers remain unaware.
/// </summary>
public sealed class AuditInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUserService _currentUserService;

    private readonly ITenantService _tenantService;

    public AuditInterceptor(ICurrentUserService currentUserService,ITenantService tenantService)
    {
        _currentUserService = currentUserService;

        _tenantService = tenantService;
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

        foreach (var entry in context.ChangeTracker.Entries())
        {
            if (entry.Entity is not IAggregate aggregate) continue;

            switch (entry.State)
            {
                case EntityState.Added:
                    aggregate.SetCreated(now, currentUserId);
                    aggregate.SetTenant(_tenantService.GetRequiredTenantId());
                    break;

                case EntityState.Modified:
                    if (aggregate is IHaveSoftDelete { IsDeleted: true, Deleted: null })
                        aggregate.SetDeleted(now, currentUserId);
                    else
                        aggregate.SetLastModified(now, currentUserId);

                    aggregate.IncrementVersion();
                    break;

                case EntityState.Deleted:
                    entry.State = EntityState.Modified;
                    aggregate.SetDeleted(now, currentUserId);
                    aggregate.IncrementVersion();
                    break;
            }
        }
    }
}
