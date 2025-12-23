using Intec.Banking.FinancialInstitutions.Domain.ValueObjects;

namespace Intec.Banking.FinancialInstitutions.Domain;

public sealed class FinancialInstitution
{
    private readonly TaxId _taxId;
    private readonly List<LocalBankCode> _localCodes = new();

    public FinancialInstitutionId Id { get; }
    public string OfficialName { get; private set; }
    public string? TradeName { get; private set; }
    public CountryCode Country { get; private set; }
    public SwiftBic? SwiftBic { get; private set; }

    public IReadOnlyCollection<LocalBankCode> LocalCodes => _localCodes.AsReadOnly();

    public ColombianBankingDetails? ColombianDetails { get; private set; }

    private FinancialInstitution(
        FinancialInstitutionId id,
        string officialName,
        string? tradeName,
        CountryCode country,
        TaxId taxId,
        SwiftBic? swiftBic)
    {
        if (string.IsNullOrWhiteSpace(officialName))
            throw new ArgumentException("Official name cannot be empty.");
        _taxId = taxId;

        Id = id;
        OfficialName = officialName.Trim();
        TradeName = tradeName?.Trim();
        Country = country;
        TaxId = taxId;
        SwiftBic = swiftBic;

        if (!Equals(TaxId.Country, Country))
        {
            throw new ArgumentException("TaxId country must match the institution's country.");
        }
    }

    public TaxId TaxId { get; set; }

    public static FinancialInstitution CreateBank(
        string officialName,
        string? tradeName,
        CountryCode country,
        TaxId taxId,
        SwiftBic? swiftBic)
    {
        if (!country.IsColombia() && swiftBic == null)
            throw new ArgumentException("SWIFT/BIC is required for non-Colombian institutions.");

        return new FinancialInstitution(
            FinancialInstitutionId.New(),
            officialName,
            tradeName,
            country,
            taxId,
            swiftBic);
    }

    public void AddLocalCode(LocalBankCode code)
    {
        if (_localCodes.Contains(code)) return;
        _localCodes.Add(code);
    }

    public void SetColombianDetails(ColombianBankingDetails details)
    {
        if (!Country.IsColombia())
            throw new InvalidOperationException("Colombian details only allowed for Colombian institutions.");
        ColombianDetails = details;
        AddLocalCode(details.AchBankCode);
    }

    public bool CanReceiveInternationalTransfer()
    {
        // Simple rule: needs SWIFT/BIC
        return SwiftBic is not null;
    }

    public override string ToString()
    {
        return $"{OfficialName} [{Country}] - TaxId: {TaxId}";
    }
}
