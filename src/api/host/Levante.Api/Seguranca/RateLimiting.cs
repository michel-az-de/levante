using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace Levante.Api.Seguranca;

/// <summary>
/// Rate limiting: limiter global por IP (fixed window) e uma policy "ready"
/// mais estrita para o readiness, que toca o Mongo.
/// </summary>
public static class RateLimiting
{
    public const string PolicyReady = "ready";
    public const string PolicyAuth = "auth";
    public const string PolicyPublico = "publico";

    public static IServiceCollection AddLevanteRateLimiting(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        return services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    context.Connection.RemoteIpAddress?.ToString() ?? "desconhecido",
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 100,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0,
                    }));

            options.AddFixedWindowLimiter(PolicyReady, limiter =>
            {
                limiter.PermitLimit = 10;
                limiter.Window = TimeSpan.FromMinutes(1);
                limiter.QueueLimit = 0;
            });

            // Login: estrita, para frear brute-force (complementa o lockout por conta).
            options.AddFixedWindowLimiter(PolicyAuth, limiter =>
            {
                limiter.PermitLimit = 5;
                limiter.Window = TimeSpan.FromMinutes(1);
                limiter.QueueLimit = 0;
            });

            // Escrita publica (reacoes/comentarios): por IP do cliente (X-Forwarded-For
            // posto pelo BFF), para frear spam sem afetar outros visitantes.
            options.AddPolicy(PolicyPublico, contexto =>
                RateLimitPartition.GetFixedWindowLimiter(
                    OrigemDoCliente.Ip(contexto),
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 20,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0,
                    }));
        });
    }
}
