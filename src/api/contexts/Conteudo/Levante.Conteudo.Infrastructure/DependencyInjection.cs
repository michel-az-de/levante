using Levante.Conteudo.Domain.Artigos;
using Levante.Conteudo.Domain.Categorias;
using Levante.Conteudo.Infrastructure.HealthChecks;
using Levante.Conteudo.Infrastructure.Persistence;
using Levante.Conteudo.Infrastructure.Seguranca;
using Levante.SharedKernel.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Levante.Conteudo.Infrastructure;

/// <summary>Composition root do contexto Conteudo (camada Infrastructure).</summary>
public static class DependencyInjection
{
    // registrarServicosDeBoot=false (ex.: emissao do contrato OpenAPI) nao
    // registra os hosted services nem o ValidateOnStart: o host sobe sem tocar
    // o Mongo.
    public static IServiceCollection AddConteudoInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        bool registrarServicosDeBoot = true)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        // Options + IMongoClient compartilhados (registro idempotente entre contextos).
        services.AddLevanteMongo(configuration, validarNoBoot: registrarServicosDeBoot);

        services.AddSingleton<ConteudoMongoContext>();
        services.AddScoped<IArtigoRepository, ArtigoRepository>();
        services.AddScoped<ICategoriaRepository, CategoriaRepository>();

        services.AddHealthChecks()
            .AddCheck<MongoHealthCheck>("mongo", tags: ["ready"]);

        if (registrarServicosDeBoot)
        {
            // Inicializacao (indices + seed em dev) e guarda de privilegio minimo no boot.
            services.AddHostedService<ConteudoInicializacaoHostedService>();
            services.AddHostedService<SelfCheckPrivilegioMongoHostedService>();
        }

        return services;
    }
}
