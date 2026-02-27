using Intec.Banking.FinancialInstitutions.Primitives;

namespace Intec.Banking.FinancialInstitutions.Infrastructure.Services;

/// <summary>
/// Reads the current tenant from the X-Tenant-Id HTTP header.
/// Scoped per request — each request has its own tenant context.
///
/// IMPORTANT: Constructor never throws. HttpContext does not exist during
/// startup/scope-validation. Enforcement happens via TenantValidationMiddleware.
/// </summary>
public sealed class TenantService : ITenantService
{
    public const string HeaderName = "X-Tenant-Id";

    private readonly IHttpContextAccessor _httpContextAccessor;

    public TenantService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// Nullable — null si no hay HttpContext o el header está ausente/inválido.
    /// Usado por HasQueryFilter en el DbContext.
    /// </summary>
    public Guid? TenantId
    {
        get
        {
            var header = _httpContextAccessor.HttpContext?
                .Request.Headers[HeaderName]
                .FirstOrDefault();

            return !string.IsNullOrWhiteSpace(header) && Guid.TryParse(header, out var parsed)
                ? parsed
                : null;
        }
    }

    /// <summary>
    /// Lanza UnauthorizedAccessException si el header está ausente o es inválido.
    /// Llamado por AuditInterceptor al poblar TenantId en INSERT/UPDATE.
    /// </summary>
    public Guid GetRequiredTenantId() =>
        TenantId ?? throw new UnauthorizedAccessException(
            $"Missing or invalid '{HeaderName}' header.");
}