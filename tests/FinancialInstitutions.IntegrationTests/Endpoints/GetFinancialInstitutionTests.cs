using FinancialInstitutions.IntegrationTests.Infrastructure;
using FluentAssertions;
using Intec.Banking.FinancialInstitutions.Application.DTOs;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace FinancialInstitutions.IntegrationTests.Endpoints;

[Collection(nameof(IntegrationTestCollection))]
public sealed class GetFinancialInstitutionsTests : IntegrationTestBase
{
    public GetFinancialInstitutionsTests(FinancialInstitutionsApiFactory factory)
        : base(factory) { }

    [Fact]
    public async Task GET_List_Returns200WithItems()
    {
        await Client.PostAsJsonAsync("/api/financial-institutions", TestData.ColombianBank());
        await Client.PostAsJsonAsync("/api/financial-institutions", TestData.ColombianBank());

        var response = await Client.GetAsync("/api/financial-institutions?page=1&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var items = await response.Content.ReadFromJsonAsync<List<FinancialInstitutionDto>>();
        items.Should().NotBeNull();
        items!.Count.Should().BeGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task GET_List_InvalidPage_Returns400()
    {
        var response = await Client.GetAsync(
            "/api/financial-institutions?page=0&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GET_List_PageSizeTooLarge_Returns400()
    {
        var response = await Client.GetAsync(
            "/api/financial-institutions?page=1&pageSize=200");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GET_List_DoesNotReturnDeletedInstitutions()
    {
        var createResponse = await Client.PostAsJsonAsync(
            "/api/financial-institutions", TestData.ColombianBank());
        var id = await createResponse.Content.ReadFromJsonAsync<Guid>();
        await Client.DeleteAsync($"/api/financial-institutions/{id}");

        var response = await Client.GetAsync("/api/financial-institutions?page=1&pageSize=10");
        var items = await response.Content.ReadFromJsonAsync<List<FinancialInstitutionDto>>();

        items!.Should().NotContain(x => x.Id == id);
    }

    [Fact]
    public async Task GET_List_WithoutTenantHeader_Returns401()
    {
        var clientWithoutTenant = Factory.CreateClient();

        var response = await clientWithoutTenant.GetAsync(
            "/api/financial-institutions?page=1&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}