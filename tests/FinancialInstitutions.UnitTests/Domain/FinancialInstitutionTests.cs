using FluentAssertions;
using Intec.Banking.FinancialInstitutions.Domain;
using Intec.Banking.FinancialInstitutions.Domain.Events;
using Intec.Banking.FinancialInstitutions.Domain.ValueObjects;
using Xunit;

namespace FinancialInstitutions.UnitTests.Domain;

public sealed class FinancialInstitutionTests
{
    // ── Helpers ───────────────────────────────────────────────────────────────

    private static CountryCode Colombia => CountryCode.Colombia;
    private static CountryCode UnitedStates => CountryCode.UnitedStates;
    private static TaxId ColombianTaxId => TaxId.Create("900123456-1", Colombia);
    private static TaxId UsTaxId => TaxId.Create("12-3456789", UnitedStates);
    private static SwiftBic ValidSwift => SwiftBic.Create("AAAABBCC");

    private static FinancialInstitution CreateColombian() =>
        FinancialInstitution.CreateBank("Banco Colombia", null, Colombia, ColombianTaxId, null);

    private static FinancialInstitution CreateInternational() =>
        FinancialInstitution.CreateBank("Bank of America", "BofA", UnitedStates, UsTaxId, ValidSwift);

    // ── CreateBank — valid ────────────────────────────────────────────────────

    [Fact]
    public void CreateBank_ColombianWithoutSwift_Succeeds()
    {
        var institution = CreateColombian();

        institution.OfficialName.Should().Be("Banco Colombia");
        institution.Country.Should().Be(Colombia);
        institution.SwiftBic.Should().BeNull();
        institution.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void CreateBank_InternationalWithSwift_Succeeds()
    {
        var institution = CreateInternational();

        institution.OfficialName.Should().Be("Bank of America");
        institution.TradeName.Should().Be("BofA");
        institution.SwiftBic.Should().Be(ValidSwift);
    }

    [Fact]
    public void CreateBank_TrimsOfficialName()
    {
        var institution = FinancialInstitution.CreateBank(
            "  Banco Colombia  ", null, Colombia, ColombianTaxId, null);

        institution.OfficialName.Should().Be("Banco Colombia");
    }

    [Fact]
    public void CreateBank_GeneratesUniqueId()
    {
        var a = CreateColombian();
        var b = CreateColombian();

        a.Id.Should().NotBe(b.Id);
    }

    [Fact]
    public void CreateBank_RaisesFinancialInstitutionCreatedEvent()
    {
        var institution = CreateColombian();

        var events = institution.GetUncommittedDomainEvents();

        events.Should().ContainSingle()
            .Which.Should().BeOfType<FinancialInstitutionCreatedEvent>();
    }

    // ── CreateBank — invalid ──────────────────────────────────────────────────

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void CreateBank_EmptyOfficialName_ThrowsArgumentException(string? name)
    {
        var act = () => FinancialInstitution.CreateBank(
            name!, null, Colombia, ColombianTaxId, null);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*cannot be empty*");
    }

    [Fact]
    public void CreateBank_NonColombianWithoutSwift_ThrowsArgumentException()
    {
        var act = () => FinancialInstitution.CreateBank(
            "Bank of America", null, UnitedStates, UsTaxId, null);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*SWIFT/BIC is required*");
    }

    [Fact]
    public void CreateBank_TaxIdCountryMismatch_ThrowsArgumentException()
    {
        var act = () => FinancialInstitution.CreateBank(
            "Banco Colombia", null, Colombia, UsTaxId, null);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*TaxId country must match*");
    }

    // ── Update ────────────────────────────────────────────────────────────────

    [Fact]
    public void Update_ValidArgs_UpdatesProperties()
    {
        var institution = CreateColombian();
        institution.DequeueUncommittedDomainEvents(); // clear created event

        institution.Update("Nuevo Nombre", "NN", Colombia, ColombianTaxId, null);

        institution.OfficialName.Should().Be("Nuevo Nombre");
        institution.TradeName.Should().Be("NN");
    }

    [Fact]
    public void Update_RaisesFinancialInstitutionUpdatedEvent()
    {
        var institution = CreateColombian();
        institution.DequeueUncommittedDomainEvents();

        institution.Update("Nuevo Nombre", null, Colombia, ColombianTaxId, null);

        institution.GetUncommittedDomainEvents()
            .Should().ContainSingle()
            .Which.Should().BeOfType<FinancialInstitutionUpdatedEvent>();
    }

    [Fact]
    public void Update_EmptyOfficialName_ThrowsArgumentException()
    {
        var institution = CreateColombian();

        var act = () => institution.Update("", null, Colombia, ColombianTaxId, null);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*cannot be empty*");
    }

    [Fact]
    public void Update_TaxIdCountryMismatch_ThrowsArgumentException()
    {
        var institution = CreateColombian();

        var act = () => institution.Update("Nombre", null, Colombia, UsTaxId, null);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*TaxId country must match*");
    }

    [Fact]
    public void Update_NonColombianWithoutSwift_ThrowsArgumentException()
    {
        var institution = CreateInternational();

        var act = () => institution.Update("Bank", null, UnitedStates, UsTaxId, null);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*SWIFT/BIC is required*");
    }

    // ── Delete ────────────────────────────────────────────────────────────────

    [Fact]
    public void Delete_ActiveInstitution_SetsIsDeletedTrue()
    {
        var institution = CreateColombian();

        institution.Delete();

        institution.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public void Delete_RaisesFinancialInstitutionDeletedEvent()
    {
        var institution = CreateColombian();
        institution.DequeueUncommittedDomainEvents();

        institution.Delete();

        institution.GetUncommittedDomainEvents()
            .Should().ContainSingle()
            .Which.Should().BeOfType<FinancialInstitutionDeletedEvent>();
    }

    [Fact]
    public void Delete_AlreadyDeleted_ThrowsInvalidOperationException()
    {
        var institution = CreateColombian();
        institution.Delete();

        var act = () => institution.Delete();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*already deleted*");
    }

    // ── AddLocalCode ──────────────────────────────────────────────────────────

    [Fact]
    public void AddLocalCode_NewCode_AddsToCollection()
    {
        var institution = CreateColombian();
        var code = LocalBankCode.CreateAchCode("001", Colombia);

        institution.AddLocalCode(code);

        institution.LocalCodes.Should().ContainSingle()
            .Which.Should().Be(code);
    }

    [Fact]
    public void AddLocalCode_DuplicateCode_DoesNotAdd()
    {
        var institution = CreateColombian();
        var code = LocalBankCode.CreateAchCode("001", Colombia);

        institution.AddLocalCode(code);
        institution.AddLocalCode(code);

        institution.LocalCodes.Should().HaveCount(1);
    }

    [Fact]
    public void AddLocalCode_RaisesLocalCodeAddedEvent()
    {
        var institution = CreateColombian();
        institution.DequeueUncommittedDomainEvents();

        var code = LocalBankCode.CreateAchCode("001", Colombia);
        institution.AddLocalCode(code);

        institution.GetUncommittedDomainEvents()
            .Should().ContainSingle()
            .Which.Should().BeOfType<LocalCodeAddedEvent>();
    }

    [Fact]
    public void AddLocalCode_DuplicateCode_DoesNotRaiseEvent()
    {
        var institution = CreateColombian();
        var code = LocalBankCode.CreateAchCode("001", Colombia);
        institution.AddLocalCode(code);
        institution.DequeueUncommittedDomainEvents();

        institution.AddLocalCode(code); // duplicate

        institution.HasUncommittedDomainEvents().Should().BeFalse();
    }

    // ── SetColombianDetails ───────────────────────────────────────────────────

    [Fact]
    public void SetColombianDetails_ColombianInstitution_SetsDetails()
    {
        var institution = CreateColombian();
        var achCode = LocalBankCode.CreateAchCode("001", Colombia);
        var details = ColombianBankingDetails.Create(achCode, "0014");

        institution.SetColombianDetails(details);

        institution.ColombianDetails.Should().Be(details);
    }

    [Fact]
    public void SetColombianDetails_NonColombianInstitution_ThrowsInvalidOperationException()
    {
        var institution = CreateInternational();
        var achCode = LocalBankCode.CreateAchCode("001", Colombia);
        var details = ColombianBankingDetails.Create(achCode);

        var act = () => institution.SetColombianDetails(details);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Colombian details only allowed*");
    }

    // ── CanReceiveInternationalTransfer ───────────────────────────────────────

    [Fact]
    public void CanReceiveInternationalTransfer_WithSwift_ReturnsTrue()
    {
        var institution = CreateInternational();

        institution.CanReceiveInternationalTransfer().Should().BeTrue();
    }

    [Fact]
    public void CanReceiveInternationalTransfer_WithoutSwift_ReturnsFalse()
    {
        var institution = CreateColombian();

        institution.CanReceiveInternationalTransfer().Should().BeFalse();
    }

    // ── Domain Events infrastructure ──────────────────────────────────────────

    [Fact]
    public void DequeueUncommittedDomainEvents_ClearsEvents()
    {
        var institution = CreateColombian();

        institution.DequeueUncommittedDomainEvents();

        institution.HasUncommittedDomainEvents().Should().BeFalse();
    }

    [Fact]
    public void ToString_ReturnsFormattedString()
    {
        var institution = CreateColombian();

        institution.ToString().Should().Contain("Banco Colombia")
            .And.Contain("CO");
    }
}