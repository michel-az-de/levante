using Levante.Conteudo.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Levante.Conteudo.Infrastructure;

/// <summary>Composition root do contexto Conteudo (camada Infrastructure).</summary>
public static class DependencyInjection
{
    public static IServiceCollection AddConteudoInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddOptions<ConteudoMongoOptions>()
            .Bind(configuration.GetSection(ConteudoMongoOptions.SecaoConfig))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        // Repositorio, seeder, healthcheck e inspecao de privilegio: proximo commit.
        return services;
    }
}
