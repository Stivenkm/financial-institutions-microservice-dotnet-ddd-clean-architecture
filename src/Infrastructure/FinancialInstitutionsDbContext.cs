using Intec.Banking.FinancialInstitutions.Domain;
using Intec.Banking.FinancialInstitutions.Domain.ValueObjects;
using Intec.Banking.FinancialInstitutions.Infrastructure.Services;
using Intec.Banking.FinancialInstitutions.Primitives;
using Microsoft.EntityFrameworkCore;

namespace Intec.Banking.FinancialInstitutions.Infrastructure;

public class FinancialInstitutionsDbContext : DbContext
{
    private readonly ITenantService _tenantService;
    public FinancialInstitutionsDbContext(DbContextOptions<FinancialInstitutionsDbContext> options, ITenantService tenantService)
        : base(options)
    {
        _tenantService = tenantService;
    }

    public DbSet<FinancialInstitution> FinancialInstitutions => Set<FinancialInstitution>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FinancialInstitutionsDbContext).Assembly);

        // Global query filters — applied to every query automatically
        // Combines tenant isolation + soft delete in a single filter
        // When TenantId is null (no request context), filter is bypassed
        // Use IgnoreQueryFilters() explicitly for cross-tenant/admin operations
        modelBuilder.Entity<FinancialInstitution>()
            .HasQueryFilter(x => !x.IsDeleted && (_tenantService.TenantId == null || x.TenantId == _tenantService.TenantId));
    }
}