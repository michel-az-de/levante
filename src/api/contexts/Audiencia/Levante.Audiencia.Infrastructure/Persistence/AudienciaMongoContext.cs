using Levante.SharedKernel.Infrastructure;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Levante.Audiencia.Infrastructure.Persistence;

/// <summary>Acesso tipado ao banco do contexto Audiencia (singleton; compartilha o cluster).</summary>
internal sealed class AudienciaMongoContext
{
    public AudienciaMongoContext(IMongoClient client, IOptions<MongoOptions> options)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(options);

        Database = client.GetDatabase(options.Value.DatabaseName);
        Assinantes = Database.GetCollection<AssinanteDocument>("assinantes");
    }

    public IMongoDatabase Database { get; }

    public IMongoCollection<AssinanteDocument> Assinantes { get; }

    /// <summary>
    /// Indices do contexto: e-mail unico (uma inscricao por e-mail) e token unico
    /// (lookup de confirmacao/descadastro). Idempotente; seguro sob scale-out.
    /// </summary>
    public async Task EnsureIndexesAsync(CancellationToken ct)
    {
        var indiceEmail = new CreateIndexModel<AssinanteDocument>(
            Builders<AssinanteDocument>.IndexKeys.Ascending(d => d.Email),
            new CreateIndexOptions { Unique = true, Name = "ux_assinantes_email" });

        var indiceToken = new CreateIndexModel<AssinanteDocument>(
            Builders<AssinanteDocument>.IndexKeys.Ascending(d => d.Token),
            new CreateIndexOptions { Unique = true, Name = "ux_assinantes_token" });

        await Assinantes.Indexes.CreateManyAsync([indiceEmail, indiceToken], ct);
    }
}
