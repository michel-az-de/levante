using System.Threading.RateLimiting;

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

            // Readiness: por IP tambem — um cliente ruidoso nao deve esgotar o balde
            // para os demais. (AddFixedWindowLimiter cria um limiter global, nao particionado.)
            options.AddPolicy(PolicyReady, contexto =>
                RateLimitPartition.GetFixedWindowLimiter(
                    OrigemDoCliente.Ip(contexto),
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 10,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0,
                    }));

            // Login: estrita e POR IP, para frear brute-force. Um balde nao-particionado
            // (AddFixedWindowLimiter) seria global: 5 req/min de qualquer IP trancariam o
            // admin legitimo. Particionar por IP isola cada cliente (como o PolicyPublico).
            options.AddPolicy(PolicyAuth, contexto =>
                RateLimitPartition.GetFixedWindowLimiter(
                    OrigemDoCliente.Ip(contexto),
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 5,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0,
                    }));

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
