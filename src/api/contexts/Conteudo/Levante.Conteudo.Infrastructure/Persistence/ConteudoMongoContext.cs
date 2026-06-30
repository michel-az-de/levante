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
        Categorias = Database.GetCollection<CategoriaDocument>("categorias");
    }

    public IMongoDatabase Database { get; }

    public IMongoCollection<ArtigoDocument> Artigos { get; }

    public IMongoCollection<CategoriaDocument> Categorias { get; }

    /// <summary>Cria os indices unicos de slug (idempotente; seguro sob scale-out).</summary>
    public async Task EnsureIndexesAsync(CancellationToken ct)
    {
        var indiceArtigo = new CreateIndexModel<ArtigoDocument>(
            Builders<ArtigoDocument>.IndexKeys.Ascending(d => d.Slug),
            new CreateIndexOptions { Unique = true, Name = "ux_artigos_slug" });

        var indiceCategoria = new CreateIndexModel<CategoriaDocument>(
            Builders<CategoriaDocument>.IndexKeys.Ascending(d => d.Slug),
            new CreateIndexOptions { Unique = true, Name = "ux_categorias_slug" });

        await Artigos.Indexes.CreateOneAsync(indiceArtigo, cancellationToken: ct);
        await Categorias.Indexes.CreateOneAsync(indiceCategoria, cancellationToken: ct);
    }
}
