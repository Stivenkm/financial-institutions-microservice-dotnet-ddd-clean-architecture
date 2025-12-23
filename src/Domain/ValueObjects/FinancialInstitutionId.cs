using Intec.Banking.FinancialInstitutions.Primitives;

namespace Intec.Banking.FinancialInstitutions.Domain.ValueObjects;

public sealed record FinancialInstitutionId : AggregateId<Guid>
{
    public FinancialInstitutionId(Guid value) : base(value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("FinancialInstitutionId cannot be empty.", nameof(value));
    }

    public static FinancialInstitutionId New() => new(Guid.NewGuid());

    public static FinancialInstitutionId From(Guid value) => new(value);

    public override string ToString() => Value.ToString();
}
