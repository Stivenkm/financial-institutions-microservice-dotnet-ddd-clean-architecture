using Intec.Banking.FinancialInstitutions.Primitives;

namespace Intec.Banking.FinancialInstitutions.Domain.ValueObjects;

public sealed class ColombianBankingDetails : ValueObject
{
    public LocalBankCode AchBankCode { get; }
    public string? SuperFinancialCode { get; }
    private ColombianBankingDetails() { }

    private ColombianBankingDetails(LocalBankCode achBankCode, string? superFinancialCode)
    {
        if (!achBankCode.Country.IsColombia())
            throw new ArgumentException("ACH bank code must be for Colombia.", nameof(achBankCode));

        if (achBankCode.CodeType != "ACH")
            throw new ArgumentException("Code type must be ACH.", nameof(achBankCode));

        AchBankCode = achBankCode ?? throw new ArgumentNullException(nameof(achBankCode));
        SuperFinancialCode = superFinancialCode?.Trim();
    }

    public static ColombianBankingDetails Create(LocalBankCode achBankCode, string? superFinancialCode = null)
        => new(achBankCode, superFinancialCode);

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return AchBankCode;

        if (SuperFinancialCode is not null)
            yield return SuperFinancialCode;
    }

    public override string ToString() => $"ACH:{AchBankCode.Code}";
}
