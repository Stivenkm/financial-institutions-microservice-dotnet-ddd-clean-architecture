using Intec.Banking.FinancialInstitutions.Domain.Events;
using Intec.Banking.FinancialInstitutions.Domain.ValueObjects;
using Intec.Banking.FinancialInstitutions.Primitives;

namespace Intec.Banking.FinancialInstitutions.Domain;

public sealed class FinancialInstitution : Aggregate<FinancialInstitutionId>
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

    private FinancialInstitution() { }

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

        var institution = new FinancialInstitution(
            FinancialInstitutionId.New(),
            officialName,
            tradeName,
            country,
            taxId,
            swiftBic);

        institution.AddDomainEvent(new FinancialInstitutionCreatedEvent(institution.Id));

        return institution;
    }

    public void Update(
        string officialName,
        string? tradeName,
        CountryCode country,
        TaxId taxId,
        SwiftBic? swiftBic)
    {
        if (string.IsNullOrWhiteSpace(officialName))
            throw new ArgumentException("Official name cannot be empty.");

        if (!Equals(taxId.Country, country))
            throw new ArgumentException("TaxId country must match the institution's country.");

        if (!country.IsColombia() && swiftBic == null)
            throw new ArgumentException("SWIFT/BIC is required for non-Colombian institutions.");

        OfficialName = officialName.Trim();
        TradeName = tradeName?.Trim();
        Country = country;
        TaxId = taxId;
        SwiftBic = swiftBic;

        AddDomainEvent(new FinancialInstitutionUpdatedEvent(Id));
    }

    /// <summary>
    /// Marks the institution as deleted (soft delete).
    /// A deleted institution cannot be updated or used in transfers.
    /// Physical deletion is not allowed — financial records must be preserved
    /// for regulatory and audit purposes.
    /// </summary>
    public void Delete()
    {
        if (IsDeleted)
            throw new InvalidOperationException("Financial institution is already deleted.");

        IsDeleted = true;
        AddDomainEvent(new FinancialInstitutionDeletedEvent(Id));
    }

    public void AddLocalCode(LocalBankCode code)
    {
        if (_localCodes.Contains(code)) return;
        _localCodes.Add(code);

        AddDomainEvent(new LocalCodeAddedEvent(Id, code.Code));
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
