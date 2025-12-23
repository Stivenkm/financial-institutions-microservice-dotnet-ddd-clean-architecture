

namespace Intec.Banking.FinancialInstitutions.Primitives;

public interface IBusinessRule
{
    bool IsBroken();
    string Message { get; }
}