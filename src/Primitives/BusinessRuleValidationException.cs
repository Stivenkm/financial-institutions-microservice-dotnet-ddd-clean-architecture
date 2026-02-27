namespace Intec.Banking.FinancialInstitutions.Primitives;

/// <summary>
/// Thrown when a business rule is broken.
/// Handled by GlobalExceptionHandler → 400 Bad Request.
/// </summary>
public sealed class BusinessRuleValidationException : Exception
{
    public IBusinessRule BrokenRule { get; }

    public BusinessRuleValidationException(IBusinessRule rule)
        : base(rule.Message)
    {
        BrokenRule = rule;
    }
}