using FluentAssertions;
using Intec.Banking.FinancialInstitutions.Domain.ValueObjects;
using Xunit;

namespace FinancialInstitutions.UnitTests.Domain.ValueObjects;

public sealed class ColombianBankingDetailsTests
{
    private static LocalBankCode ValidAchCode =>
        LocalBankCode.CreateAchCode("001", CountryCode.Colombia);

    // ── Create — valid ────────────────────────────────────────────────────────

    [Fact]
    public void Create_ValidAchCode_ReturnsDetails()
    {
        var details = ColombianBankingDetails.Create(ValidAchCode);

        details.AchBankCode.Should().Be(ValidAchCode);
        details.SuperFinancialCode.Should().BeNull();
    }

    [Fact]
    public void Create_WithSuperFinancialCode_SetsCode()
    {
        var details = ColombianBankingDetails.Create(ValidAchCode, "  0014  ");

        details.SuperFinancialCode.Should().Be("0014");
    }

    // ── Create — invalid ──────────────────────────────────────────────────────

    [Fact]
    public void Create_NonColombianAchCode_ThrowsArgumentException()
    {
        var usCode = LocalBankCode.CreateAchCode("001", CountryCode.UnitedStates);

        var act = () => ColombianBankingDetails.Create(usCode);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*Colombia*");
    }

    [Fact]
    public void Create_WrongCodeType_ThrowsArgumentException()
    {
        var routingCode = LocalBankCode.CreateRoutingNumber("001", CountryCode.Colombia);

        var act = () => ColombianBankingDetails.Create(routingCode);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*ACH*");
    }

    // ── Equality ──────────────────────────────────────────────────────────────

    [Fact]
    public void TwoDetails_SameValues_AreEqual()
    {
        var a = ColombianBankingDetails.Create(ValidAchCode, "0014");
        var b = ColombianBankingDetails.Create(ValidAchCode, "0014");

        a.Should().Be(b);
    }

    [Fact]
    public void TwoDetails_DifferentSuperCode_AreNotEqual()
    {
        var a = ColombianBankingDetails.Create(ValidAchCode, "0014");
        var b = ColombianBankingDetails.Create(ValidAchCode, "0015");

        a.Should().NotBe(b);
    }

    // ── ToString ──────────────────────────────────────────────────────────────

    [Fact]
    public void ToString_ReturnsAchCode()
    {
        var details = ColombianBankingDetails.Create(ValidAchCode);

        details.ToString().Should().Be("ACH:001");
    }
}