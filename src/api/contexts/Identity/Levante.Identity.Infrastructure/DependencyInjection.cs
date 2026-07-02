using Levante.Identity.Application.Ports;
using Levante.Identity.Domain.Administradores;
using Levante.Identity.Infrastructure.Persistence;
using Levante.Identity.Infrastructure.Seguranca;
using Levante.SharedKernel.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Levante.Identity.Infrastructure;

/// <summary>Composition root do contexto Identity.</summary>
public static class DependencyInjection
{
    // registrarServicosDeBoot=false (emissao do contrato OpenAPI) pula
    // ValidateOnStart e hosted services, para o host subir sem Mongo/secret.
    public static IServiceCollection AddIdentityInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        bool registrarServicosDeBoot = true)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        // Options + IMongoClient compartilhados (registro idempotente entre contextos).
        services.AddLevanteMongo(configuration, validarNoBoot: registrarServicosDeBoot);

        var jwt = services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SecaoConfig))
            .ValidateDataAnnotations();

        services.AddOptions<AdminSeedOptions>()
            .Bind(configuration.GetSection(AdminSeedOptions.SecaoConfig));

        if (registrarServicosDeBoot)
        {
            jwt.ValidateOnStart();
        }

        services.AddSingleton<IdentityMongoContext>();
        services.AddScoped<IAdministradorRepository, AdministradorRepository>();
        services.AddSingleton<IHashDeSenha, HashDeSenhaPasswordHasher>();
        services.AddSingleton<IGeradorDeToken, GeradorDeTokenJwt>();

        if (registrarServicosDeBoot)
        {
            services.AddHostedService<IdentityInicializacaoHostedService>();
        }

        return services;
    }
}
