namespace Intec.Banking.FinancialInstitutions.Primitives;

// Interfaz no genérica — usada por AuditInterceptor para acceder
// a los setters de auditoría sin conocer el tipo del Id
public interface IAggregate
{
    void SetCreated(DateTime createdAt, int? createdBy);
    void SetLastModified(DateTime modifiedAt, int? modifiedBy);
    void SetDeleted(DateTime deletedAt, int? deletedBy);

    void SetTenant(Guid tenantId);
    void IncrementVersion();
}

public interface IAggregate<TId>
{
}