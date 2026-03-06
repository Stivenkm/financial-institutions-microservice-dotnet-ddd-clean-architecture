using Intec.Banking.FinancialInstitutions.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace FinancialInstitutions.IntegrationTests.Infrastructure;

/// <summary>
/// xUnit Collection — one PostgreSQL container shared across all integration tests.
/// Container starts once, tests run, container stops.
/// </summary>
[CollectionDefinition(nameof(IntegrationTestCollection))]
public sealed class IntegrationTestCollection
    : ICollectionFixture<FinancialInstitutionsApiFactory>;

/// <summary>
/// Base class for all integration tests.
/// Provides HttpClient with X-Tenant-Id and DbContext for setup/assertions.
/// Each test class inherits this and calls ResetDatabaseAsync() in constructor
/// or via IAsyncLifetime to ensure test isolation.
/// </summary>
public abstract class IntegrationTestBase : IAsyncLifetime
{
    protected readonly FinancialInstitutionsApiFactory Factory;
    protected readonly HttpClient Client;

    protected IntegrationTestBase(FinancialInstitutionsApiFactory factory)
    {
        Factory = factory;
        Client = factory.CreateClientWithTenant();
    }

    /// <summary>
    /// Resets all financial institution data between tests.
    /// Preserves schema — only deletes rows.
    /// </summary>
    protected async Task ResetDatabaseAsync()
    {
        using var db = Factory.CreateDbContext();

        // Order matters — owned types first
        await db.Database.ExecuteSqlRawAsync(
            "DELETE FROM \"FinancialInstitutions\"");
    }

    public virtual Task InitializeAsync() => ResetDatabaseAsync();

    public virtual Task DisposeAsync()
    {
        Client.Dispose();
        return Task.CompletedTask;
    }
}