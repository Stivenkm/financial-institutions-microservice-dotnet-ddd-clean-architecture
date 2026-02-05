using Intec.Banking.FinancialInstitutions.Primitives;

namespace Intec.Banking.FinancialInstitutions.Domain.ValueObjects;

public sealed class TaxId : ValueObject
{
    public string Value { get; }
    public CountryCode Country { get; }

    private TaxId() { }

    private TaxId(string value, CountryCode country)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Tax ID value cannot be empty.", nameof(value));

        Value = value.Trim();
        Country = country ?? throw new ArgumentNullException(nameof(country));
    }

    public static TaxId Create(string value, CountryCode country) => new(value, country);

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
        yield return Country;
    }

    public override string ToString() => $"{Value} ({Country})";
}
