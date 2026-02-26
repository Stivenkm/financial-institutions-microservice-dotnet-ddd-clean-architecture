using Intec.Banking.FinancialInstitutions.Domain;
using Intec.Banking.FinancialInstitutions.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Intec.Banking.FinancialInstitutions.Infrastructure.Configuration;

public sealed class FinancialInstitutionConfiguration
    : IEntityTypeConfiguration<FinancialInstitution>
{
    public void Configure(EntityTypeBuilder<FinancialInstitution> builder)
    {
        builder.ToTable("FinancialInstitutions");

        ConfigurePrimaryKey(builder);
        ConfigureBasicProperties(builder);
        ConfigureAudit(builder);
        ConfigureSoftDelete(builder);
        ConfigureVersion(builder);
        ConfigureCountry(builder);
        ConfigureTaxId(builder);
        ConfigureSwiftBic(builder);
        ConfigureLocalCodes(builder);
        ConfigureColombianDetails(builder);
    }

    // ────────────────────────────────────────────────────────────
    // PK
    // ────────────────────────────────────────────────────────────

    private static void ConfigurePrimaryKey(EntityTypeBuilder<FinancialInstitution> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(
                id => id.Value,
                value => FinancialInstitutionId.From(value))
            .ValueGeneratedNever();
    }

    // ────────────────────────────────────────────────────────────
    // BASIC PROPERTIES
    // ────────────────────────────────────────────────────────────

    private static void ConfigureBasicProperties(EntityTypeBuilder<FinancialInstitution> builder)
    {
        builder.Property(x => x.OfficialName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.TradeName)
            .HasMaxLength(200);
    }

    // ────────────────────────────────────────────────────────────
    // AUDIT — IHaveAudit + IHaveCreator
    // ────────────────────────────────────────────────────────────

    private static void ConfigureAudit(EntityTypeBuilder<FinancialInstitution> builder)
    {
        builder.Property(x => x.Created)
            .IsRequired()
            .HasColumnName("CreatedAt");

        builder.Property(x => x.CreatedBy)
            .HasColumnName("CreatedBy")
            .IsRequired(false);

        builder.Property(x => x.LastModified)
            .HasColumnName("UpdatedAt")
            .IsRequired(false);

        builder.Property(x => x.LastModifiedBy)
            .HasColumnName("UpdatedBy")
            .IsRequired(false);
    }

    // ────────────────────────────────────────────────────────────
    // SOFT DELETE — IHaveSoftDelete
    // Filtered global query to exclude deleted records automatically
    // ────────────────────────────────────────────────────────────

    private static void ConfigureSoftDelete(EntityTypeBuilder<FinancialInstitution> builder)
    {
        builder.Property(x => x.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.Deleted)
            .HasColumnName("DeletedAt")
            .IsRequired(false);

        builder.Property(x => x.DeletedBy)
            .HasColumnName("DeletedBy")
            .IsRequired(false);

        // Global query filter — deleted institutions are invisible to all queries
        // Use IgnoreQueryFilters() explicitly when needed (admin, audit)
        builder.HasQueryFilter(x => !x.IsDeleted);
    }

    // ────────────────────────────────────────────────────────────
    // OPTIMISTIC CONCURRENCY — IHaveAggregateVersion
    // OriginalVersion is the concurrency token.
    // EF generates: UPDATE ... WHERE Id = ? AND "Version" = ?
    // If no rows affected → DbUpdateConcurrencyException → 409 Conflict
    // ────────────────────────────────────────────────────────────

    private static void ConfigureVersion(EntityTypeBuilder<FinancialInstitution> builder)
    {
        builder.Property(x => x.OriginalVersion)
            .HasColumnName("Version")
            .IsRequired()
            .HasDefaultValue(0L)
            .IsConcurrencyToken();
    }

    // ────────────────────────────────────────────────────────────
    // VALUE OBJECTS — MAIN TABLE
    // ────────────────────────────────────────────────────────────

    private static void ConfigureCountry(EntityTypeBuilder<FinancialInstitution> builder)
    {
        builder.OwnsOne(x => x.Country, country =>
        {
            country.Property(c => c.Code)
                .HasColumnName("CountryCode")
                .IsRequired()
                .HasMaxLength(3);
        });
    }

    private static void ConfigureTaxId(EntityTypeBuilder<FinancialInstitution> builder)
    {
        builder.OwnsOne(x => x.TaxId, taxId =>
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
    }

    private static void ConfigureSwiftBic(EntityTypeBuilder<FinancialInstitution> builder)
    {
        builder.OwnsOne(x => x.SwiftBic, swift =>
        {
            swift.Property(s => s.Code)
                .HasColumnName("SwiftBic")
                .HasMaxLength(11);
        });
    }

    // ────────────────────────────────────────────────────────────
    // LOCAL CODES — SEPARATE TABLE
    // ────────────────────────────────────────────────────────────

    private static void ConfigureLocalCodes(EntityTypeBuilder<FinancialInstitution> builder)
    {
        builder.OwnsMany(x => x.LocalCodes, localCodes =>
        {
            localCodes.ToTable("FinancialInstitutionLocalCodes");

            localCodes.WithOwner()
                .HasForeignKey("FinancialInstitutionId");

            localCodes.Property<Guid>("FinancialInstitutionId")
                .HasColumnType("uuid");

            localCodes.Property<Guid>("Id")
                .HasColumnType("uuid");

            localCodes.HasKey("Id");

            localCodes.Property(x => x.Code)
                .IsRequired()
                .HasMaxLength(50);

            localCodes.Property(x => x.CodeType)
                .IsRequired()
                .HasMaxLength(20);

            localCodes.OwnsOne(x => x.Country, country =>
            {
                country.Property(c => c.Code)
                    .HasColumnName("CountryCode")
                    .IsRequired()
                    .HasMaxLength(3);
            });
        });
    }

    // ────────────────────────────────────────────────────────────
    // COLOMBIAN DETAILS — SEPARATE TABLE
    //
    // WORKAROUND — EF Core 9 Bug:
    // NavigationFixer throws IndexOutOfRangeException with OwnsOne
    // in separate table + value conversion on owner PK + nested owned types.
    // HasColumnType("uuid") on the FK provides enough metadata to partially
    // mitigate the issue. See DatabaseSeeder for full workaround.
    // ────────────────────────────────────────────────────────────

    private static void ConfigureColombianDetails(EntityTypeBuilder<FinancialInstitution> builder)
    {
        builder.OwnsOne(x => x.ColombianDetails, colombian =>
        {
            colombian.ToTable("FinancialInstitutionColombianDetails");

            colombian.WithOwner()
                .HasForeignKey("FinancialInstitutionId");

            colombian.Property<Guid>("FinancialInstitutionId")
                .HasColumnType("uuid");

            colombian.HasKey("FinancialInstitutionId");

            colombian.Property(c => c.SuperFinancialCode)
                .HasColumnName("SuperFinancialCode")
                .HasMaxLength(20)
                .IsRequired(false);

            colombian.OwnsOne(c => c.AchBankCode, ach =>
            {
                ach.Property(a => a.Code)
                    .HasColumnName("AchCode")
                    .HasMaxLength(50)
                    .IsRequired();

                ach.Property(a => a.CodeType)
                    .HasColumnName("AchCodeType")
                    .HasMaxLength(20)
                    .IsRequired();

                ach.OwnsOne(a => a.Country, country =>
                {
                    country.Property(c => c.Code)
                        .HasColumnName("AchCountryCode")
                        .HasMaxLength(3)
                        .IsRequired();
                });
            });
        });
    }
}