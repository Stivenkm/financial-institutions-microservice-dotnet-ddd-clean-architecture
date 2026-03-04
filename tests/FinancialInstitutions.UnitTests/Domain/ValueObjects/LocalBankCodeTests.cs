using FluentAssertions;
using Intec.Banking.FinancialInstitutions.Domain.ValueObjects;
using Xunit;

namespace FinancialInstitutions.UnitTests.Domain.ValueObjects;

public sealed class LocalBankCodeTests
{
    // ── Create — valid ────────────────────────────────────────────────────────

    [Fact]
    public void Create_ValidArgs_ReturnsLocalBankCode()
    {
        var country = CountryCode.Colombia;
        var code = LocalBankCode.Create("001", "ACH", country);

        code.Code.Should().Be("001");
        code.CodeType.Should().Be("ACH");
        code.Country.Should().Be(country);
    }

    [Fact]
    public void Create_CodeTypeNormalizesToUppercase()
    {
        var code = LocalBankCode.Create("001", "ach", CountryCode.Colombia);

        code.CodeType.Should().Be("ACH");
    }

    [Fact]
    public void Create_CodeTrimsWhitespace()
    {
        var code = LocalBankCode.Create("  001  ", "ACH", CountryCode.Colombia);

        code.Code.Should().Be("001");
    }

    // ── Create — invalid ──────────────────────────────────────────────────────

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_EmptyCode_ThrowsArgumentException(string? code)
    {
        var act = () => LocalBankCode.Create(code!, "ACH", CountryCode.Colombia);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*cannot be empty*");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_EmptyCodeType_ThrowsArgumentException(string? codeType)
    {
        var act = () => LocalBankCode.Create("001", codeType!, CountryCode.Colombia);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*cannot be empty*");
    }

    [Fact]
    public void Create_NullCountry_ThrowsArgumentNullException()
    {
        var act = () => LocalBankCode.Create("001", "ACH", null!);

        act.Should().Throw<ArgumentNullException>();
    }

    // ── Static factories ──────────────────────────────────────────────────────

    [Fact]
    public void CreateAchCode_SetsCodeTypeToACH()
    {
        var code = LocalBankCode.CreateAchCode("001", CountryCode.Colombia);

        code.CodeType.Should().Be("ACH");
    }

    [Fact]
    public void CreateRoutingNumber_SetsCodeTypeToROUTING()
    {
        var code = LocalBankCode.CreateRoutingNumber("021000021", CountryCode.UnitedStates);

        code.CodeType.Should().Be("ROUTING");
    }

    // ── Equality ──────────────────────────────────────────────────────────────

    [Fact]
    public void TwoLocalBankCodes_SameValues_AreEqual()
    {
        var a = LocalBankCode.Create("001", "ACH", CountryCode.Colombia);
        var b = LocalBankCode.Create("001", "ACH", CountryCode.Colombia);

        a.Should().Be(b);
    }

    [Fact]
    public void TwoLocalBankCodes_DifferentCode_AreNotEqual()
    {
        var a = LocalBankCode.Create("001", "ACH", CountryCode.Colombia);
        var b = LocalBankCode.Create("002", "ACH", CountryCode.Colombia);

        a.Should().NotBe(b);
    }

    // ── ToString ──────────────────────────────────────────────────────────────

    [Fact]
    public void ToString_ReturnsFormattedString()
    {
        var code = LocalBankCode.Create("001", "ACH", CountryCode.Colombia);

        code.ToString().Should().Be("ACH:001 (CO)");
    }
}