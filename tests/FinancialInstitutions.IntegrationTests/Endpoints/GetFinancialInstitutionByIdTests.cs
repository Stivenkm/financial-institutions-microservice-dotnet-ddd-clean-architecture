using FinancialInstitutions.IntegrationTests.Infrastructure;
using FluentAssertions;
using Intec.Banking.FinancialInstitutions.Application.DTOs;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace FinancialInstitutions.IntegrationTests.Endpoints;

[Collection(nameof(IntegrationTestCollection))]
public sealed class GetFinancialInstitutionByIdTests : IntegrationTestBase
{
    public GetFinancialInstitutionByIdTests(FinancialInstitutionsApiFactory factory)
        : base(factory) { }

    [Fact]
    public async Task GET_ById_ExistingId_Returns200WithDto()
    {
        var cmd = TestData.ColombianBank(officialName: "Banco Test GET");
        var createResponse = await Client.PostAsJsonAsync("/api/financial-institutions", cmd);
        var id = await createResponse.Content.ReadFromJsonAsync<Guid>();

        var response = await Client.GetAsync($"/api/financial-institutions/{id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var dto = await response.Content.ReadFromJsonAsync<FinancialInstitutionDto>();
        dto.Should().NotBeNull();
        dto!.Id.Should().Be(id);
        dto.OfficialName.Should().Be("Banco Test GET");
    }

    [Fact]
    public async Task GET_ById_NonExistingId_Returns404()
    {
        var response = await Client.GetAsync(
            $"/api/financial-institutions/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GET_ById_DeletedInstitution_Returns404()
    {
        var createResponse = await Client.PostAsJsonAsync(
            "/api/financial-institutions", TestData.ColombianBank());
        var id = await createResponse.Content.ReadFromJsonAsync<Guid>();

        await Client.DeleteAsync($"/api/financial-institutions/{id}");

        var response = await Client.GetAsync($"/api/financial-institutions/{id}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GET_ById_WithoutTenantHeader_Returns401()
    {
        var clientWithoutTenant = Factory.CreateClient();

        var response = await clientWithoutTenant.GetAsync(
            $"/api/financial-institutions/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}