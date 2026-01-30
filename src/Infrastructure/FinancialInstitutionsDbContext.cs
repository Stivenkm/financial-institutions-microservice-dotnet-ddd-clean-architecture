using Intec.Banking.FinancialInstitutions.Domain;
using Intec.Banking.FinancialInstitutions.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Intec.Banking.FinancialInstitutions.Infrastructure;

public class FinancialInstitutionsDbContext : DbContext
{
    public FinancialInstitutionsDbContext(DbContextOptions<FinancialInstitutionsDbContext> options)
        : base(options)
    {
    }

    public DbSet<FinancialInstitution> FinancialInstitutions => Set<FinancialInstitution>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FinancialInstitutionsDbContext).Assembly);
 
    }
}