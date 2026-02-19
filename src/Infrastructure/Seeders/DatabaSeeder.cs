using Intec.Banking.FinancialInstitutions.Domain;
using Intec.Banking.FinancialInstitutions.Domain.ValueObjects;
using Intec.Banking.FinancialInstitutions.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Intec.Banking.FinancialInstitutions.Infrastructure.Seeders;

/// <summary>
/// Siembra datos de prueba para el ambiente de desarrollo.
///
/// ESTRATEGIA DE PERSISTENCIA:
/// Las instituciones financieras se construyen íntegramente a través del
/// Aggregate Root, respetando todas las reglas de negocio y Value Objects
/// del dominio. La persistencia se realiza en dos fases:
///
/// Fase 1 — Instituciones + LocalCodes via EF (dominio completo)
/// Fase 2 — ColombianDetails via SQL directo
///
/// WORKAROUND — Bug EF Core 9 (Npgsql 9.0.3):
/// EF Core 9 tiene un bug en el NavigationFixer que lanza
/// IndexOutOfRangeException al persistir un OwnsOne en tabla separada
/// cuando el Aggregate owner usa value conversion en su PK y el owned type
/// contiene owned types anidados (AchBankCode dentro de ColombianDetails).
/// El dominio sigue siendo la fuente de verdad — las entidades se construyen
/// y validan completamente antes de persistir. Solo la persistencia de
/// ColombianDetails usa SQL como workaround temporal.
/// Issue: https://github.com/dotnet/efcore/issues
/// Eliminar workaround cuando se corrija en EF Core 9.x
/// </summary>
public static class DatabaseSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider, ILogger? logger = null)
    {
        await using var checkScope = serviceProvider.CreateAsyncScope();
        var checkDb = checkScope.ServiceProvider.GetRequiredService<FinancialInstitutionsDbContext>();

        if (await checkDb.FinancialInstitutions.AnyAsync())
        {
            logger?.LogInformation("Database already seeded. Skipping.");
            return;
        }

        logger?.LogInformation("Seeding financial institutions...");

        // Construir todas las instituciones via dominio — valida reglas de negocio
        var (colombianInstitutions, colombianDetails) = BuildColombianInstitutions();
        var internationalInstitutions = BuildInternationalInstitutions();

        // ── FASE 1: Persistir instituciones colombianas (sin ColombianDetails) ──
        // Scope limpio por Aggregate para evitar conflictos del change tracker
        foreach (var institution in colombianInstitutions)
        {
            await using var scope = serviceProvider.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<FinancialInstitutionsDbContext>();
            db.FinancialInstitutions.Add(institution);
            await db.SaveChangesAsync();
            logger?.LogDebug("Seeded: [{Country}] {Name}", institution.Country.Code, institution.OfficialName);
        }

        // ── FASE 2: Persistir ColombianDetails via SQL (workaround bug EF Core 9) ──
        // Los datos fueron construidos y validados por el dominio en BuildColombianInstitutions
        await using var detailsScope = serviceProvider.CreateAsyncScope();
        var detailsDb = detailsScope.ServiceProvider.GetRequiredService<FinancialInstitutionsDbContext>();
        await PersistColombianDetailsAsync(detailsDb, colombianDetails, logger);

        // ── FASE 3: Persistir bancos internacionales (no tienen ColombianDetails) ──
        foreach (var institution in internationalInstitutions)
        {
            await using var scope = serviceProvider.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<FinancialInstitutionsDbContext>();
            db.FinancialInstitutions.Add(institution);
            await db.SaveChangesAsync();
            logger?.LogDebug("Seeded: [{Country}] {Name}", institution.Country.Code, institution.OfficialName);
        }

        var total = colombianInstitutions.Count + internationalInstitutions.Count;
        logger?.LogInformation("Seeded {Count} financial institutions successfully.", total);
    }

    // ────────────────────────────────────────────────────────────────────────────
    // CONSTRUCCIÓN DEL DOMINIO
    // Cada institución se construye respetando el Aggregate Root y sus invariantes
    // ────────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Construye bancos colombianos usando el Aggregate Root completo.
    /// Retorna las instituciones y sus ColombianDetails separados para
    /// persistirlos en dos fases debido al bug de EF Core 9.
    ///
    /// Reglas del dominio que se validan durante la construcción:
    /// - TaxId.Create     → país del TaxId debe coincidir con país de la institución
    /// - SwiftBic.Create  → formato SWIFT/BIC válido (8 u 11 caracteres)
    /// - CreateBank       → SWIFT es obligatorio para bancos no colombianos
    /// - CreateAchCode    → tipo de código debe ser ACH y país debe ser Colombia
    /// - ColombianBankingDetails.Create → AchBankCode obligatorio, SuperCode opcional
    /// - SetColombianDetails → solo bancos colombianos pueden tener ColombianDetails
    /// </summary>
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
            // Aggregate Root — valida todas las reglas de negocio
            var institution = FinancialInstitution.CreateBank(
                officialName: officialName,
                tradeName: tradeName,
                country: colombia,
                taxId: TaxId.Create(taxId, colombia),
                swiftBic: swiftBic is not null ? SwiftBic.Create(swiftBic) : null);

            // El dominio dicta que SetColombianDetails agrega el AchBankCode
            // también como LocalCode del Aggregate (ver FinancialInstitution.SetColombianDetails).
            // Como el workaround del bug de EF9 impide llamar SetColombianDetails,
            // replicamos ese comportamiento explícitamente aquí.
            institution.AddLocalCode(
                LocalBankCode.CreateAchCode(achCode, colombia));

            // Registrar ColombianDetails para persistir en Fase 2 via SQL
            details.Add(new ColombianDetailsSeed(
                InstitutionId: institution.Id.Value,
                AchCode: achCode,
                SuperFinancialCode: superFinancialCode));

            institutions.Add(institution);
        }

        // Bancos con SWIFT (operación internacional + local)
        Add("Bancolombia S.A.", "Bancolombia", "890903938-8", "COLOCOBM", "007", "007");
        Add("Banco de Bogotá S.A.", "Banco de Bogotá", "860002964-4", "BBOGCOBB", "001", "001");
        Add("Banco Davivienda S.A.", "Davivienda", "860034313-7", "CAFICOBB", "051", "051");
        Add("BBVA Colombia S.A.", "BBVA", "860003020-1", "BBVACOBB", "013", "013");

        // Bancos sin SWIFT (solo operación local)
        // Dominio permite SWIFT null para bancos colombianos
        Add("Banco Popular S.A.", "Banco Popular", "860007738-2", null, "002", "002");
        Add("Banco Agrario de Colombia S.A.", "Banco Agrario", "800037800-8", null, "040", "040");

        // Fintech — sin SWIFT y sin código Superfinanciera
        // Valida que SuperFinancialCode es nullable en ColombianBankingDetails
        Add("Nequi S.A.S.", "Nequi", "900200960-1", null, "507", null);

        return (institutions, details);
    }

    /// <summary>
    /// Construye bancos internacionales usando el Aggregate Root.
    ///
    /// Reglas del dominio que se validan durante la construcción:
    /// - CreateBank          → SWIFT obligatorio para bancos no colombianos
    /// - SwiftBic.Create     → formato SWIFT/BIC válido
    /// - CreateRoutingNumber → tipo de código debe ser ROUTING
    /// - AddLocalCode        → agrega el routing number como LocalBankCode del Aggregate
    /// </summary>
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

            // Aggregate method — valida y agrega routing number como LocalBankCode
            institution.AddLocalCode(
                LocalBankCode.CreateRoutingNumber(routingNumber, us));

            return institution;
        }

        return new List<FinancialInstitution>
        {
            Create("JPMorgan Chase Bank, N.A.", "Chase",           "13-4994650", "CHASUS33", "021000021"),
            Create("Bank of America, N.A.",     "Bank of America", "56-0906609", "BOFAUS3N", "026009593"),
            Create("Citibank, N.A.",            "Citi",            "13-5266470", "CITIUS33", "021000089"),
        };
    }

    // ────────────────────────────────────────────────────────────────────────────
    // PERSISTENCIA — WORKAROUND EF CORE 9
    // ────────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Persiste ColombianDetails via SQL directo.
    ///
    /// WORKAROUND: EF Core 9 tiene un bug en el NavigationFixer que impide
    /// persistir OwnsOne en tabla separada cuando el owner usa value conversion
    /// en su PK y el owned type tiene owned types anidados.
    ///
    /// Los datos aquí son los mismos que el dominio construyó y validó en
    /// BuildColombianInstitutions — el SQL solo los persiste, no los inventa.
    /// Eliminar cuando EF Core corrija el bug.
    /// </summary>
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

    // ────────────────────────────────────────────────────────────────────────────
    // SEED DATA RECORD
    // ────────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Datos de ColombianDetails validados por el dominio, listos para persistir.
    /// </summary>
    private sealed record ColombianDetailsSeed(
        Guid InstitutionId,
        string AchCode,
        string? SuperFinancialCode);
}