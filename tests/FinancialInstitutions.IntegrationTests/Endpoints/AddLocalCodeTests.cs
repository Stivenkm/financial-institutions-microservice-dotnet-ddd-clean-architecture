using FinancialInstitutions.IntegrationTests.Infrastructure;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace FinancialInstitutions.IntegrationTests.Endpoints;

[Collection(nameof(IntegrationTestCollection))]
public sealed class AddLocalCodeTests : IntegrationTestBase
{
    public AddLocalCodeTests(FinancialInstitutionsApiFactory factory)
        : base(factory) { }

    [Fact]
    public async Task POST_ValidLocalCode_Returns204()
    {
        var createResponse = await Client.PostAsJsonAsync(
            "/api/financial-institutions", TestData.ColombianBank());
        var id = await createResponse.Content.ReadFromJsonAsync<Guid>();

        var body = new { Code = "COL001", CodeType = "ACH" };

        var response = await Client.PostAsJsonAsync(
            $"/api/financial-institutions/{id}/local-codes", body);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task POST_LocalCode_NonExistingInstitution_Returns404()
    {
        var body = new { Code = "COL001", CodeType = "ACH" };

        var response = await Client.PostAsJsonAsync(
            $"/api/financial-institutions/{Guid.NewGuid()}/local-codes", body);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task POST_LocalCode_EmptyCode_Returns400()
    {
        var createResponse = await Client.PostAsJsonAsync(
            "/api/financial-institutions", TestData.ColombianBank());
        var id = await createResponse.Content.ReadFromJsonAsync<Guid>();

        var body = new { Code = "", CodeType = "ACH" };

        var response = await Client.PostAsJsonAsync(
            $"/api/financial-institutions/{id}/local-codes", body);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task POST_LocalCode_EmptyCodeType_Returns400()
    {
        var createResponse = await Client.PostAsJsonAsync(
            "/api/financial-institutions", TestData.ColombianBank());
        var id = await createResponse.Content.ReadFromJsonAsync<Guid>();

        var body = new { Code = "COL001", CodeType = "" };

        var response = await Client.PostAsJsonAsync(
            $"/api/financial-institutions/{id}/local-codes", body);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task POST_LocalCode_WithoutTenantHeader_Returns401()
    {
        var clientWithoutTenant = Factory.CreateClient();

        var body = new { Code = "COL001", CodeType = "ACH" };

        var response = await clientWithoutTenant.PostAsJsonAsync(
            $"/api/financial-institutions/{Guid.NewGuid()}/local-codes", body);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}