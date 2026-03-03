using Intec.Banking.FinancialInstitutions.Application.Features.FinancialInstitutions.AddLocalCode;
using Intec.Banking.FinancialInstitutions.Application.Features.FinancialInstitutions.CreateFinancialInstitution;
using Intec.Banking.FinancialInstitutions.Application.Features.FinancialInstitutions.DeleteFinancialInstitution;
using Intec.Banking.FinancialInstitutions.Application.Features.FinancialInstitutions.GetFinancialInstitutionById;
using Intec.Banking.FinancialInstitutions.Application.Features.FinancialInstitutions.GetFinancialInstitutions;
using Intec.Banking.FinancialInstitutions.Application.Features.FinancialInstitutions.SearchFinancialInstitutions;
using Intec.Banking.FinancialInstitutions.Application.Features.FinancialInstitutions.SetColombianDetails;
using Intec.Banking.FinancialInstitutions.Application.Features.FinancialInstitutions.UpdateFinancialInstituion;
using Intec.Banking.FinancialInstitutions.Domain.ValueObjects;
using Intec.Banking.FinancialInstitutions.Infrastructure.Filters;
using Intec.Banking.FinancialInstitutions.Primitives;
using Microsoft.AspNetCore.Mvc;

namespace Intec.Banking.FinancialInstitutions.Application.Features.FinancialInstitutions;

public static class FinancialInstitutionEndpoints
{
    public static IEndpointRouteBuilder MapFinancialInstitutionEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/financial-institutions")
            .WithTags("Financial Institutions")
            .WithOpenApi()
            .AddEndpointFilter<ValidationFilter>();

        group.MapPost("/", CreateFinancialInstitution)
            .WithName("CreateFinancialInstitution")
            .WithSummary("Create a new financial institution")
            .Produces<Guid>(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        group.MapGet("/{id:guid}", GetFinancialInstitutionById)
            .WithName("GetFinancialInstitutionById")
            .WithSummary("Get a financial institution by ID")
            .Produces<DTOs.FinancialInstitutionDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/", GetFinancialInstitutions)
            .WithName("GetFinancialInstitutions")
            .WithSummary("Get financial institutions paginated")
            .Produces<List<DTOs.FinancialInstitutionDto>>(StatusCodes.Status200OK)
            .ProducesValidationProblem();

        group.MapGet("/search", SearchFinancialInstitutions)
            .WithName("SearchFinancialInstitutions")
            .WithSummary("Search financial institutions with filters")
            .Produces<List<DTOs.FinancialInstitutionDto>>(StatusCodes.Status200OK)
            .ProducesValidationProblem();

        group.MapPut("/{id:guid}", UpdateFinancialInstitution)
            .WithName("UpdateFinancialInstitution")
            .WithSummary("Update an existing financial institution")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:guid}", DeleteFinancialInstitution)
            .WithName("DeleteFinancialInstitution")
            .WithSummary("Delete an existing financial institution")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/{id:guid}/local-codes", AddLocalCode)
            .WithName("AddLocalCode")
            .WithSummary("Add Local Code")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .ProducesValidationProblem();

        group.MapPut("/{id:guid}/colombian-details", SetColombianDetails)
            .WithName("SetColombianDetails")
            .WithSummary("Set Colombian regulatory details")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .ProducesValidationProblem();

        return app;
    }

    private static async Task<IResult> CreateFinancialInstitution(
        [FromBody] CreateFinancialInstitutionCommand command,
        [FromServices] CommandDispatcher dispatcher,
        CancellationToken ct)
    {
        var id = await dispatcher.DispatchAsync(command, ct);
        return Results.Created($"/api/financial-institutions/{id.Value}", id.Value);
    }

    private static async Task<IResult> GetFinancialInstitutionById(
        Guid id,
        [FromServices] QueryDispatcher dispatcher,
        CancellationToken ct)
    {
        var financialInstitutionId = FinancialInstitutionId.From(id);
        var query = new GetFinancialInstitutionByIdQuery(financialInstitutionId);
        var result = await dispatcher.DispatchAsync(query, ct);

        return result is not null
            ? Results.Ok(result)
            : Results.NotFound();
    }

    private static async Task<IResult> GetFinancialInstitutions(
    [FromQuery] int page,
    [FromQuery] int pageSize,
    [FromServices] QueryDispatcher dispatcher,
    CancellationToken ct)
    {
        var query = new GetFinancialInstitutionsQuery(page, pageSize);

        var result = await dispatcher.DispatchAsync(query, ct);

        return Results.Ok(result);
    }

    private static async Task<IResult> SearchFinancialInstitutions(
    string? country,
    string? name,
    string? swiftBicCode,
    int page,
    int pageSize,
    [FromServices] QueryDispatcher dispatcher,
    CancellationToken ct)
    {
        var query = new SearchFinancialInstitutionsQuery(
            country,
            name,
            swiftBicCode,
            page,
            pageSize);

        var result = await dispatcher.DispatchAsync(query, ct);

        return Results.Ok(result);
    }

    private static async Task<IResult> UpdateFinancialInstitution(
        Guid id,
        [FromBody] UpdateFinancialInstitutionCommand command,
        [FromServices] CommandDispatcher dispatcher,
        CancellationToken ct)
    {
        var updatedCommand = command with
        {
            Id = FinancialInstitutionId.From(id)
        };

        await dispatcher.DispatchAsync(updatedCommand, ct);

        return Results.NoContent();
    }

    private static async Task<IResult> DeleteFinancialInstitution(
    Guid id,
    [FromServices] CommandDispatcher dispatcher,
    CancellationToken ct)
    {
        var command = new DeleteFinancialInstitutionCommand
        {
            Id = FinancialInstitutionId.From(id)
        };

        await dispatcher.DispatchAsync(command, ct);

        return Results.NoContent();
    }

    private static async Task<IResult> AddLocalCode(
        Guid id,
        [FromBody]AddLocalCodeRequest request,
        [FromServices] CommandDispatcher dispatcher,
        CancellationToken ct) 
        {
            var command = new AddLocalCodeCommand(
                new FinancialInstitutionId(id),
                request.Code,
                request.CodeType);

            await dispatcher.DispatchAsync(command, ct);

            return Results.NoContent();
    }

    private static async Task<IResult> SetColombianDetails(
        Guid id,
        [FromBody] SetColombianDetailsRequest request,
        [FromServices] CommandDispatcher dispatcher,
        CancellationToken ct)
    {
        var command = new SetColombianDetailsCommand(
            new FinancialInstitutionId(id),
            request.AchCode,
            request.SuperFinancialCode);

        await dispatcher.DispatchAsync(command, ct);

        return Results.NoContent();
    }
}