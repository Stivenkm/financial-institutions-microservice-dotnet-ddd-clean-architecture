using FluentAssertions;
using Intec.Banking.FinancialInstitutions.Domain.ValueObjects;
using Xunit;

namespace FinancialInstitutions.UnitTests.Domain.ValueObjects;

public sealed class SwiftBicTests
{
    // ── Create — valid ────────────────────────────────────────────────────────

    [Theory]
    [InlineData("AAAABBCC")]       // 8 chars — valid
    [InlineData("AAAABBCC123")]    // 11 chars — valid
    [InlineData("aaaabbcc")]       // lowercase — normalizes
    [InlineData("AAAA BB CC")]     // spaces — stripped
    [InlineData("BBVACOBBXXX")]    // real-world example
    public void Create_ValidCode_ReturnsSwiftBic(string code)
    {
        var act = () => SwiftBic.Create(code);

        act.Should().NotThrow();
    }

    [Fact]
    public void Create_LowercaseCode_NormalizesToUppercase()
    {
        var swift = SwiftBic.Create("aaaabbcc");

        swift.Code.Should().Be("AAAABBCC");
    }

    [Fact]
    public void Create_CodeWithSpaces_StripsSpaces()
    {
        var swift = SwiftBic.Create("AAAA BB CC");

        swift.Code.Should().Be("AAAABBCC");
    }

    // ── Create — invalid ──────────────────────────────────────────────────────

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_EmptyOrNull_ThrowsArgumentException(string? code)
    {
        var act = () => SwiftBic.Create(code!);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*cannot be empty*");
    }

    [Theory]
    [InlineData("AAAABB")]         // 6 chars
    [InlineData("AAAABBCCD")]      // 9 chars — invalid length
    [InlineData("AAAABBCCDD")]     // 10 chars — invalid length
    [InlineData("AAAABBCCDDDD")]   // 12 chars
    [InlineData("1AAABBCC")]       // starts with digit
    [InlineData("AAAA1BCC")]       // digit in wrong position
    public void Create_InvalidFormat_ThrowsArgumentException(string code)
    {
        var act = () => SwiftBic.Create(code);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*8 or 11 characters*");
    }

    // ── IsTestCode ────────────────────────────────────────────────────────────

    [Fact]
    public void IsTestCode_8CharCodeWithZeroAtPosition7_ReturnsTrue()
    {
        var swift = SwiftBic.Create("AAAABBC0");  // position 7 = '0' ✅

        swift.IsTestCode.Should().BeTrue();
    }

    [Fact]
    public void IsTestCode_NormalCode_ReturnsFalse()
    {
        var swift = SwiftBic.Create("AAAABBCC");

        swift.IsTestCode.Should().BeFalse();
    }

    // ── Equality ──────────────────────────────────────────────────────────────

    [Fact]
    public void TwoSwiftBics_SameCode_AreEqual()
    {
        var a = SwiftBic.Create("AAAABBCC");
        var b = SwiftBic.Create("AAAABBCC");

        a.Should().Be(b);
    }

    [Fact]
    public void TwoSwiftBics_DifferentCode_AreNotEqual()
    {
        var a = SwiftBic.Create("AAAABBCC");
        var b = SwiftBic.Create("BBBBCCDD");

        a.Should().NotBe(b);
    }

    [Fact]
    public void TwoSwiftBics_CaseInsensitive_AreEqual()
    {
        var a = SwiftBic.Create("aaaabbcc");
        var b = SwiftBic.Create("AAAABBCC");

        a.Should().Be(b);
    }

    // ── ToString ──────────────────────────────────────────────────────────────

    [Fact]
    public void ToString_ReturnsUppercaseCode()
    {
        SwiftBic.Create("aaaabbcc").ToString().Should().Be("AAAABBCC");
    }
}