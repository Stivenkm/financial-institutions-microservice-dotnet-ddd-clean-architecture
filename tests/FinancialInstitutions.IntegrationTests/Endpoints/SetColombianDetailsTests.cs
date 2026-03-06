using FinancialInstitutions.IntegrationTests.Infrastructure;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace FinancialInstitutions.IntegrationTests.Endpoints;

[Collection(nameof(IntegrationTestCollection))]
public sealed class SetColombianDetailsTests : IntegrationTestBase
{
    public SetColombianDetailsTests(FinancialInstitutionsApiFactory factory)
        : base(factory) { }

    [Fact]
    public async Task PUT_ValidColombianDetails_Returns204()
    {
        var createResponse = await Client.PostAsJsonAsync(
            "/api/financial-institutions", TestData.ColombianBank());
        var id = await createResponse.Content.ReadFromJsonAsync<Guid>();

        var body = new { AchCode = "BCOLOBOG", SuperFinancialCode = "010" };

        var response = await Client.PutAsJsonAsync(
            $"/api/financial-institutions/{id}/colombian-details", body);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task PUT_ColombianDetails_WithoutSuperFinancialCode_Returns204()
    {
        var createResponse = await Client.PostAsJsonAsync(
            "/api/financial-institutions", TestData.ColombianBank());
        var id = await createResponse.Content.ReadFromJsonAsync<Guid>();

        var body = new { AchCode = "BCOLOBOG", SuperFinancialCode = (string?)null };

        var response = await Client.PutAsJsonAsync(
            $"/api/financial-institutions/{id}/colombian-details", body);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task PUT_ColombianDetails_NonExistingInstitution_Returns404()
    {
        var body = new { AchCode = "BCOLOBOG", SuperFinancialCode = "010" };

        var response = await Client.PutAsJsonAsync(
            $"/api/financial-institutions/{Guid.NewGuid()}/colombian-details", body);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task PUT_ColombianDetails_NonColombianInstitution_Returns400()
    {
        // El dominio lanza InvalidOperationException cuando la institución no es colombiana.
        // GlobalExceptionHandler lo mapea a 400 Bad Request.
        var createResponse = await Client.PostAsJsonAsync(
            "/api/financial-institutions", TestData.InternationalBank());
        var id = await createResponse.Content.ReadFromJsonAsync<Guid>();

        var body = new { AchCode = "BCOLOBOG", SuperFinancialCode = "010" };

        var response = await Client.PutAsJsonAsync(
            $"/api/financial-institutions/{id}/colombian-details", body);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PUT_ColombianDetails_EmptyAchCode_Returns400()
    {
        var createResponse = await Client.PostAsJsonAsync(
            "/api/financial-institutions", TestData.ColombianBank());
        var id = await createResponse.Content.ReadFromJsonAsync<Guid>();

        var body = new { AchCode = "", SuperFinancialCode = "010" };

        var response = await Client.PutAsJsonAsync(
            $"/api/financial-institutions/{id}/colombian-details", body);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PUT_ColombianDetails_WithoutTenantHeader_Returns401()
    {
        var clientWithoutTenant = Factory.CreateClient();

        var body = new { AchCode = "BCOLOBOG", SuperFinancialCode = "010" };

        var response = await clientWithoutTenant.PutAsJsonAsync(
            $"/api/financial-institutions/{Guid.NewGuid()}/colombian-details", body);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}