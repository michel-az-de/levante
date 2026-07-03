using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Levante.SharedKernel.Infrastructure.Outbox;

/// <summary>Registro do relay do Outbox (reconciliacao -> RabbitMQ). Chamado uma vez pelo host.</summary>
public static class OutboxDependencyInjection
{
    /// <summary>
    /// Registra o relay fora do modo emit do contrato. A decisao de LIGAR e feita em
    /// runtime pelo proprio relay (le <c>Outbox:RelayHabilitado</c> via IOptions) — nao
    /// no registro, porque o WebApplicationFactory injeta a config DEPOIS do builder.
    /// Sem o flag, o relay sobe e encerra de imediato (nao toca Rabbit).
    /// </summary>
    public static IServiceCollection AddLevanteOutboxRelay(
        this IServiceCollection services, IConfiguration configuration, bool ligarNoBoot)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddOptions<OutboxOptions>().Bind(configuration.GetSection(OutboxOptions.SecaoConfig));

        // Modo emit do contrato OpenAPI: nao sobe o relay (nao toca Mongo/Rabbit).
        if (!ligarNoBoot)
        {
            return services;
        }

        // Bind lazy (sem ValidateOnStart): quando desabilitado, o relay nunca usa o Rabbit,
        // e a fixture geral (sem config de Rabbit) sobe normalmente. Habilitado sem broker,
        // o relay falha ao publicar e refaz com backoff (logado).
        services.AddOptions<RabbitMqOptions>().Bind(configuration.GetSection(RabbitMqOptions.SecaoConfig));
        services.AddSingleton<IPublicadorDeEventos, PublicadorRabbitMq>();
        services.AddHostedService<RelayDeOutbox>();

        return services;
    }
}
