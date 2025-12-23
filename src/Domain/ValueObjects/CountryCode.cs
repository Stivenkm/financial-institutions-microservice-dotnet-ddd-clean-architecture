using Intec.Banking.FinancialInstitutions.Primitives;

namespace Intec.Banking.FinancialInstitutions.Domain.ValueObjects;

public sealed class CountryCode : ValueObject
{
    public string Code { get; }

    private CountryCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Country code cannot be empty.", nameof(code));

        if (code.Length != 2 && code.Length != 3)
            throw new ArgumentException("Country code must be 2 or 3 characters (ISO 3166).", nameof(code));

        Code = code.ToUpperInvariant();
    }

    public static CountryCode Create(string code) => new(code);

    public static CountryCode Colombia => new("CO");
    public static CountryCode UnitedStates => new("US");

    public bool IsColombia() => Code == "CO";

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Code;
    }

    public override string ToString() => Code;
}
