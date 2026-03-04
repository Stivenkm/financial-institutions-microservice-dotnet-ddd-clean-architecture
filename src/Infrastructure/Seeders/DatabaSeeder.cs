using Intec.Banking.FinancialInstitutions.Domain;
using Intec.Banking.FinancialInstitutions.Domain.ValueObjects;
using Intec.Banking.FinancialInstitutions.Infrastructure.Interceptors;
using Intec.Banking.FinancialInstitutions.Infrastructure.Services;
using Intec.Banking.FinancialInstitutions.Primitives;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Intec.Banking.FinancialInstitutions.Infrastructure.Seeders;

/// <summary>
/// Seeds reference data for development environment.
///
/// TENANT STRATEGY:
/// Seed data belongs to a fixed system tenant (SystemTenantId).
/// The seeder creates its own DbContext with a SeederTenantService
/// that provides the system TenantId without requiring HTTP context.
/// This bypasses the DI-registered TenantService which requires HttpContext.
///
/// PERSISTENCE STRATEGY:
/// Phase 1 — Institutions + LocalCodes via EF (full domain validation)
/// Phase 2 — ColombianDetails via raw SQL (EF Core 9 bug workaround)
///
/// WORKAROUND — EF Core 9 Bug (Npgsql 9.0.3):
/// NavigationFixer throws IndexOutOfRangeException when persisting OwnsOne
/// in a separate table when the owner uses value conversion on PK and the
/// owned type contains nested owned types (AchBankCode inside ColombianDetails).
/// Remove when EF Core fixes the bug.
/// </summary>
public static class DatabaseSeeder
{
    /// <summary>
    /// Fixed system tenant for seed data.
    /// Exposed as a constant so integration tests can use the same tenant.
    /// </summary>
    public static readonly Guid SystemTenantId = new("00000000-0000-0000-0000-000000000001");

    public static async Task SeedAsync(IServiceProvider serviceProvider, ILogger? logger = null)
    {
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();
        var connectionString = configuration.GetConnectionString("Default")!;

        // Check if already seeded — use IgnoreQueryFilters to check across all tenants
        // The standard DbContext uses HasQueryFilter so AnyAsync() would always
        // return false without a valid tenant — IgnoreQueryFilters bypasses this
        await using var checkDb = CreateSeederDbContext(connectionString);
        if (await checkDb.FinancialInstitutions.IgnoreQueryFilters().AnyAsync())
        {
            logger?.LogInformation("Database already seeded. Skipping.");
            return;
        }

        logger?.LogInformation("Seeding financial institutions with SystemTenantId: {TenantId}", SystemTenantId);

        var (colombianInstitutions, colombianDetails) = BuildColombianInstitutions();
        var internationalInstitutions = BuildInternationalInstitutions();

        // ── PHASE 1: Persist Colombian institutions (without ColombianDetails) ──
        // Separate DbContext per aggregate to avoid change tracker conflicts
        foreach (var institution in colombianInstitutions)
        {
            await using var db = CreateSeederDbContext(connectionString);
            db.FinancialInstitutions.Add(institution);
            await db.SaveChangesAsync();
            logger?.LogDebug("Seeded: [{Country}] {Name}", institution.Country.Code, institution.OfficialName);
        }

        // ── PHASE 2: Persist ColombianDetails via raw SQL (EF Core 9 workaround) ──
        await using var detailsDb = CreateSeederDbContext(connectionString);
        await PersistColombianDetailsAsync(detailsDb, colombianDetails, logger);

        // ── PHASE 3: Persist international institutions ──
        foreach (var institution in internationalInstitutions)
        {
            await using var db = CreateSeederDbContext(connectionString);
            db.FinancialInstitutions.Add(institution);
            await db.SaveChangesAsync();
            logger?.LogDebug("Seeded: [{Country}] {Name}", institution.Country.Code, institution.OfficialName);
        }

        var total = colombianInstitutions.Count + internationalInstitutions.Count;
        logger?.LogInformation("Seeded {Count} financial institutions successfully.", total);
    }

    // ────────────────────────────────────────────────────────────────────────────
    // SEEDER DB CONTEXT
    // Creates a DbContext with SeederTenantService — no HttpContext required.
    // AuditInterceptor uses SystemTenantId for all seeded records.
    // ────────────────────────────────────────────────────────────────────────────

    private static FinancialInstitutionsDbContext CreateSeederDbContext(string connectionString)
    {
        var tenantService = new SeederTenantService(SystemTenantId);
        var currentUserService = new SeederCurrentUserService();
        var interceptor = new AuditInterceptor(currentUserService, tenantService);

        var options = new DbContextOptionsBuilder<FinancialInstitutionsDbContext>()
            .UseNpgsql(connectionString)
            .AddInterceptors(interceptor)
            .Options;

        return new FinancialInstitutionsDbContext(options, tenantService);
    }

    // ────────────────────────────────────────────────────────────────────────────
    // DOMAIN CONSTRUCTION
    // ────────────────────────────────────────────────────────────────────────────

    private static (List<FinancialInstitution> Institutions, List<ColombianDetailsSeed> Details)
        BuildColombianInstitutions()
    {
        var colombia = CountryCode.Colombia;
        var institutions = new List<FinancialInstitution>();
        var details = new List<ColombianDetailsSeed>();

        void Add(
            string officialName,
            string tradeName,
            string taxId,
            string? swiftBic,
            string achCode,
            string? superFinancialCode)
        {
            var institution = FinancialInstitution.CreateBank(
                officialName: officialName,
                tradeName: tradeName,
                country: colombia,
                taxId: TaxId.Create(taxId, colombia),
                swiftBic: swiftBic is not null ? SwiftBic.Create(swiftBic) : null);

            institution.AddLocalCode(LocalBankCode.CreateAchCode(achCode, colombia));

            details.Add(new ColombianDetailsSeed(
                InstitutionId: institution.Id.Value,
                AchCode: achCode,
                SuperFinancialCode: superFinancialCode));

            institutions.Add(institution);
        }

        Add("Bancolombia S.A.", "Bancolombia", "890903938-8", "COLOCOBM", "007", "007");
        Add("Banco de Bogotá S.A.", "Banco de Bogotá", "860002964-4", "BBOGCOBB", "001", "001");
        Add("Banco Davivienda S.A.", "Davivienda", "860034313-7", "CAFICOBB", "051", "051");
        Add("BBVA Colombia S.A.", "BBVA", "860003020-1", "BBVACOBB", "013", "013");
        Add("Banco Popular S.A.", "Banco Popular", "860007738-2", null, "002", "002");
        Add("Banco Agrario de Colombia S.A.", "Banco Agrario", "800037800-8", null, "040", "040");
        Add("Nequi S.A.S.", "Nequi", "900200960-1", null, "507", null);

        return (institutions, details);
    }

    private static List<FinancialInstitution> BuildInternationalInstitutions()
    {
        var us = CountryCode.UnitedStates;

        FinancialInstitution Create(
            string officialName,
            string tradeName,
            string taxId,
            string swiftBic,
            string routingNumber)
        {
            var institution = FinancialInstitution.CreateBank(
                officialName: officialName,
                tradeName: tradeName,
                country: us,
                taxId: TaxId.Create(taxId, us),
                swiftBic: SwiftBic.Create(swiftBic));

            institution.AddLocalCode(LocalBankCode.CreateRoutingNumber(routingNumber, us));

            return institution;
        }

        return
        [
            Create("JPMorgan Chase Bank, N.A.", "Chase",           "13-4994650", "CHASUS33", "021000021"),
            Create("Bank of America, N.A.",     "Bank of America", "56-0906609", "BOFAUS3N", "026009593"),
            Create("Citibank, N.A.",            "Citi",            "13-5266470", "CITIUS33", "021000089"),
        ];
    }

    // ────────────────────────────────────────────────────────────────────────────
    // PERSISTENCE — EF CORE 9 WORKAROUND
    // ────────────────────────────────────────────────────────────────────────────

    private static async Task PersistColombianDetailsAsync(
        FinancialInstitutionsDbContext db,
        List<ColombianDetailsSeed> details,
        ILogger? logger)
    {
        foreach (var detail in details)
        {
            if (detail.SuperFinancialCode is null)
            {
                await db.Database.ExecuteSqlRawAsync(@"
                    INSERT INTO ""FinancialInstitutionColombianDetails""
                        (""FinancialInstitutionId"", ""AchCode"", ""AchCodeType"", ""AchCountryCode"")
                    VALUES ({0}, {1}, {2}, {3})",
                    detail.InstitutionId, detail.AchCode, "ACH", "CO");
            }
            else
            {
                await db.Database.ExecuteSqlRawAsync(@"
                    INSERT INTO ""FinancialInstitutionColombianDetails""
                        (""FinancialInstitutionId"", ""AchCode"", ""AchCodeType"", ""AchCountryCode"", ""SuperFinancialCode"")
                    VALUES ({0}, {1}, {2}, {3}, {4})",
                    detail.InstitutionId, detail.AchCode, "ACH", "CO", detail.SuperFinancialCode);
            }

            logger?.LogDebug("Persisted ColombianDetails for institution: {Id}", detail.InstitutionId);
        }
    }

    private sealed record ColombianDetailsSeed(
        Guid InstitutionId,
        string AchCode,
        string? SuperFinancialCode);
}

// ────────────────────────────────────────────────────────────────────────────
// SEEDER SERVICES
// Minimal implementations for seeding — no HTTP context required.
// Internal — only used by DatabaseSeeder.
// ────────────────────────────────────────────────────────────────────────────

internal sealed class SeederTenantService : ITenantService
{
    private readonly Guid _tenantId;
    public SeederTenantService(Guid tenantId) => _tenantId = tenantId;
    public Guid? TenantId => _tenantId;
    public Guid GetRequiredTenantId() => _tenantId;
}

internal sealed class SeederCurrentUserService : ICurrentUserService
{
    public int? UserId => 1; // System operation — no user
}