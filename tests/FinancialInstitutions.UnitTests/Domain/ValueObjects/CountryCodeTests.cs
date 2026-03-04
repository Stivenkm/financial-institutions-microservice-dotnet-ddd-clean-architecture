using FluentAssertions;
using Intec.Banking.FinancialInstitutions.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using Xunit;

namespace FinancialInstitutions.UnitTests.Domain.ValueObjects;

public sealed class CountryCodeTests
{
    // ── Create — valid ────────────────────────────────────────────────────────

    [Theory]
    [InlineData("CO")]
    [InlineData("US")]
    [InlineData("co")]   // normalizes to uppercase
    [InlineData("COL")]  // 3-char ISO
    [InlineData("USA")]
    public void Create_ValidCode_ReturnsCountryCode(string code)
    {
        var country = CountryCode.Create(code);

        country.Code.Should().Be(code.ToUpperInvariant());
    }

    // ── Create — invalid ──────────────────────────────────────────────────────

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_EmptyOrNull_ThrowsArgumentException(string? code)
    {
        var act = () => CountryCode.Create(code!);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*cannot be empty*");
    }

    [Theory]
    [InlineData("C")]       // 1 char
    [InlineData("COLA")]    // 4 chars
    [InlineData("COLOMBIA")]
    public void Create_InvalidLength_ThrowsArgumentException(string code)
    {
        var act = () => CountryCode.Create(code);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*2 or 3 characters*");
    }

    // ── IsColombia ────────────────────────────────────────────────────────────

    [Fact]
    public void IsColombia_WithCO_ReturnsTrue()
    {
        var country = CountryCode.Colombia;

        country.IsColombia().Should().BeTrue();
    }

    [Fact]
    public void IsColombia_WithOtherCode_ReturnsFalse()
    {
        var country = CountryCode.UnitedStates;

        country.IsColombia().Should().BeFalse();
    }

    // ── Static factories ──────────────────────────────────────────────────────

    [Fact]
    public void Colombia_ReturnsCodeCO()
    {
        CountryCode.Colombia.Code.Should().Be("CO");
    }

    [Fact]
    public void UnitedStates_ReturnsCodeUS()
    {
        CountryCode.UnitedStates.Code.Should().Be("US");
    }

    // ── Equality ──────────────────────────────────────────────────────────────

    [Fact]
    public void TwoCountryCodes_SameValue_AreEqual()
    {
        var a = CountryCode.Create("CO");
        var b = CountryCode.Create("CO");

        a.Should().Be(b);
    }

    [Fact]
    public void TwoCountryCodes_DifferentValue_AreNotEqual()
    {
        var a = CountryCode.Create("CO");
        var b = CountryCode.Create("US");

        a.Should().NotBe(b);
    }

    [Fact]
    public void TwoCountryCodes_CaseInsensitive_AreEqual()
    {
        var a = CountryCode.Create("co");
        var b = CountryCode.Create("CO");

        a.Should().Be(b);
    }

    // ── ToString ──────────────────────────────────────────────────────────────

    [Fact]
    public void ToString_ReturnUppercaseCode()
    {
        CountryCode.Create("co").ToString().Should().Be("CO");
    }
}