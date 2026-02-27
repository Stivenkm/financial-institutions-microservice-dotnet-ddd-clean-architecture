namespace Intec.Banking.FinancialInstitutions.Infrastructure.Exceptions;

using Ardalis.GuardClauses;
using FluentValidation;
using Intec.Banking.FinancialInstitutions.Primitives;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {

        if (exception is DomainException or ArgumentException or BadHttpRequestException)
            _logger.LogWarning(exception, exception.Message);

        else if (exception is ValidationException)
            _logger.LogInformation(exception, exception.Message);

        else if (exception is DbUpdateConcurrencyException)
            _logger.LogWarning(exception, "Concurrency conflict: {Message}", exception.Message);
        
        else
            _logger.LogError(exception, "Exception occurred: {Message}", exception.Message);

        var (statusCode, title, errors) = exception switch
        {
            ValidationException validationException => (
                StatusCodes.Status400BadRequest,
                "One or more validation errors occurred",
                validationException.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToArray()
                    )
            ),

            DomainException domainException => (
                StatusCodes.Status400BadRequest,
                domainException.Message,
                 new Dictionary<string, string[]>()
            ),

            // Domain validation errors thrown as ArgumentException
            ArgumentException argumentException => (
                StatusCodes.Status400BadRequest,
                argumentException.Message,
                new Dictionary<string, string[]>()
            ),

            NotFoundException => (
                StatusCodes.Status404NotFound,
                "Resource not found",
                new Dictionary<string, string[]>()
            ),

            // Missing or invalid query string / route parameters
            BadHttpRequestException badHttpRequestException => (
                StatusCodes.Status400BadRequest,
                badHttpRequestException.Message,
                new Dictionary<string, string[]>()
            ),

            // Optimistic concurrency conflict
            DbUpdateConcurrencyException concurrencyException => (
                StatusCodes.Status409Conflict,
                concurrencyException.Message,
                new Dictionary<string, string[]>()
            ),

            _ => (
                StatusCodes.Status500InternalServerError,
                "An error occurred while processing your request",
                new Dictionary<string, string[]>()
            )
        };

        httpContext.Response.StatusCode = statusCode;

        await httpContext.Response.WriteAsJsonAsync(new
        {
            type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            title,
            status = statusCode,
            errors
        }, cancellationToken);

        return true;
    }
}