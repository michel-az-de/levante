using Levante.Conteudo.Domain.Artigos;
using MongoDB.Driver;

namespace Levante.Conteudo.Infrastructure.Persistence;

/// <summary>Repositorio do agregado Artigo (encapsula o MongoDB.Driver).</summary>
internal sealed class ArtigoRepository(ConteudoMongoContext contexto) : IArtigoRepository
{
    public async Task<IReadOnlyList<Artigo>> ListPublicadosAsync(CancellationToken ct)
    {
        var docs = await contexto.Artigos
            .Find(d => d.Status == StatusArtigo.Publicado)
            .SortByDescending(d => d.DataPublicacao)
            .ToListAsync(ct);

        return [.. docs.Select(d => d.ParaDominio())];
    }

    public async Task<Artigo?> GetBySlugAsync(string slug, CancellationToken ct)
    {
        var doc = await contexto.Artigos
            .Find(d => d.Slug == slug)
            .FirstOrDefaultAsync(ct);

        return doc?.ParaDominio();
    }

    public Task AddAsync(Artigo artigo, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(artigo);

        return contexto.Artigos.InsertOneAsync(ArtigoDocument.DeDominio(artigo), options: null, ct);
    }
}
