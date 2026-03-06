using FinancialInstitutions.IntegrationTests.Infrastructure;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace FinancialInstitutions.IntegrationTests.Endpoints;

[Collection(nameof(IntegrationTestCollection))]
public sealed class DeleteFinancialInstitutionTests : IntegrationTestBase
{
    public DeleteFinancialInstitutionTests(FinancialInstitutionsApiFactory factory)
        : base(factory) { }

    [Fact]
    public async Task DELETE_ExistingInstitution_Returns204()
    {
        var createResponse = await Client.PostAsJsonAsync(
            "/api/financial-institutions", TestData.ColombianBank());
        var id = await createResponse.Content.ReadFromJsonAsync<Guid>();

        var response = await Client.DeleteAsync($"/api/financial-institutions/{id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DELETE_SoftDeletes_NotReturnedInGet()
    {
        var createResponse = await Client.PostAsJsonAsync(
            "/api/financial-institutions", TestData.ColombianBank());
        var id = await createResponse.Content.ReadFromJsonAsync<Guid>();

        await Client.DeleteAsync($"/api/financial-institutions/{id}");

        var getResponse = await Client.GetAsync($"/api/financial-institutions/{id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DELETE_NonExistingId_Returns404()
    {
        var response = await Client.DeleteAsync(
            $"/api/financial-institutions/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DELETE_AlreadyDeleted_Returns404()
    {
        // El global query filter excluye soft-deleted, el handler lanza
        // NotFoundException → 404 en el segundo intento.
        var createResponse = await Client.PostAsJsonAsync(
            "/api/financial-institutions", TestData.ColombianBank());
        var id = await createResponse.Content.ReadFromJsonAsync<Guid>();

        await Client.DeleteAsync($"/api/financial-institutions/{id}");
        var response = await Client.DeleteAsync($"/api/financial-institutions/{id}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DELETE_WithoutTenantHeader_Returns401()
    {
        var clientWithoutTenant = Factory.CreateClient();

        var response = await clientWithoutTenant.DeleteAsync(
            $"/api/financial-institutions/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}