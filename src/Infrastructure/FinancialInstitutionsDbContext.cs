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
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<FinancialInstitution>(entity =>
        {
            entity.HasKey(e => e.Id);

            // Configure FinancialInstitutionId as value conversion
            entity.Property(e => e.Id)
                .HasConversion(
                    id => id.Value,
                    value => FinancialInstitutionId.From(value))
                .ValueGeneratedNever();

            entity.Property(e => e.OfficialName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.TradeName).HasMaxLength(200);

            // Configure CountryCode as owned entity or value conversion
            entity.OwnsOne(e => e.Country, country =>
            {
                country.Property(c => c.Code)
                    .HasColumnName("CountryCode")
                    .IsRequired()
                    .HasMaxLength(3);
            });

            // Configure TaxId as owned entity
            entity.OwnsOne(e => e.TaxId, taxId =>
            {
                taxId.Property(t => t.Value)
                    .HasColumnName("TaxIdValue")
                    .IsRequired()
                    .HasMaxLength(50);

                taxId.OwnsOne(t => t.Country, country =>
                {
                    country.Property(c => c.Code)
                        .HasColumnName("TaxIdCountryCode")
                        .IsRequired()
                        .HasMaxLength(3);
                });
            });

            // Configure SwiftBic as owned entity (nullable)
            entity.OwnsOne(e => e.SwiftBic, swiftBic =>
            {
                swiftBic.Property(s => s.Code)
                    .HasColumnName("SwiftBicCode")
                    .HasMaxLength(11);
            });

            // Ignore complex collections for now
            entity.Ignore(e => e.LocalCodes);
            entity.Ignore(e => e.ColombianDetails);
        });
    }
}