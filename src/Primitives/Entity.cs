
namespace Intec.Banking.FinancialInstitutions.Primitives;

public abstract class Entity<TId> : IEntity<TId>
{
    public TId Id { get; protected set; } = default!;

}