using Levante.SharedKernel.Infrastructure;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;

namespace Levante.Conteudo.Infrastructure.Persistence;

/// <summary>Acesso tipado ao banco do contexto Conteudo (singleton).</summary>
internal sealed class ConteudoMongoContext
{
    public ConteudoMongoContext(IMongoClient client, IOptions<MongoOptions> options)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(options);

        Database = client.GetDatabase(options.Value.DatabaseName);
        Artigos = Database.GetCollection<ArtigoDocument>("artigos");
        Categorias = Database.GetCollection<CategoriaDocument>("categorias");
        // Bucket com id proprio (Guid, nao ObjectId) para casar com a rota publica
        // /midias/{id:guid}. Exige o GuidSerializer registrado globalmente (ver
        // MongoDependencyInjection.AddLevanteMongo) — o bucket nao tem um Document
        // com [BsonGuidRepresentation] proprio como Artigo/Categoria.
        Midias = new GridFSBucket<Guid>(Database, new GridFSBucketOptions { BucketName = "midias" });
    }

    public IMongoDatabase Database { get; }

    public IMongoCollection<ArtigoDocument> Artigos { get; }

    public IMongoCollection<CategoriaDocument> Categorias { get; }

    public GridFSBucket<Guid> Midias { get; }

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
