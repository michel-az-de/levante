using Levante.Engajamento.Application.Ports;
using Levante.Engajamento.Domain.Comentarios;
using Levante.Engajamento.Domain.Reacoes;
using Levante.Engajamento.Infrastructure.Persistence;
using Levante.Engajamento.Infrastructure.Seguranca;
using Levante.SharedKernel.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Levante.Engajamento.Infrastructure;

/// <summary>Composition root do contexto Engajamento (camada Infrastructure).</summary>
public static class DependencyInjection
{
    // registrarServicosDeBoot=false (ex.: emissao do contrato OpenAPI) pula o
    // ValidateOnStart e o hosted service: o host sobe sem tocar o Mongo/segredo.
    public static IServiceCollection AddEngajamentoInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        bool registrarServicosDeBoot = true)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        // Options + IMongoClient compartilhados (registro idempotente entre contextos).
        services.AddLevanteMongo(configuration, validarNoBoot: registrarServicosDeBoot);

        var opcoes = services.AddOptions<EngajamentoOptions>()
            .Bind(configuration.GetSection(EngajamentoOptions.SecaoConfig))
            .ValidateDataAnnotations();

        if (registrarServicosDeBoot)
        {
            opcoes.ValidateOnStart();
        }

        services.AddSingleton<EngajamentoMongoContext>();
        services.AddScoped<IReacaoRepository, ReacaoRepository>();
        services.AddScoped<IComentarioRepository, ComentarioRepository>();
        services.AddSingleton<IGeradorDeOrigemHash, GeradorDeOrigemHashHmac>();

        if (registrarServicosDeBoot)
        {
            services.AddHostedService<EngajamentoInicializacaoHostedService>();
        }

        return services;
    }
}
