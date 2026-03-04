using FluentAssertions;
using FluentValidation.TestHelper;
using Intec.Banking.FinancialInstitutions.Application.Common;
using Intec.Banking.FinancialInstitutions.Application.Features.FinancialInstitutions.AddLocalCode;
using Intec.Banking.FinancialInstitutions.Application.Features.FinancialInstitutions.CreateFinancialInstitution;
using Intec.Banking.FinancialInstitutions.Application.Features.FinancialInstitutions.DeleteFinancialInstitution;
using Intec.Banking.FinancialInstitutions.Application.Features.FinancialInstitutions.GetFinancialInstitutions;
using Intec.Banking.FinancialInstitutions.Application.Features.FinancialInstitutions.SearchFinancialInstitutions;
using Intec.Banking.FinancialInstitutions.Application.Features.FinancialInstitutions.SetColombianDetails;
using Intec.Banking.FinancialInstitutions.Application.Features.FinancialInstitutions.UpdateFinancialInstituion;
using Intec.Banking.FinancialInstitutions.Application.Features.FinancialInstitutions.UpdateFinancialInstitution;
using Intec.Banking.FinancialInstitutions.Domain.ValueObjects;
using Xunit;

namespace FinancialInstitutions.UnitTests.Application.Validators;

// ── CreateFinancialInstitutionValidator ───────────────────────────────────────

public sealed class CreateFinancialInstitutionValidatorTests
{
    private readonly CreateFinancialInstitutionValidator _sut = new();

    private static CreateFinancialInstitutionCommand Valid() =>
        new("Banco Colombia", null, "CO", "900123456-1", null);

    [Fact]
    public void Valid_Command_PassesValidation()
    {
        _sut.TestValidate(Valid()).ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void OfficialName_Empty_FailsValidation(string? name)
    {
        var cmd = new CreateFinancialInstitutionCommand(name!, null, "CO", "900123456-1", null);
        _sut.TestValidate(cmd).ShouldHaveValidationErrorFor(x => x.OfficialName);
    }

    [Fact]
    public void OfficialName_ExceedsMaxLength_FailsValidation()
    {
        var cmd = new CreateFinancialInstitutionCommand(new string('A', 201), null, "CO", "900123456-1", null);
        _sut.TestValidate(cmd).ShouldHaveValidationErrorFor(x => x.OfficialName);
    }

    [Theory]
    [InlineData("C")]
    [InlineData("COLA")]
    public void CountryCode_InvalidLength_FailsValidation(string code)
    {
        var cmd = new CreateFinancialInstitutionCommand("Banco", null, code, "900123456-1", null);
        _sut.TestValidate(cmd).ShouldHaveValidationErrorFor(x => x.CountryCode);
    }

    [Theory]
    [InlineData("CO")]
    [InlineData("COL")]
    public void CountryCode_ValidLength_PassesValidation(string code)
    {
        var cmd = new CreateFinancialInstitutionCommand("Banco", null, code, "900123456-1", null);
        _sut.TestValidate(cmd).ShouldNotHaveValidationErrorFor(x => x.CountryCode);
    }

    [Theory]
    [InlineData("AAAABBCCD")]
    [InlineData("AAAABBCCDD")]
    [InlineData("1AAABBCC")]
    public void SwiftBicCode_InvalidFormat_FailsValidation(string swift)
    {
        var cmd = new CreateFinancialInstitutionCommand("Banco", null, "CO", "900123456-1", swift);
        _sut.TestValidate(cmd).ShouldHaveValidationErrorFor(x => x.SwiftBicCode);
    }

    [Theory]
    [InlineData("AAAABBCC")]
    [InlineData("AAAABBCC123")]
    public void SwiftBicCode_ValidFormat_PassesValidation(string swift)
    {
        var cmd = new CreateFinancialInstitutionCommand("Banco", null, "CO", "900123456-1", swift);
        _sut.TestValidate(cmd).ShouldNotHaveValidationErrorFor(x => x.SwiftBicCode);
    }

    [Fact]
    public void SwiftBicCode_Null_PassesValidation()
    {
        var cmd = new CreateFinancialInstitutionCommand("Banco", null, "CO", "900123456-1", null);
        _sut.TestValidate(cmd).ShouldNotHaveValidationErrorFor(x => x.SwiftBicCode);
    }
}

// ── UpdateFinancialInstitutionValidator ───────────────────────────────────────

public sealed class UpdateFinancialInstitutionValidatorTests
{
    private readonly UpdateFinancialInstitutionValidator _sut = new();

    private static UpdateFinancialInstitutionCommand Valid() =>
        new(FinancialInstitutionId.New(), "Banco Colombia", null, "CO", "900123456-1", null, 0);

    [Fact]
    public void Valid_Command_PassesValidation()
    {
        _sut.TestValidate(Valid()).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void OfficialName_Empty_FailsValidation()
    {
        var cmd = new UpdateFinancialInstitutionCommand(FinancialInstitutionId.New(), "", null, "CO", "900123456-1", null, 0);
        _sut.TestValidate(cmd).ShouldHaveValidationErrorFor(x => x.OfficialName);
    }

    [Theory]
    [InlineData("C")]
    [InlineData("COLA")]
    public void CountryCode_InvalidLength_FailsValidation(string code)
    {
        var cmd = new UpdateFinancialInstitutionCommand(FinancialInstitutionId.New(), "Banco", null, code, "900123456-1", null, 0);
        _sut.TestValidate(cmd).ShouldHaveValidationErrorFor(x => x.CountryCode);
    }

    [Fact]
    public void TaxIdValue_Empty_FailsValidation()
    {
        var cmd = new UpdateFinancialInstitutionCommand(FinancialInstitutionId.New(), "Banco", null, "CO", "", null, 0);
        _sut.TestValidate(cmd).ShouldHaveValidationErrorFor(x => x.TaxIdValue);
    }

    [Fact]
    public void OriginalVersion_Negative_FailsValidation()
    {
        var cmd = new UpdateFinancialInstitutionCommand(FinancialInstitutionId.New(), "Banco", null, "CO", "900123456-1", null, -1);
        _sut.TestValidate(cmd).ShouldHaveValidationErrorFor(x => x.OriginalVersion);
    }
}

// ── DeleteFinancialInstitutionValidator ───────────────────────────────────────

public sealed class DeleteFinancialInstitutionValidatorTests
{
    private readonly DeleteFinancialInstitutionValidator _sut = new();

    [Fact]
    public void Valid_Command_PassesValidation()
    {
        var cmd = new DeleteFinancialInstitutionCommand(FinancialInstitutionId.New());
        _sut.TestValidate(cmd).ShouldNotHaveAnyValidationErrors();
    }

    // Note: FinancialInstitutionId rejects Guid.Empty at construction time (domain invariant).
    // The validator's NotEmpty rule on Id is a defense-in-depth guard for deserialization
    // scenarios where Id could be default — not testable via FinancialInstitutionId directly.
}

// ── GetFinancialInstitutionsQueryValidator ────────────────────────────────────

public sealed class GetFinancialInstitutionsQueryValidatorTests
{
    private readonly GetFinancialInstitutionsQueryValidator _sut = new();

    [Fact]
    public void Valid_Query_PassesValidation()
    {
        var query = new GetFinancialInstitutionsQuery(1, 10);
        _sut.TestValidate(query).ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Page_LessThanMinimum_FailsValidation(int page)
    {
        var query = new GetFinancialInstitutionsQuery(page, 10);
        _sut.TestValidate(query).ShouldHaveValidationErrorFor(x => x.Page);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    [InlineData(200)]
    public void PageSize_OutOfRange_FailsValidation(int pageSize)
    {
        var query = new GetFinancialInstitutionsQuery(1, pageSize);
        _sut.TestValidate(query).ShouldHaveValidationErrorFor(x => x.PageSize);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(50)]
    [InlineData(PaginationParams.DefaultMaxPageSize)]
    public void PageSize_WithinRange_PassesValidation(int pageSize)
    {
        var query = new GetFinancialInstitutionsQuery(1, pageSize);
        _sut.TestValidate(query).ShouldNotHaveValidationErrorFor(x => x.PageSize);
    }
}

// ── SearchFinancialInstitutionsQueryValidator ─────────────────────────────────

public sealed class SearchFinancialInstitutionsQueryValidatorTests
{
    private readonly SearchFinancialInstitutionsQueryValidator _sut = new();

    private static SearchFinancialInstitutionsQuery Valid() =>
        new(null, null, null, 1, 10);

    [Fact]
    public void Valid_Query_NoFilters_PassesValidation()
    {
        _sut.TestValidate(Valid()).ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("C")]       // 1 char
    [InlineData("COLA")]    // 4 chars
    public void CountryCode_InvalidLength_FailsValidation(string code)
    {
        var query = new SearchFinancialInstitutionsQuery(code, null, null, 1, 10);
        _sut.TestValidate(query).ShouldHaveValidationErrorFor(x => x.CountryCode);
    }

    [Theory]
    [InlineData("CO")]
    [InlineData("COL")]
    public void CountryCode_ValidLength_PassesValidation(string code)
    {
        var query = new SearchFinancialInstitutionsQuery(code, null, null, 1, 10);
        _sut.TestValidate(query).ShouldNotHaveValidationErrorFor(x => x.CountryCode);
    }

    [Theory]
    [InlineData("AAAABBCCD")]   // 9 chars — invalid
    [InlineData("AAAABBCCDD")]  // 10 chars — invalid
    public void SwiftBicCode_InvalidFormat_FailsValidation(string swift)
    {
        var query = new SearchFinancialInstitutionsQuery(null, null, swift, 1, 10);
        _sut.TestValidate(query).ShouldHaveValidationErrorFor(x => x.SwiftBicCode);
    }

    [Theory]
    [InlineData("AAAABBCC")]     // 8 chars
    [InlineData("AAAABBCC123")] // 11 chars
    public void SwiftBicCode_ValidFormat_PassesValidation(string swift)
    {
        var query = new SearchFinancialInstitutionsQuery(null, null, swift, 1, 10);
        _sut.TestValidate(query).ShouldNotHaveValidationErrorFor(x => x.SwiftBicCode);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public void PageSize_OutOfRange_FailsValidation(int pageSize)
    {
        var query = new SearchFinancialInstitutionsQuery(null, null, null, 1, pageSize);
        _sut.TestValidate(query).ShouldHaveValidationErrorFor(x => x.PageSize);
    }
}

// ── AddLocalCodeRequestValidator ──────────────────────────────────────────────

public sealed class AddLocalCodeRequestValidatorTests
{
    private readonly AddLocalCodeRequestValidator _sut = new();

    [Fact]
    public void Valid_Request_PassesValidation()
    {
        var request = new AddLocalCodeRequest("001", "ACH");
        _sut.TestValidate(request).ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Code_Empty_FailsValidation(string? code)
    {
        var request = new AddLocalCodeRequest(code!, "ACH");
        _sut.TestValidate(request).ShouldHaveValidationErrorFor(x => x.Code);
    }

    [Fact]
    public void Code_ExceedsMaxLength_FailsValidation()
    {
        var request = new AddLocalCodeRequest(new string('A', 51), "ACH");
        _sut.TestValidate(request).ShouldHaveValidationErrorFor(x => x.Code);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void CodeType_Empty_FailsValidation(string? codeType)
    {
        var request = new AddLocalCodeRequest("001", codeType!);
        _sut.TestValidate(request).ShouldHaveValidationErrorFor(x => x.CodeType);
    }
}

// ── SetColombianDetailsRequestValidator ───────────────────────────────────────

public sealed class SetColombianDetailsRequestValidatorTests
{
    private readonly SetColombianDetailsRequestValidator _sut = new();

    [Fact]
    public void Valid_Request_PassesValidation()
    {
        var request = new SetColombianDetailsRequest("001", null);
        _sut.TestValidate(request).ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void AchCode_Empty_FailsValidation(string? achCode)
    {
        var request = new SetColombianDetailsRequest(achCode!, null);
        _sut.TestValidate(request).ShouldHaveValidationErrorFor(x => x.AchCode);
    }

    [Fact]
    public void AchCode_ExceedsMaxLength_FailsValidation()
    {
        var request = new SetColombianDetailsRequest(new string('A', 51), null);
        _sut.TestValidate(request).ShouldHaveValidationErrorFor(x => x.AchCode);
    }

    [Fact]
    public void SuperFinancialCode_ExceedsMaxLength_FailsValidation()
    {
        var request = new SetColombianDetailsRequest("001", new string('A', 21));
        _sut.TestValidate(request).ShouldHaveValidationErrorFor(x => x.SuperFinancialCode);
    }

    [Fact]
    public void SuperFinancialCode_Null_PassesValidation()
    {
        var request = new SetColombianDetailsRequest("001", null);
        _sut.TestValidate(request).ShouldNotHaveValidationErrorFor(x => x.SuperFinancialCode);
    }
}