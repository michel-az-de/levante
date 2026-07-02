using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Levante.SharedKernel.Infrastructure.Outbox;

/// <summary>Registro do relay do Outbox (Change Stream -> RabbitMQ). Chamado uma vez pelo host.</summary>
public static class OutboxDependencyInjection
{
    /// <summary>
    /// Liga o relay so quando <paramref name="ligarNoBoot"/> e <c>Outbox:RelayHabilitado</c>
    /// sao true. Sem o relay, as escritas ainda gravam no outbox (a fila acumula), mas nada
    /// e publicado — o modo dos testes single-node e da emissao do contrato.
    /// </summary>
    public static IServiceCollection AddLevanteOutboxRelay(
        this IServiceCollection services, IConfiguration configuration, bool ligarNoBoot)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddOptions<OutboxOptions>().Bind(configuration.GetSection(OutboxOptions.SecaoConfig));

        var relayLigado = ligarNoBoot
            && configuration.GetValue<bool>($"{OutboxOptions.SecaoConfig}:{nameof(OutboxOptions.RelayHabilitado)}");

        if (!relayLigado)
        {
            return services;
        }

        services.AddOptions<RabbitMqOptions>()
            .Bind(configuration.GetSection(RabbitMqOptions.SecaoConfig))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton<IPublicadorDeEventos, PublicadorRabbitMq>();
        services.AddHostedService<RelayDeOutbox>();

        return services;
    }
}
