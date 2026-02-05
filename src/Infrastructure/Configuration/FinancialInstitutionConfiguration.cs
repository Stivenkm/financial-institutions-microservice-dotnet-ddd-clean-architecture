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

        // -------- PK --------
        builder.HasKey(x => x.Id);

        // Configure FinancialInstitutionId as value conversion
        builder.Property(x => x.Id)
            .HasConversion(
                id => id.Value,
                value => FinancialInstitutionId.From(value))
            .ValueGeneratedNever();

        // -------- Basic fields --------
        builder.Property(x => x.OfficialName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.TradeName)
            .HasMaxLength(200);

        // -------- Country --------
        builder.OwnsOne(x => x.Country, country =>
        {
            country.Property(c => c.Code)
                .HasColumnName("CountryCode")
                .IsRequired()
                .HasMaxLength(3);
        });

        // -------- TaxId --------
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

        // -------- SwiftBic --------
        builder.OwnsOne(x => x.SwiftBic, swift =>
        {
            swift.Property(s => s.Code)
                .HasColumnName("SwiftBic")
                .HasMaxLength(11);
        });

        // -------- LocalCodes (collection of VO) --------
        builder.OwnsMany(x => x.LocalCodes, localCodes =>
        {
            localCodes.ToTable("FinancialInstitutionLocalCodes");

            localCodes.WithOwner()
                .HasForeignKey("FinancialInstitutionId");

            localCodes.Property<Guid>("Id");
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

        // -------- Colombian Details --------
        builder.OwnsOne(x => x.ColombianDetails, colombian =>
        {
            colombian.Property(c => c.SuperFinancialCode)
                .HasColumnName("SuperFinancialCode")
                .HasMaxLength(20)
                .IsRequired();

            colombian.OwnsOne(c => c.AchBankCode, ach =>
            {
                ach.Property(a => a.Code)
                    .HasColumnName("AchCode")
                    .HasMaxLength(50);

                ach.Property(a => a.CodeType)
                    .HasColumnName("AchCodeType")
                    .HasMaxLength(20);

                ach.OwnsOne(a => a.Country, country =>
                {
                    country.Property(c => c.Code)
                        .HasColumnName("AchCountryCode")
                        .HasMaxLength(3);
                });
            });
        });
    }
}