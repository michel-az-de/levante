using Levante.Api.Seguranca;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace Levante.Api.Endpoints;

/// <summary>
/// Healthchecks divididos: liveness raso e publico; readiness toca o Mongo
/// (tag "ready") e tem rate limit proprio.
/// </summary>
public static class HealthEndpoints
{
    public static IEndpointRouteBuilder MapHealthEndpoints(this IEndpointRouteBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        // Liveness: o processo esta de pe. Nao toca dependencias.
        app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false })
            .AllowAnonymous()
            .WithTags("Health");

        // Readiness: pronto para servir (Mongo respondendo). Rate limit dedicado.
        app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = check => check.Tags.Contains("ready") })
            .AllowAnonymous()
            .RequireRateLimiting(RateLimiting.PolicyReady)
            .WithTags("Health");

        return app;
    }
}
