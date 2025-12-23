

namespace Intec.Banking.FinancialInstitutions.Primitives;

public interface IHaveCreator
{
    DateTime Created { get; }
    int? CreatedBy { get; }
}