using Levante.SharedKernel.Infrastructure.Hiram;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Options;
using Polly;

namespace Levante.SharedKernel.Infrastructure.Outbox;

/// <summary>Registro do relay do Outbox (reconciliacao -> HTTP Hiram). Chamado uma vez pelo host.</summary>
public static class OutboxDependencyInjection
{
    /// <summary>
    /// Registra o mapeador, o cliente do Hiram e o relay fora do modo emit. A decisao de
    /// LIGAR e feita em runtime pelo proprio relay (le <c>Outbox:RelayHabilitado</c> via
    /// IOptions) — nao no registro, porque o WebApplicationFactory injeta a config DEPOIS
    /// do builder. Sem o flag, o relay sobe e encerra de imediato (nao toca Mongo/Hiram).
    /// </summary>
    public static IServiceCollection AddLevanteOutboxRelay(
        this IServiceCollection services, IConfiguration configuration, bool ligarNoBoot)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddOptions<OutboxOptions>().Bind(configuration.GetSection(OutboxOptions.SecaoConfig));

        // Modo emit do contrato OpenAPI: nao sobe o relay (nao toca Mongo/Hiram).
        if (!ligarNoBoot)
        {
            return services;
        }

        // Bind lazy (sem ValidateOnStart): com o relay desabilitado nada e exigido; habilitado
        // sem Hiram acessivel, o relay falha ao emitir e reprocessa por backoff (logado).
        services.AddOptions<NotificacoesOptions>().Bind(configuration.GetSection(NotificacoesOptions.SecaoConfig));
        services.AddOptions<HiramOptions>().Bind(configuration.GetSection(HiramOptions.SecaoConfig));
        services.AddSingleton<IMapeadorDeEmissao, MapeadorDeEmissao>();

        // Cliente tipado do Hiram: X-Api-Key vem daqui; Idempotency-Key vai por requisicao.
        // Resiliencia in-flight = timeout + circuit breaker; SEM retry (o retry mora no relay).
        services.AddHttpClient<IHiramClient, HiramClient>((sp, http) =>
            {
                var opcoes = sp.GetRequiredService<IOptions<HiramOptions>>().Value;
                if (opcoes.BaseUrl is not null)
                {
                    http.BaseAddress = opcoes.BaseUrl;
                }

                if (!string.IsNullOrEmpty(opcoes.ApiKey))
                {
                    http.DefaultRequestHeaders.Add("X-Api-Key", opcoes.ApiKey);
                }

                http.Timeout = TimeSpan.FromSeconds(15);
            })
            .AddResilienceHandler("hiram", b =>
            {
                b.AddTimeout(TimeSpan.FromSeconds(10));
                b.AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions());
            });

        services.AddHostedService<RelayDeOutbox>();

        return services;
    }
}
