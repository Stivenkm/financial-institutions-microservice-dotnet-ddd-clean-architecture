using Intec.Banking.FinancialInstitutions.Application.Features.FinancialInstitutions.CreateFinancialInstitution;
using Intec.Banking.FinancialInstitutions.Application.Features.FinancialInstitutions.DeleteFinancialInstitution;
using Intec.Banking.FinancialInstitutions.Application.Features.FinancialInstitutions.GetFinancialInstitutionById;
using Intec.Banking.FinancialInstitutions.Application.Features.FinancialInstitutions.UpdateFinancialInstitution;
using Intec.Banking.FinancialInstitutions.Domain.ValueObjects;
using Intec.Banking.FinancialInstitutions.Primitives;
using Intec.Banking.FinanciialInstitutions.Application.Features.FinnacialInstitutions.UpdateFinancialIntituion;
using Microsoft.AspNetCore.Mvc;

namespace Intec.Banking.FinancialInstitutions.Application.Features.FinancialInstitutions;

public static class FinancialInstitutionEndpoints
{
    public static IEndpointRouteBuilder MapFinancialInstitutionEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/financial-institutions")
            .WithTags("Financial Institutions")
            .WithOpenApi();

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
}
