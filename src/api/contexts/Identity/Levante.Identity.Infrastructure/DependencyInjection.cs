using Levante.Identity.Application.Ports;
using Levante.Identity.Domain.Administradores;
using Levante.Identity.Infrastructure.Persistence;
using Levante.Identity.Infrastructure.Seguranca;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

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

        var mongo = services.AddOptions<IdentityMongoOptions>()
            .Bind(configuration.GetSection(IdentityMongoOptions.SecaoConfig))
            .ValidateDataAnnotations();

        var jwt = services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SecaoConfig))
            .ValidateDataAnnotations();

        services.AddOptions<AdminSeedOptions>()
            .Bind(configuration.GetSection(AdminSeedOptions.SecaoConfig));

        if (registrarServicosDeBoot)
        {
            mongo.ValidateOnStart();
            jwt.ValidateOnStart();
        }

        services.TryAddSingleton<IMongoClient>(sp =>
            new MongoClient(sp.GetRequiredService<IOptions<IdentityMongoOptions>>().Value.ConnectionString));
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
