using Levante.SharedKernel.Infrastructure.Outbox;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;

namespace Levante.SharedKernel.Infrastructure;

/// <summary>
/// Registro unico do Mongo compartilhado (options + IMongoClient singleton).
/// Cada composition root de contexto chama; a partir da segunda chamada e no-op
/// (idempotente), entao a ordem de registro dos contextos nao importa.
/// </summary>
public static class MongoDependencyInjection
{
    // validarNoBoot=false (ex.: emissao do contrato OpenAPI) pula o
    // ValidateOnStart para o host subir sem Mongo/secret.
    public static IServiceCollection AddLevanteMongo(
        this IServiceCollection services,
        IConfiguration configuration,
        bool validarNoBoot = true)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        // Idempotencia: se as options ja foram vinculadas por outro contexto, no-op.
        if (services.Any(d => d.ServiceType == typeof(IConfigureOptions<MongoOptions>)))
        {
            return services;
        }

        // Guid global: o driver 3.x removeu o default implicito de representacao
        // (exigia decidir Standard/CSharpLegacy explicitamente). Documentos com
        // [BsonGuidRepresentation] por propriedade nao precisam disso, mas o
        // GridFSBucket<Guid> (id de midia) nao tem um Document proprio para
        // anotar — sem este registro global, toda serializacao de Guid crua falha
        // no boot. TryRegisterSerializer e idempotente (nao lanca se ja registrado).
        BsonSerializer.TryRegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));

        var opcoes = services.AddOptions<MongoOptions>()
            .Bind(configuration.GetSection(MongoOptions.SecaoConfig))
            .ValidateDataAnnotations();

        if (validarNoBoot)
        {
            opcoes.ValidateOnStart();
        }

        services.TryAddSingleton<IMongoClient>(sp =>
            new MongoClient(sp.GetRequiredService<IOptions<MongoOptions>>().Value.ConnectionString));

        // Sequencia monotonica (emissionSeq) usada pelo gravador dentro da transacao.
        services.TryAddSingleton<ISequenciaDeEmissao, SequenciaMongo>();

        // Gravador transacional do Outbox (agregado + eventos na mesma transacao).
        services.TryAddSingleton<IGravadorDeAgregado, GravadorDeAgregadoMongo>();

        return services;
    }
}
