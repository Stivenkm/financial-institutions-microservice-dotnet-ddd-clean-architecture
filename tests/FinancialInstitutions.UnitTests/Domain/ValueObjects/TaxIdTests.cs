using FluentAssertions;
using Intec.Banking.FinancialInstitutions.Domain.ValueObjects;
using Xunit;

namespace FinancialInstitutions.UnitTests.Domain.ValueObjects;

public sealed class TaxIdTests
{
    // ── Create — valid ────────────────────────────────────────────────────────

    [Fact]
    public void Create_ValidValueAndCountry_ReturnsTaxId()
    {
        var country = CountryCode.Colombia;
        var taxId = TaxId.Create("900123456-1", country);

        taxId.Value.Should().Be("900123456-1");
        taxId.Country.Should().Be(country);
    }

    [Fact]
    public void Create_ValueWithWhitespace_TrimsValue()
    {
        var taxId = TaxId.Create("  900123456  ", CountryCode.Colombia);

        taxId.Value.Should().Be("900123456");
    }

    // ── Create — invalid ──────────────────────────────────────────────────────

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_EmptyOrNullValue_ThrowsArgumentException(string? value)
    {
        var act = () => TaxId.Create(value!, CountryCode.Colombia);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*cannot be empty*");
    }

    [Fact]
    public void Create_NullCountry_ThrowsArgumentNullException()
    {
        var act = () => TaxId.Create("900123456", null!);

        act.Should().Throw<ArgumentNullException>();
    }

    // ── Equality ──────────────────────────────────────────────────────────────

    [Fact]
    public void TwoTaxIds_SameValueAndCountry_AreEqual()
    {
        var a = TaxId.Create("900123456", CountryCode.Colombia);
        var b = TaxId.Create("900123456", CountryCode.Colombia);

        a.Should().Be(b);
    }

    [Fact]
    public void TwoTaxIds_SameValueDifferentCountry_AreNotEqual()
    {
        var a = TaxId.Create("900123456", CountryCode.Colombia);
        var b = TaxId.Create("900123456", CountryCode.UnitedStates);

        a.Should().NotBe(b);
    }

    [Fact]
    public void TwoTaxIds_DifferentValue_AreNotEqual()
    {
        var a = TaxId.Create("900123456", CountryCode.Colombia);
        var b = TaxId.Create("999999999", CountryCode.Colombia);

        a.Should().NotBe(b);
    }

    // ── ToString ──────────────────────────────────────────────────────────────

    [Fact]
    public void ToString_ReturnsValueAndCountry()
    {
        var taxId = TaxId.Create("900123456", CountryCode.Colombia);

        taxId.ToString().Should().Be("900123456 (CO)");
    }
}