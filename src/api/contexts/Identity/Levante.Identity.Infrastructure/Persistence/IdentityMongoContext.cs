using Levante.SharedKernel.Infrastructure;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Levante.Identity.Infrastructure.Persistence;

/// <summary>Acesso tipado a collection de administradores.</summary>
internal sealed class IdentityMongoContext
{
    public IdentityMongoContext(IMongoClient client, IOptions<MongoOptions> options)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(options);

        Database = client.GetDatabase(options.Value.DatabaseName);
        Administradores = Database.GetCollection<AdministradorDocument>("administradores");
    }

    public IMongoDatabase Database { get; }

    public IMongoCollection<AdministradorDocument> Administradores { get; }

    /// <summary>Cria o indice unico de email (idempotente).</summary>
    public Task EnsureIndexesAsync(CancellationToken ct)
    {
        var indice = new CreateIndexModel<AdministradorDocument>(
            Builders<AdministradorDocument>.IndexKeys.Ascending(d => d.Email),
            new CreateIndexOptions { Unique = true, Name = "ux_administradores_email" });

        return Administradores.Indexes.CreateOneAsync(indice, cancellationToken: ct);
    }
}
