namespace Intec.Banking.FinancialInstitutions.Primitives;

/// <summary>
/// Marks an aggregate as tenant-aware.
/// Used by AuditInterceptor to automatically populate TenantId on every SaveChanges.
/// </summary>
public interface IHaveTenant
{
    Guid TenantId { get; }
    void SetTenant(Guid tenantId);
}