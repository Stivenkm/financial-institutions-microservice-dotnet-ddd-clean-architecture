using FinancialInstitutions.IntegrationTests.Infrastructure;
using FluentAssertions;
using Intec.Banking.FinancialInstitutions.Application.DTOs;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace FinancialInstitutions.IntegrationTests.Endpoints;

[Collection(nameof(IntegrationTestCollection))]
public sealed class SearchFinancialInstitutionTests : IntegrationTestBase
{
    public SearchFinancialInstitutionTests(FinancialInstitutionsApiFactory factory)
        : base(factory) { }

    [Fact]
    public async Task GET_Search_NoFilters_Returns200WithItems()
    {
        await Client.PostAsJsonAsync("/api/financial-institutions", TestData.ColombianBank());

        var response = await Client.GetAsync(
            "/api/financial-institutions/search?page=1&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var items = await response.Content.ReadFromJsonAsync<List<FinancialInstitutionDto>>();
        items.Should().NotBeNull();
        items!.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GET_Search_ByCountryCode_ReturnsFilteredResults()
    {
        await Client.PostAsJsonAsync("/api/financial-institutions", TestData.ColombianBank());
        await Client.PostAsJsonAsync("/api/financial-institutions", TestData.InternationalBank());

        var response = await Client.GetAsync(
            "/api/financial-institutions/search?countryCode=CO&page=1&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var items = await response.Content.ReadFromJsonAsync<List<FinancialInstitutionDto>>();
        items!.Should().NotBeEmpty();
        items.Should().AllSatisfy(x => x.Country.Should().Be("CO"));
    }

    [Fact]
    public async Task GET_Search_ByName_ReturnsMatchingResults()
    {
        var uniqueName = $"BancoSearch_{Guid.NewGuid():N}";
        await Client.PostAsJsonAsync("/api/financial-institutions",
            TestData.ColombianBank(officialName: uniqueName));

        var response = await Client.GetAsync(
            $"/api/financial-institutions/search?name={uniqueName}&page=1&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var items = await response.Content.ReadFromJsonAsync<List<FinancialInstitutionDto>>();
        items!.Should().ContainSingle(x => x.OfficialName == uniqueName);
    }

    [Fact]
    public async Task GET_Search_ByName_PartialMatch_ReturnsResults()
    {
        var uniqueSuffix = Guid.NewGuid().ToString("N")[..8];
        await Client.PostAsJsonAsync("/api/financial-institutions",
            TestData.ColombianBank(officialName: $"BancoPartial_{uniqueSuffix}"));

        var response = await Client.GetAsync(
            $"/api/financial-institutions/search?name=BancoPartial_{uniqueSuffix}&page=1&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var items = await response.Content.ReadFromJsonAsync<List<FinancialInstitutionDto>>();
        items!.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GET_Search_NoMatches_ReturnsEmptyList()
    {
        var response = await Client.GetAsync(
            "/api/financial-institutions/search?name=NONEXISTENT_XYZ_999&page=1&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var items = await response.Content.ReadFromJsonAsync<List<FinancialInstitutionDto>>();
        items!.Should().BeEmpty();
    }

    [Fact]
    public async Task GET_Search_InvalidSwiftBic_Returns400()
    {
        var response = await Client.GetAsync(
            "/api/financial-institutions/search?swiftBicCode=INVALID&page=1&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GET_Search_InvalidPage_Returns400()
    {
        var response = await Client.GetAsync(
            "/api/financial-institutions/search?page=0&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GET_Search_PageSizeTooLarge_Returns400()
    {
        var response = await Client.GetAsync(
            "/api/financial-institutions/search?page=1&pageSize=200");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GET_Search_InvalidCountryCode_Returns400()
    {
        // CountryCode debe ser 2-3 chars ISO 3166
        var response = await Client.GetAsync(
            "/api/financial-institutions/search?countryCode=TOOLONG&page=1&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GET_Search_DeletedInstitution_NotReturned()
    {
        var uniqueName = $"BancoDeleted_{Guid.NewGuid():N}";
        var createResponse = await Client.PostAsJsonAsync(
            "/api/financial-institutions", TestData.ColombianBank(officialName: uniqueName));
        var id = await createResponse.Content.ReadFromJsonAsync<Guid>();

        await Client.DeleteAsync($"/api/financial-institutions/{id}");

        var response = await Client.GetAsync(
            $"/api/financial-institutions/search?name={uniqueName}&page=1&pageSize=10");

        var items = await response.Content.ReadFromJsonAsync<List<FinancialInstitutionDto>>();
        items!.Should().NotContain(x => x.Id == id);
    }

    [Fact]
    public async Task GET_Search_WithoutTenantHeader_Returns401()
    {
        var clientWithoutTenant = Factory.CreateClient();

        var response = await clientWithoutTenant.GetAsync(
            "/api/financial-institutions/search?page=1&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}