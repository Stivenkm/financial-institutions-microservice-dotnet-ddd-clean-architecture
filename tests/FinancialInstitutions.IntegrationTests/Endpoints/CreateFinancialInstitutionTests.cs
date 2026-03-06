using FinancialInstitutions.IntegrationTests.Infrastructure;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace FinancialInstitutions.IntegrationTests.Endpoints;

[Collection(nameof(IntegrationTestCollection))]
public sealed class CreateFinancialInstitutionTests : IntegrationTestBase
{
    public CreateFinancialInstitutionTests(FinancialInstitutionsApiFactory factory)
        : base(factory) { }

    // ── 201 Created ───────────────────────────────────────────────────────────

    [Fact]
    public async Task POST_ColombianBank_WithoutSwift_Returns201()
    {
        var cmd = TestData.ColombianBank();

        var response = await Client.PostAsJsonAsync("/api/financial-institutions", cmd);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var id = await response.Content.ReadFromJsonAsync<Guid>();
        id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task POST_InternationalBank_WithSwift_Returns201()
    {
        var cmd = TestData.InternationalBank();

        var response = await Client.PostAsJsonAsync("/api/financial-institutions", cmd);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task POST_Returns_LocationHeader_With_Id()
    {
        var cmd = TestData.ColombianBank();

        var response = await Client.PostAsJsonAsync("/api/financial-institutions", cmd);

        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().Should().Contain("/api/financial-institutions/");
    }

    // ── 400 Bad Request — Validation ─────────────────────────────────────────

    [Fact]
    public async Task POST_EmptyOfficialName_Returns400()
    {
        var cmd = TestData.ColombianBank(officialName: "");

        var response = await Client.PostAsJsonAsync("/api/financial-institutions", cmd);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task POST_InvalidCountryCode_Returns400()
    {
        var cmd = new
        {
            OfficialName = "Banco Test",
            CountryCode = "X",   // 1 char — invalid
            TaxIdValue = "900123456-1",
            SwiftBicCode = (string?)null
        };

        var response = await Client.PostAsJsonAsync("/api/financial-institutions", cmd);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task POST_InvalidSwiftBic_Returns400()
    {
        var cmd = new
        {
            OfficialName = "Bank Test",
            CountryCode = "US",
            TaxIdValue = "12-3456789",
            SwiftBicCode = "INVALID123"  // 10 chars — invalid format
        };

        var response = await Client.PostAsJsonAsync("/api/financial-institutions", cmd);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ── 400 Bad Request — Domain rules ────────────────────────────────────────

    [Fact]
    public async Task POST_NonColombianWithoutSwift_Returns400()
    {
        var cmd = new
        {
            OfficialName = "Bank of America",
            CountryCode = "US",
            TaxIdValue = "12-3456789",
            SwiftBicCode = (string?)null  // required for non-Colombian
        };

        var response = await Client.PostAsJsonAsync("/api/financial-institutions", cmd);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ── 401 Unauthorized ─────────────────────────────────────────────────────

    [Fact]
    public async Task POST_WithoutTenantHeader_Returns401()
    {
        var clientWithoutTenant = Factory.CreateClient();
        var cmd = TestData.ColombianBank();

        var response = await clientWithoutTenant.PostAsJsonAsync(
            "/api/financial-institutions", cmd);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── 409 Conflict — Unique constraint ─────────────────────────────────────

    [Fact]
    public async Task POST_DuplicateSwiftBic_Returns409()
    {
        var swiftBic = TestData.GenerateSwiftBic();
        var first = TestData.InternationalBank(swiftBic: swiftBic);
        var second = TestData.InternationalBank(swiftBic: swiftBic); // same SwiftBic

        await Client.PostAsJsonAsync("/api/financial-institutions", first);
        var response = await Client.PostAsJsonAsync("/api/financial-institutions", second);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task POST_AfterSoftDelete_SameSwiftBic_Returns201()
    {
        // Create → Delete → Create again with same SwiftBic
        var swiftBic = TestData.GenerateSwiftBic();
        var cmd = TestData.InternationalBank(swiftBic: swiftBic);

        var createResponse = await Client.PostAsJsonAsync("/api/financial-institutions", cmd);
        var id = await createResponse.Content.ReadFromJsonAsync<Guid>();

        await Client.DeleteAsync($"/api/financial-institutions/{id}");

        var recreateResponse = await Client.PostAsJsonAsync("/api/financial-institutions", cmd);

        recreateResponse.StatusCode.Should().Be(HttpStatusCode.Created);
    }
}