using Ardalis.GuardClauses;
using FluentValidation;
using Intec.Banking.FinancialInstitutions.Primitives;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;

namespace Intec.Banking.FinancialInstitutions.Infrastructure.Exceptions;

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
        // ── LOGGING ──────────────────────────────────────────────────────────
        switch (exception)
        {
            case ValidationException:
                _logger.LogInformation(exception, "Validation failed: {Message}", exception.Message);
                break;

            case DomainException:
            case ArgumentException:
            case BadHttpRequestException:
                _logger.LogWarning(exception, "Business rule violation: {Message}", exception.Message);
                break;

            case DbUpdateConcurrencyException:
                _logger.LogWarning(exception, "Concurrency conflict: {Message}", exception.Message);
                break;

            case DbUpdateException dbEx when IsUniqueConstraintViolation(dbEx):
                _logger.LogWarning(exception, "Unique constraint violation: {Message}", exception.Message);
                break;

            case UnauthorizedAccessException:
                _logger.LogWarning(exception, "Unauthorized: {Message}", exception.Message);
                break;

            case NotFoundException:
                _logger.LogWarning(exception, "Not found: {Message}", exception.Message);
                break;

            default:
                _logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);
                break;
        }

        // ── RESPONSE MAPPING ─────────────────────────────────────────────────
        var (statusCode, title, errors) = exception switch
        {
            // 400 — Validation errors (FluentValidation)
            ValidationException validationException => (
                StatusCodes.Status400BadRequest,
                "One or more validation errors occurred.",
                validationException.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToArray())),

            // 400 — Domain business rule violations
            DomainException domainException => (
                StatusCodes.Status400BadRequest,
                domainException.Message,
                new Dictionary<string, string[]>()),

            // 400 — Domain invariants thrown as ArgumentException
            ArgumentException argumentException => (
                StatusCodes.Status400BadRequest,
                argumentException.Message,
                new Dictionary<string, string[]>()),

            // 400 — Missing or malformed query/route parameters
            BadHttpRequestException badHttpRequestException => (
                StatusCodes.Status400BadRequest,
                badHttpRequestException.Message,
                new Dictionary<string, string[]>()),

            // 401 — Missing or invalid X-Tenant-Id header
            UnauthorizedAccessException unauthorizedException => (
                StatusCodes.Status401Unauthorized,
                unauthorizedException.Message,
                new Dictionary<string, string[]>()),

            // 404 — Resource not found
            NotFoundException => (
                StatusCodes.Status404NotFound,
                "The requested resource was not found.",
                new Dictionary<string, string[]>()),

            // 409 — Optimistic concurrency conflict
            DbUpdateConcurrencyException concurrencyException => (
                StatusCodes.Status409Conflict,
                concurrencyException.Message,
                new Dictionary<string, string[]>()),

            // 409 — Unique constraint violation (SwiftBic, TaxId)
            DbUpdateException dbEx when IsUniqueConstraintViolation(dbEx) => (
                StatusCodes.Status409Conflict,
                "A record with the same unique values already exists.",
                new Dictionary<string, string[]>()),

            // 500 — Unexpected errors
            _ => (
                StatusCodes.Status500InternalServerError,
                "An unexpected error occurred while processing your request.",
                new Dictionary<string, string[]>())
        };

        httpContext.Response.StatusCode = statusCode;

        await httpContext.Response.WriteAsJsonAsync(new
        {
            type = $"https://httpstatuses.com/{statusCode}",
            title,
            status = statusCode,
            traceId = httpContext.TraceIdentifier,
            errors
        }, cancellationToken);

        return true;
    }

    // ── HELPERS ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Detects PostgreSQL unique constraint violation (error code 23505).
    /// </summary>
    private static bool IsUniqueConstraintViolation(DbUpdateException ex) =>
        ex.InnerException?.Message.Contains("23505") == true ||
        ex.InnerException?.Message.Contains("duplicate key") == true;
}