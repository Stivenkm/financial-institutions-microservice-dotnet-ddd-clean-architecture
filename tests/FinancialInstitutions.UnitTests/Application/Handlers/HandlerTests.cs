using Ardalis.GuardClauses;
using FluentAssertions;
using Intec.Banking.FinancialInstitutions.Application.Features.FinancialInstitutions.AddLocalCode;
using Intec.Banking.FinancialInstitutions.Application.Features.FinancialInstitutions.CreateFinancialInstitution;
using Intec.Banking.FinancialInstitutions.Application.Features.FinancialInstitutions.DeleteFinancialInstitution;
using Intec.Banking.FinancialInstitutions.Application.Features.FinancialInstitutions.GetFinancialInstitutionById;
using Intec.Banking.FinancialInstitutions.Application.Features.FinancialInstitutions.GetFinancialInstitutions;
using Intec.Banking.FinancialInstitutions.Application.Features.FinancialInstitutions.SearchFinancialInstitutions;
using Intec.Banking.FinancialInstitutions.Application.Features.FinancialInstitutions.SetColombianDetails;
using Intec.Banking.FinancialInstitutions.Application.Features.FinancialInstitutions.UpdateFinancialInstituion;
using Intec.Banking.FinancialInstitutions.Application.Features.FinancialInstitutions.UpdateFinancialInstitution;
using Intec.Banking.FinancialInstitutions.Domain;
using Intec.Banking.FinancialInstitutions.Domain.ValueObjects;
using Intec.Banking.FinancialInstitutions.Infrastructure;
using Intec.Banking.FinancialInstitutions.Primitives;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Xunit;

namespace FinancialInstitutions.UnitTests.Application.Handlers;

// ── Helpers ───────────────────────────────────────────────────────────────────

file static class Helpers
{
    public static FinancialInstitution ColombianBank() =>
        FinancialInstitution.CreateBank(
            "Banco Colombia", null,
            CountryCode.Colombia,
            TaxId.Create("900123456-1", CountryCode.Colombia),
            null);

    public static FinancialInstitution InternationalBank() =>
        FinancialInstitution.CreateBank(
            "Bank of America", "BofA",
            CountryCode.UnitedStates,
            TaxId.Create("12-3456789", CountryCode.UnitedStates),
            SwiftBic.Create("AAAABBCC"));
}

// ── CreateFinancialInstitutionCommandHandler ──────────────────────────────────

public sealed class CreateFinancialInstitutionCommandHandlerTests
{
    private readonly IFinancialInstitutionRepository _repository = Substitute.For<IFinancialInstitutionRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly CreateFinancialInstitutionCommandHandler _sut;

    public CreateFinancialInstitutionCommandHandlerTests()
        => _sut = new CreateFinancialInstitutionCommandHandler(_repository, _unitOfWork);

    [Fact]
    public async Task HandleAsync_ValidColombianCommand_ReturnsId()
    {
        var cmd = new CreateFinancialInstitutionCommand("Banco Colombia", null, "CO", "900123456-1", null);

        var result = await _sut.HandleAsync(cmd, CancellationToken.None);

        result.Should().NotBeNull();
        result.Value.Should().NotBe(Guid.Empty);
        await _repository.Received(1).AddAsync(Arg.Any<FinancialInstitution>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_ValidInternationalCommand_ReturnsId()
    {
        var cmd = new CreateFinancialInstitutionCommand("Bank of America", "BofA", "US", "12-3456789", "AAAABBCC");

        var result = await _sut.HandleAsync(cmd, CancellationToken.None);

        result.Value.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task HandleAsync_InvalidSwiftBic_ThrowsArgumentException()
    {
        var cmd = new CreateFinancialInstitutionCommand("Bank", null, "US", "12-3456789", "INVALID");

        var act = async () => await _sut.HandleAsync(cmd, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>();
        await _repository.DidNotReceive().AddAsync(Arg.Any<FinancialInstitution>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_NonColombianWithoutSwift_ThrowsArgumentException()
    {
        var cmd = new CreateFinancialInstitutionCommand("Bank", null, "US", "12-3456789", null);

        var act = async () => await _sut.HandleAsync(cmd, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*SWIFT/BIC is required*");
    }
}

// ── UpdateFinancialInstitutionCommandHandler ──────────────────────────────────

public sealed class UpdateFinancialInstitutionCommandHandlerTests
{
    private readonly IFinancialInstitutionRepository _repository = Substitute.For<IFinancialInstitutionRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly UpdateFinancialInstitutionCommandHandler _sut;

    public UpdateFinancialInstitutionCommandHandlerTests()
        => _sut = new UpdateFinancialInstitutionCommandHandler(_repository, _unitOfWork);

    [Fact]
    public async Task HandleAsync_ValidCommand_UpdatesAndReturnsId()
    {
        var institution = Helpers.ColombianBank();
        _repository.GetByIdAsync(Arg.Any<FinancialInstitutionId>(), Arg.Any<CancellationToken>())
            .Returns(institution);

        var cmd = new UpdateFinancialInstitutionCommand(
            institution.Id, "Nuevo Nombre", null, "CO", "900123456-1", null, 0);

        var result = await _sut.HandleAsync(cmd, CancellationToken.None);

        result.Should().Be(institution.Id);
        institution.OfficialName.Should().Be("Nuevo Nombre");
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_InstitutionNotFound_ThrowsNotFoundException()
    {
        _repository.GetByIdAsync(Arg.Any<FinancialInstitutionId>(), Arg.Any<CancellationToken>())
            .Returns((FinancialInstitution?)null);

        // OriginalVersion = 0 — handler throws NotFoundException before reaching concurrency check
        var cmd = new UpdateFinancialInstitutionCommand(
            FinancialInstitutionId.New(), "Nombre", null, "CO", "900123456-1", null, 0);

        var act = async () => await _sut.HandleAsync(cmd, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task HandleAsync_VersionMismatch_ThrowsDbUpdateConcurrencyException()
    {
        var institution = Helpers.ColombianBank(); // OriginalVersion = 0
        _repository.GetByIdAsync(Arg.Any<FinancialInstitutionId>(), Arg.Any<CancellationToken>())
            .Returns(institution);

        // Send version 99 — institution has version 0 → mismatch
        var cmd = new UpdateFinancialInstitutionCommand(
            institution.Id, "Nombre", null, "CO", "900123456-1", null, 99);

        var act = async () => await _sut.HandleAsync(cmd, CancellationToken.None);

        await act.Should().ThrowAsync<DbUpdateConcurrencyException>()
            .WithMessage("*version*");
    }

    [Fact]
    public async Task HandleAsync_CorrectVersion_ExecutesSuccessfully()
    {
        var institution = Helpers.ColombianBank(); // OriginalVersion = 0
        _repository.GetByIdAsync(Arg.Any<FinancialInstitutionId>(), Arg.Any<CancellationToken>())
            .Returns(institution);

        // OriginalVersion matches — concurrency check passes
        var cmd = new UpdateFinancialInstitutionCommand(
            institution.Id, "Nuevo Nombre", null, "CO", "900123456-1", null, 0);

        var act = async () => await _sut.HandleAsync(cmd, CancellationToken.None);

        await act.Should().NotThrowAsync<DbUpdateConcurrencyException>();
    }
}

// ── DeleteFinancialInstitutionCommandHandler ──────────────────────────────────

public sealed class DeleteFinancialInstitutionCommandHandlerTests
{
    private readonly IFinancialInstitutionRepository _repository = Substitute.For<IFinancialInstitutionRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly DeleteFinancialInstitutionCommandHandler _sut;

    public DeleteFinancialInstitutionCommandHandlerTests()
        => _sut = new DeleteFinancialInstitutionCommandHandler(_repository, _unitOfWork);

    [Fact]
    public async Task HandleAsync_ExistingInstitution_SoftDeletes()
    {
        var institution = Helpers.ColombianBank();
        _repository.GetByIdAsync(Arg.Any<FinancialInstitutionId>(), Arg.Any<CancellationToken>())
            .Returns(institution);

        await _sut.HandleAsync(new DeleteFinancialInstitutionCommand(institution.Id), CancellationToken.None);

        institution.IsDeleted.Should().BeTrue();
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_NotFound_ThrowsNotFoundException()
    {
        _repository.GetByIdAsync(Arg.Any<FinancialInstitutionId>(), Arg.Any<CancellationToken>())
            .Returns((FinancialInstitution?)null);

        var act = async () => await _sut.HandleAsync(
            new DeleteFinancialInstitutionCommand(FinancialInstitutionId.New()),
            CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}

// ── AddLocalCodeCommandHandler ────────────────────────────────────────────────

public sealed class AddLocalCodeCommandHandlerTests
{
    private readonly IFinancialInstitutionRepository _repository = Substitute.For<IFinancialInstitutionRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly AddLocalCodeCommandHandler _sut;

    public AddLocalCodeCommandHandlerTests()
        => _sut = new AddLocalCodeCommandHandler(_repository, _unitOfWork);

    [Fact]
    public async Task HandleAsync_ValidCommand_AddsLocalCode()
    {
        var institution = Helpers.ColombianBank();
        _repository.GetByIdAsync(Arg.Any<FinancialInstitutionId>(), Arg.Any<CancellationToken>())
            .Returns(institution);

        var cmd = new AddLocalCodeCommand(institution.Id, "001", "ACH");
        await _sut.HandleAsync(cmd, CancellationToken.None);

        institution.LocalCodes.Should().HaveCount(1);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_NotFound_ThrowsNotFoundException()
    {
        _repository.GetByIdAsync(Arg.Any<FinancialInstitutionId>(), Arg.Any<CancellationToken>())
            .Returns((FinancialInstitution?)null);

        var act = async () => await _sut.HandleAsync(
            new AddLocalCodeCommand(FinancialInstitutionId.New(), "001", "ACH"),
            CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}

// ── SetColombianDetailsCommandHandler ─────────────────────────────────────────

public sealed class SetColombianDetailsCommandHandlerTests
{
    private readonly IFinancialInstitutionRepository _repository = Substitute.For<IFinancialInstitutionRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly SetColombianDetailsCommandHandler _sut;

    public SetColombianDetailsCommandHandlerTests()
        => _sut = new SetColombianDetailsCommandHandler(_repository, _unitOfWork);

    [Fact]
    public async Task HandleAsync_ColombianInstitution_SetsDetailsAndRegistersAchCode()
    {
        var institution = Helpers.ColombianBank();
        _repository.GetByIdAsync(Arg.Any<FinancialInstitutionId>(), Arg.Any<CancellationToken>())
            .Returns(institution);

        var cmd = new SetColombianDetailsCommand(institution.Id, "001", "0014");
        await _sut.HandleAsync(cmd, CancellationToken.None);

        institution.ColombianDetails.Should().NotBeNull();
        // ACH code is also registered in LocalCodes for payment routing
        institution.LocalCodes.Should().ContainSingle(c => c.Code == "001");
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_NonColombianInstitution_ThrowsArgumentException()
    {
        var institution = Helpers.InternationalBank();
        _repository.GetByIdAsync(Arg.Any<FinancialInstitutionId>(), Arg.Any<CancellationToken>())
            .Returns(institution);

        var cmd = new SetColombianDetailsCommand(institution.Id, "001", null);

        var act = async () => await _sut.HandleAsync(cmd, CancellationToken.None);

        // ColombianBankingDetails.Create fails first because achBankCode.Country = US ≠ CO
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Colombia*");
    }

    [Fact]
    public async Task HandleAsync_NotFound_ThrowsNotFoundException()
    {
        _repository.GetByIdAsync(Arg.Any<FinancialInstitutionId>(), Arg.Any<CancellationToken>())
            .Returns((FinancialInstitution?)null);

        var act = async () => await _sut.HandleAsync(
            new SetColombianDetailsCommand(FinancialInstitutionId.New(), "001", null),
            CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}

// ── GetFinancialInstitutionByIdQueryHandler ───────────────────────────────────

public sealed class GetFinancialInstitutionByIdQueryHandlerTests
{
    private readonly IFinancialInstitutionRepository _repository = Substitute.For<IFinancialInstitutionRepository>();
    private readonly GetFinancialInstitutionByIdQueryHandler _sut;

    public GetFinancialInstitutionByIdQueryHandlerTests()
        => _sut = new GetFinancialInstitutionByIdQueryHandler(_repository);

    [Fact]
    public async Task HandleAsync_ExistingId_ReturnsDto()
    {
        var institution = Helpers.ColombianBank();
        _repository.GetByIdAsync(Arg.Any<FinancialInstitutionId>(), Arg.Any<CancellationToken>())
            .Returns(institution);

        var result = await _sut.HandleAsync(
            new GetFinancialInstitutionByIdQuery(institution.Id),
            CancellationToken.None);

        result.Should().NotBeNull();
        result!.Id.Should().Be(institution.Id.Value);
        result.OfficialName.Should().Be("Banco Colombia");
    }

    [Fact]
    public async Task HandleAsync_NotFound_ReturnsNull()
    {
        _repository.GetByIdAsync(Arg.Any<FinancialInstitutionId>(), Arg.Any<CancellationToken>())
            .Returns((FinancialInstitution?)null);

        var result = await _sut.HandleAsync(
            new GetFinancialInstitutionByIdQuery(FinancialInstitutionId.New()),
            CancellationToken.None);

        result.Should().BeNull();
    }
}

// ── GetFinancialInstitutionsQueryHandler ──────────────────────────────────────

public sealed class GetFinancialInstitutionsQueryHandlerTests
{
    private readonly IFinancialInstitutionRepository _repository = Substitute.For<IFinancialInstitutionRepository>();
    private readonly GetFinancialInstitutionsQueryHandler _sut;

    public GetFinancialInstitutionsQueryHandlerTests()
        => _sut = new GetFinancialInstitutionsQueryHandler(_repository);

    [Fact]
    public async Task HandleAsync_ReturnsMappedDtos()
    {
        var institutions = new List<FinancialInstitution>
        {
            Helpers.ColombianBank(),
            Helpers.InternationalBank()
        };
        _repository.GetPagedAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(institutions);

        var result = await _sut.HandleAsync(
            new GetFinancialInstitutionsQuery(1, 10),
            CancellationToken.None);

        result.Should().HaveCount(2);
        result[0].OfficialName.Should().Be("Banco Colombia");
        result[1].OfficialName.Should().Be("Bank of America");
    }

    [Fact]
    public async Task HandleAsync_EmptyRepository_ReturnsEmptyList()
    {
        _repository.GetPagedAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new List<FinancialInstitution>());

        var result = await _sut.HandleAsync(
            new GetFinancialInstitutionsQuery(1, 10),
            CancellationToken.None);

        result.Should().BeEmpty();
    }
}

// ── SearchFinancialInstitutionsQueryHandler ───────────────────────────────────

public sealed class SearchFinancialInstitutionsQueryHandlerTests
{
    private readonly IFinancialInstitutionRepository _repository = Substitute.For<IFinancialInstitutionRepository>();
    private readonly SearchFinancialInstitutionsQueryHandler _sut;

    public SearchFinancialInstitutionsQueryHandlerTests()
        => _sut = new SearchFinancialInstitutionsQueryHandler(_repository);

    [Fact]
    public async Task HandleAsync_WithFilters_PassesFiltersToRepository()
    {
        _repository.SearchAsync(
                Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(),
                Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new List<FinancialInstitution>());

        var query = new SearchFinancialInstitutionsQuery("CO", "Banco", null, 1, 10);
        await _sut.HandleAsync(query, CancellationToken.None);

        await _repository.Received(1).SearchAsync(
            "CO", "Banco", null,
            Arg.Any<int>(), Arg.Any<int>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_ReturnsMappedDtos()
    {
        _repository.SearchAsync(
                Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(),
                Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new List<FinancialInstitution> { Helpers.ColombianBank() });

        var result = await _sut.HandleAsync(
            new SearchFinancialInstitutionsQuery(null, null, null, 1, 10),
            CancellationToken.None);

        result.Should().HaveCount(1);
        result[0].OfficialName.Should().Be("Banco Colombia");
    }
}