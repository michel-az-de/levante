using Levante.Conteudo.Domain.Artigos;
using Levante.Conteudo.Infrastructure.HealthChecks;
using Levante.Conteudo.Infrastructure.Persistence;
using Levante.Conteudo.Infrastructure.Seguranca;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

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

        services.AddSingleton<IMongoClient>(sp =>
        {
            var opcoes = sp.GetRequiredService<IOptions<ConteudoMongoOptions>>().Value;
            return new MongoClient(opcoes.ConnectionString);
        });

        services.AddSingleton<ConteudoMongoContext>();
        services.AddScoped<IArtigoRepository, ArtigoRepository>();

        services.AddHealthChecks()
            .AddCheck<MongoHealthCheck>("mongo", tags: ["ready"]);

        // Inicializacao (indices + seed em dev) e guarda de privilegio minimo no boot.
        services.AddHostedService<ConteudoInicializacaoHostedService>();
        services.AddHostedService<SelfCheckPrivilegioMongoHostedService>();

        return services;
    }
}
