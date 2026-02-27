using Intec.Banking.FinancialInstitutions.Infrastructure.Services;
using Intec.Banking.FinancialInstitutions.Primitives;

namespace Intec.Banking.FinancialInstitutions.Infrastructure.Middleware;

/// <summary>
/// Validates that every HTTP request includes a valid X-Tenant-Id header.
/// Rejects requests without a valid tenant with 401 Unauthorized before
/// they reach any endpoint or DbContext query filter.
/// </summary>
public sealed class TenantValidationMiddleware
{
    private readonly RequestDelegate _next;

    public TenantValidationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ITenantService tenantService)
    {
        // Skip validation for OpenAPI/Scalar endpoints
        if (context.Request.Path.StartsWithSegments("/openapi") ||
            context.Request.Path.StartsWithSegments("/scalar"))
        {
            await _next(context);
            return;
        }

        if (tenantService.TenantId is null)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";

            await context.Response.WriteAsJsonAsync(new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                title = $"Missing or invalid '{TenantService.HeaderName}' header.",
                status = StatusCodes.Status401Unauthorized,
                errors = new { }
            });

            return;
        }

        await _next(context);
    }
}