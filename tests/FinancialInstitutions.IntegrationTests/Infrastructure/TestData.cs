using Bogus;
using Intec.Banking.FinancialInstitutions.Application.Features.FinancialInstitutions.CreateFinancialInstitution;

namespace FinancialInstitutions.IntegrationTests.Infrastructure;

/// <summary>
/// Bogus-based builders for test data.
/// Generates realistic but randomized data for each test run.
/// </summary>
public static class TestData
{
    private static readonly Faker Faker = new("es");

    // ── Valid Colombian institution ───────────────────────────────────────────

    public static CreateFinancialInstitutionCommand ColombianBank(
        string? officialName = null,
        string? swiftBic = null) =>
        new(
            OfficialName: officialName ?? $"Banco {Faker.Company.CompanyName()}",
            TradeName: Faker.Company.CompanySuffix(),
            CountryCode: "CO",
            TaxIdValue: $"{Faker.Random.Number(100_000_000, 999_999_999)}-{Faker.Random.Number(1, 9)}",
            SwiftBicCode: swiftBic
        );

    // ── Valid international institution ───────────────────────────────────────

    public static CreateFinancialInstitutionCommand InternationalBank(
        string? officialName = null,
        string? swiftBic = null) =>
        new(
            OfficialName: officialName ?? Faker.Company.CompanyName(),
            TradeName: Faker.Company.CompanySuffix(),
            CountryCode: "US",
            TaxIdValue: $"{Faker.Random.Number(10, 99)}-{Faker.Random.Number(1_000_000, 9_999_999)}",
            SwiftBicCode: swiftBic ?? GenerateSwiftBic()
        );

    // ── Known fixed institutions for deterministic tests ─────────────────────

    public static CreateFinancialInstitutionCommand BancoColombia =>
        new("Bancolombia", "Bancolombia S.A.", "CO", "890903938-8", null);

    public static CreateFinancialInstitutionCommand Citibank =>
        new("Citibank N.A.", "Citi", "US", "13-5266470", "CITIUS33XXX");

    // ── Helpers ───────────────────────────────────────────────────────────────

    public static string GenerateSwiftBic()
    {
        var alphanumeric = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var faker = new Faker();

        var bank = new string(Enumerable.Range(0, 4).Select(_ => faker.Random.Char('A', 'Z')).ToArray());
        var country = new string(Enumerable.Range(0, 2).Select(_ => faker.Random.Char('A', 'Z')).ToArray());
        var location = new string(Enumerable.Range(0, 2).Select(_ => alphanumeric[faker.Random.Number(0, alphanumeric.Length - 1)]).ToArray());

        return $"{bank}{country}{location}";
    }

    public static string SystemTenantId => "00000000-0000-0000-0000-000000000001";
}