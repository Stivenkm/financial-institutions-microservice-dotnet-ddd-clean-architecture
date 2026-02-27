namespace Intec.Banking.FinancialInstitutions.Primitives;

/// <summary>
/// Provides the current tenant identifier for the active HTTP request.
/// Reads X-Tenant-Id header and makes it available to the DbContext and interceptor.
/// </summary>
public interface ITenantService
{
    /// <summary>
    /// Nullable — null if no HttpContext or header is absent/invalid.
    /// Used by HasQueryFilter in DbContext.
    /// </summary>
    Guid? TenantId { get; }

    /// <summary>
    /// Throws UnauthorizedAccessException if header is missing or invalid.
    /// Used by AuditInterceptor when populating TenantId on INSERT/UPDATE.
    /// </summary>
    Guid GetRequiredTenantId();
}