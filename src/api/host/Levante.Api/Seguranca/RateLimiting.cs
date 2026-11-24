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

    /// <summary>
    /// Chave da particao do trafego interno. Nao colide com nenhum IP nem com o
    /// "desconhecido" de <see cref="OrigemDoCliente.Ip"/>.
    /// </summary>
    private const string ParticaoInterna = "interno";

    /// <summary>
    /// Teto do balde interno (SSR). Uma pagina de artigo custa 3 chamadas a API (artigo,
    /// categorias, relacionados), a home 1 e o sitemap 2 — logo 2000/min cobre ~500 renders
    /// por minuto, folgado para um site pessoal. Nao e ilimitado de proposito: um loop de
    /// render nao deve conseguir martelar a API sem freio.
    /// </summary>
    private const int PermitLimitInterno = 2000;

    /// <summary>Teto por visitante (fixed window de 1 minuto).</summary>
    private const int PermitLimitCliente = 100;

    public static IServiceCollection AddLevanteRateLimiting(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        return services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            // Duas particoes, porque as duas origens tem perfil de volume incompativel:
            // o visitante (via Caddy -> BFF) leva 100/min por IP; o SSR do Next, que chama a API
            // container-a-container e nao carrega IP de cliente, tem balde proprio e folgado.
            // Sem essa separacao TODO o trafego cai num balde unico (o IP do container do Next):
            // o limite deixa de isolar cliente algum e um visitante ruidoso derruba os demais.
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(contexto =>
            {
                var (chave, teto) = ParticaoGlobal(contexto);
                return RateLimitPartition.GetFixedWindowLimiter(
                    chave,
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = teto,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0,
                    });
            });

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

    /// <summary>
    /// Escolhe a particao do limiter global: IP do visitante quando a request atravessou o proxy
    /// da borda, ou o balde interno quando e o SSR chamando a API container-a-container.
    /// </summary>
    internal static (string Chave, int Teto) ParticaoGlobal(HttpContext contexto)
        => OrigemDoCliente.VeioDeCliente(contexto)
            ? (OrigemDoCliente.Ip(contexto), PermitLimitCliente)
            : (ParticaoInterna, PermitLimitInterno);
}
