using Intec.Banking.FinancialInstitutions.Primitives;

namespace Intec.Banking.FinancialInstitutions.Domain.ValueObjects;

public sealed class LocalBankCode : ValueObject
{
    public string Code { get; }
    public string CodeType { get; }
    public CountryCode Country { get; }
    private LocalBankCode() { }

    private LocalBankCode(string code, string codeType, CountryCode country)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Bank code cannot be empty.", nameof(code));

        if (string.IsNullOrWhiteSpace(codeType))
            throw new ArgumentException("Code type cannot be empty.", nameof(codeType));

        Code = code.Trim();
        CodeType = codeType.ToUpperInvariant();
        Country = country ?? throw new ArgumentNullException(nameof(country));
    }

    public static LocalBankCode Create(string code, string codeType, CountryCode country)
        => new(code, codeType, country);

    public static LocalBankCode CreateAchCode(string code, CountryCode country)
        => new(code, "ACH", country);

    public static LocalBankCode CreateRoutingNumber(string code, CountryCode country)
        => new(code, "ROUTING", country);

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Code;
        yield return CodeType;
        yield return Country;
    }

    public override string ToString() => $"{CodeType}:{Code} ({Country})";
}
