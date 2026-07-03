using Levante.Audiencia.Domain.Assinantes;
using Levante.Audiencia.Infrastructure.Persistence;
using Levante.SharedKernel.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Levante.Audiencia.Infrastructure;

/// <summary>Composition root do contexto Audiencia (camada Infrastructure).</summary>
public static class DependencyInjection
{
    // registrarServicosDeBoot=false (ex.: emissao do contrato OpenAPI) pula o hosted
    // service: o host sobe sem tocar o Mongo. O contexto nao tem segredo proprio.
    public static IServiceCollection AddAudienciaInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        bool registrarServicosDeBoot = true)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        // Options + IMongoClient + IGravadorDeAgregado compartilhados (registro idempotente).
        services.AddLevanteMongo(configuration, validarNoBoot: registrarServicosDeBoot);

        services.AddSingleton<AudienciaMongoContext>();
        services.AddScoped<IAssinanteRepository, AssinanteRepository>();

        if (registrarServicosDeBoot)
        {
            services.AddHostedService<AudienciaInicializacaoHostedService>();
        }

        return services;
    }
}
