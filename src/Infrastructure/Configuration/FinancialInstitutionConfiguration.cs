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
        ConfigureTenant(builder);
        ConfigureIndexes(builder);
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
    // NOTE: HasQueryFilter is defined in DbContext.OnModelCreating
    // combining both soft delete and tenant isolation in a single filter.
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
    }

    // ────────────────────────────────────────────────────────────
    // OPTIMISTIC CONCURRENCY — IHaveAggregateVersion
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
    // MULTI-TENANCY — IHaveTenant
    // HasQueryFilter defined in DbContext combining tenant + soft delete.
    // Use IgnoreQueryFilters() explicitly for cross-tenant admin operations.
    // ────────────────────────────────────────────────────────────

    private static void ConfigureTenant(EntityTypeBuilder<FinancialInstitution> builder)
    {
        builder.Property(x => x.TenantId)
            .IsRequired()
            .HasColumnName("TenantId");
    }

    // ────────────────────────────────────────────────────────────
    // INDEXES
    // Direct CLR property indexes only.
    // Owned type indexes defined inside their OwnsOne/HasConversion configs.
    // ────────────────────────────────────────────────────────────

    private static void ConfigureIndexes(EntityTypeBuilder<FinancialInstitution> builder)
    {
        // TenantId — every query filters by tenant via HasQueryFilter
        builder.HasIndex(x => x.TenantId)
            .HasDatabaseName("IX_FinancialInstitutions_TenantId");
    }

    // ────────────────────────────────────────────────────────────
    // COUNTRY — HasConversion (single primitive property)
    // CountryCode has only one property (Code) → HasConversion is correct.
    // Eliminates owned type tracking, resolves duplicate tracking warnings.
    // ────────────────────────────────────────────────────────────

    private static void ConfigureCountry(EntityTypeBuilder<FinancialInstitution> builder)
    {
        builder.Property(x => x.Country)
            .HasConversion(
                c => c.Code,
                code => CountryCode.Create(code))
            .HasColumnName("CountryCode")
            .IsRequired()
            .HasMaxLength(3);
    }

    // ────────────────────────────────────────────────────────────
    // TAX ID — OwnsOne (Value + Country)
    // TaxId has two properties → OwnsOne is correct.
    // Country inside TaxId uses HasConversion → no nested OwnsOne.
    // Unique constraint: same TaxId value can exist in different countries.
    // ────────────────────────────────────────────────────────────

    private static void ConfigureTaxId(EntityTypeBuilder<FinancialInstitution> builder)
    {
        builder.OwnsOne(x => x.TaxId, taxId =>
        {
            taxId.Property(t => t.Value)
                .HasColumnName("TaxIdValue")
                .IsRequired()
                .HasMaxLength(50);

            // CountryCode has single primitive → HasConversion inside OwnsOne
            taxId.Property(t => t.Country)
                .HasConversion(
                    c => c.Code,
                    code => CountryCode.Create(code))
                .HasColumnName("TaxIdCountryCode")
                .IsRequired()
                .HasMaxLength(3);

            // Composite unique: same TaxId can exist in different countries
            taxId.HasIndex(t => new { t.Value, t.Country })
                .IsUnique()
                .HasDatabaseName("UX_FinancialInstitutions_TaxId");
        });
    }

    // ────────────────────────────────────────────────────────────
    // SWIFT BIC — HasConversion (single primitive property)
    // SwiftBic has only one property (Code) → HasConversion is correct.
    // Nullable: Colombian banks may not have SWIFT.
    // Unique: a SWIFT/BIC identifies one bank globally.
    // ────────────────────────────────────────────────────────────

    private static void ConfigureSwiftBic(EntityTypeBuilder<FinancialInstitution> builder)
    {
        builder.Property(x => x.SwiftBic)
            .HasConversion(
                s => s != null ? s.Code : null,
                code => code != null ? SwiftBic.Create(code) : null)
            .HasColumnName("SwiftBic")
            .IsRequired(false)
            .HasMaxLength(11);

        // Unique — PostgreSQL natively allows multiple NULLs in unique indexes
        // No HasFilter needed: NULL != NULL in SQL standard
        builder.HasIndex(x => x.SwiftBic)
            .IsUnique()
            .HasDatabaseName("UX_FinancialInstitutions_SwiftBic");
    }

    // ────────────────────────────────────────────────────────────
    // LOCAL CODES — SEPARATE TABLE
    // Country inside LocalBankCode uses HasConversion → no nested OwnsOne.
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

            // CountryCode has single primitive → HasConversion, no nested OwnsOne
            localCodes.Property(x => x.Country)
                .HasConversion(
                    c => c.Code,
                    code => CountryCode.Create(code))
                .HasColumnName("CountryCode")
                .IsRequired()
                .HasMaxLength(3);
        });
    }

    // ────────────────────────────────────────────────────────────
    // COLOMBIAN DETAILS — SEPARATE TABLE
    //
    // WORKAROUND — EF Core 9 Bug:
    // NavigationFixer throws IndexOutOfRangeException with OwnsOne
    // in separate table + value conversion on owner PK + nested owned types.
    // HasConversion on AchBankCode.Country eliminates one level of nesting
    // and reduces (but does not fully eliminate) the bug surface.
    // See DatabaseSeeder for persistence workaround.
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

                // CountryCode has single primitive → HasConversion, no nested OwnsOne
                ach.Property(a => a.Country)
                    .HasConversion(
                        c => c.Code,
                        code => CountryCode.Create(code))
                    .HasColumnName("AchCountryCode")
                    .IsRequired()
                    .HasMaxLength(3);
            });
        });
    }
}