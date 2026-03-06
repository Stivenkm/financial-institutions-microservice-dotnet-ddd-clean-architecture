using FinancialInstitutions.IntegrationTests.Infrastructure;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace FinancialInstitutions.IntegrationTests.Endpoints;

[Collection(nameof(IntegrationTestCollection))]
public sealed class UpdateFinancialInstitutionTests : IntegrationTestBase
{
    public UpdateFinancialInstitutionTests(FinancialInstitutionsApiFactory factory)
        : base(factory) { }

    // El ValidationFilter corre ANTES de que el endpoint haga `command with { Id = routeId }`,
    // por lo que debemos incluir un Id válido en el body. El endpoint lo sobreescribe.
    private static object BuildUpdateBody(Guid id, string officialName, long? originalVersion = 0) =>
        new
        {
            Id = new { Value = id },
            OfficialName = officialName,
            TradeName = (string?)null,
            CountryCode = "CO",
            TaxIdValue = "900123456-1",
            SwiftBicCode = (string?)null,
            OriginalVersion = originalVersion
        };

    [Fact]
    public async Task PUT_ValidCommand_Returns204()
    {
        var createResponse = await Client.PostAsJsonAsync(
            "/api/financial-institutions", TestData.ColombianBank());
        var id = await createResponse.Content.ReadFromJsonAsync<Guid>();

        var response = await Client.PutAsJsonAsync(
            $"/api/financial-institutions/{id}",
            BuildUpdateBody(id, "Nombre Actualizado"));

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task PUT_UpdatedName_ReflectsInGet()
    {
        var createResponse = await Client.PostAsJsonAsync(
            "/api/financial-institutions", TestData.ColombianBank());
        var id = await createResponse.Content.ReadFromJsonAsync<Guid>();

        await Client.PutAsJsonAsync(
            $"/api/financial-institutions/{id}",
            BuildUpdateBody(id, "Nombre Nuevo Verificado"));

        var getResponse = await Client.GetAsync($"/api/financial-institutions/{id}");
        var bodyStr = await getResponse.Content.ReadAsStringAsync();

        bodyStr.Should().Contain("Nombre Nuevo Verificado");
    }

    [Fact]
    public async Task PUT_NonExistingId_Returns404()
    {
        var nonExisting = Guid.NewGuid();

        var response = await Client.PutAsJsonAsync(
            $"/api/financial-institutions/{nonExisting}",
            BuildUpdateBody(nonExisting, "Nombre"));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task PUT_WrongVersion_Returns409()
    {
        var createResponse = await Client.PostAsJsonAsync(
            "/api/financial-institutions", TestData.ColombianBank());
        var id = await createResponse.Content.ReadFromJsonAsync<Guid>();

        var response = await Client.PutAsJsonAsync(
            $"/api/financial-institutions/{id}",
            BuildUpdateBody(id, "Nombre", originalVersion: 99));

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task PUT_EmptyOfficialName_Returns400()
    {
        var createResponse = await Client.PostAsJsonAsync(
            "/api/financial-institutions", TestData.ColombianBank());
        var id = await createResponse.Content.ReadFromJsonAsync<Guid>();

        var response = await Client.PutAsJsonAsync(
            $"/api/financial-institutions/{id}",
            BuildUpdateBody(id, ""));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}