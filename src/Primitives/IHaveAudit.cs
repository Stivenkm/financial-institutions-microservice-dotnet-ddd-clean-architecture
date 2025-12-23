

namespace Intec.Banking.FinancialInstitutions.Primitives;

public interface IHaveAudit : IHaveCreator
{
    DateTime? LastModified { get; }
    int? LastModifiedBy { get; }
}