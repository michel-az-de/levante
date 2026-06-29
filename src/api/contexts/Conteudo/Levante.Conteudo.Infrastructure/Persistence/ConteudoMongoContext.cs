using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Levante.Conteudo.Infrastructure.Persistence;

/// <summary>Acesso tipado ao banco do contexto Conteudo (singleton).</summary>
internal sealed class ConteudoMongoContext
{
    public ConteudoMongoContext(IMongoClient client, IOptions<ConteudoMongoOptions> options)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(options);

        Database = client.GetDatabase(options.Value.DatabaseName);
        Artigos = Database.GetCollection<ArtigoDocument>("artigos");
    }

    public IMongoDatabase Database { get; }

    public IMongoCollection<ArtigoDocument> Artigos { get; }

    /// <summary>Cria o indice unico de slug (idempotente; seguro sob scale-out).</summary>
    public Task EnsureIndexesAsync(CancellationToken ct)
    {
        var indice = new CreateIndexModel<ArtigoDocument>(
            Builders<ArtigoDocument>.IndexKeys.Ascending(d => d.Slug),
            new CreateIndexOptions { Unique = true, Name = "ux_artigos_slug" });

        return Artigos.Indexes.CreateOneAsync(indice, cancellationToken: ct);
    }
}
