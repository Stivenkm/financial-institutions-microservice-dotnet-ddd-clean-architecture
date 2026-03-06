using Intec.Banking.FinancialInstitutions;
using Intec.Banking.FinancialInstitutions.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Testcontainers.PostgreSql;
using Xunit;

namespace FinancialInstitutions.IntegrationTests.Infrastructure;

/// <summary>
/// Shared WebApplicationFactory — spins up one PostgreSQL container
/// for the entire integration test suite via IAsyncLifetime.
/// All tests share the same container; each test class resets data via
/// FinancialInstitutionsDbContext directly.
/// </summary>
public sealed class FinancialInstitutionsApiFactory
    : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("fi_tests")
        .WithUsername("fi_user")
        .WithPassword("fi_pass")
        .Build();

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
    }

    public new async Task DisposeAsync()
    {
        await _postgres.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        // ── Inject connection string BEFORE DI runs ───────────────────────────
        // This overrides DotEnv.Load() + appsettings so DatabaseSeeder and
        // AddInfrastructure both pick up the TestContainers connection string.
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Default"] = _postgres.GetConnectionString(),
                ["APP_NAME"] = "FinancialInstitutionsTests",
                ["KESTREL_PORT"] = "0",
                ["IdGenerator:WorkerId"] = "1",
                ["IdGenerator:DatacenterId"] = "1",
            });
        });

        builder.ConfigureTestServices(services =>
        {
            // ── Suppress noisy EF/ASP logs in test output ────────────────────
            services.AddLogging(logging =>
            {
                logging.SetMinimumLevel(LogLevel.Warning);
                logging.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Warning);
            });
        });
    }

    /// <summary>
    /// Creates an HttpClient with the required X-Tenant-Id header pre-set.
    /// </summary>
    public HttpClient CreateClientWithTenant(
        string tenantId = "00000000-0000-0000-0000-000000000001")
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Add("X-Tenant-Id", tenantId);
        return client;
    }

    /// <summary>
    /// Provides direct DbContext access for test setup/teardown.
    /// </summary>
    public FinancialInstitutionsDbContext CreateDbContext()
    {
        var scope = Services.CreateScope();
        return scope.ServiceProvider
            .GetRequiredService<FinancialInstitutionsDbContext>();
    }
}