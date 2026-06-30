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

    public async Task<IReadOnlyList<Artigo>> ListTodosAsync(CancellationToken ct)
    {
        var docs = await contexto.Artigos
            .Find(FilterDefinition<ArtigoDocument>.Empty)
            .SortByDescending(d => d.DataCriacao)
            .ToListAsync(ct);

        return [.. docs.Select(d => d.ParaDominio())];
    }

    public async Task<Artigo?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var doc = await contexto.Artigos
            .Find(d => d.Id == id)
            .FirstOrDefaultAsync(ct);

        return doc?.ParaDominio();
    }

    public async Task AddAsync(Artigo artigo, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(artigo);

        try
        {
            await contexto.Artigos.InsertOneAsync(ArtigoDocument.DeDominio(artigo), options: null, ct);
        }
        catch (MongoWriteException ex) when (ex.WriteError?.Category == ServerErrorCategory.DuplicateKey)
        {
            // Indice unico de slug violado por corrida (apos a pre-checagem). Traduz
            // para excecao de dominio; a Application converte em Result "slug_em_uso".
            throw new SlugEmUsoException(artigo.Slug.Valor);
        }
    }

    public async Task UpdateAsync(Artigo artigo, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(artigo);

        try
        {
            await contexto.Artigos.ReplaceOneAsync(
                d => d.Id == artigo.Id,
                ArtigoDocument.DeDominio(artigo),
                new ReplaceOptions(),
                ct);
        }
        catch (MongoWriteException ex) when (ex.WriteError?.Category == ServerErrorCategory.DuplicateKey)
        {
            throw new SlugEmUsoException(artigo.Slug.Valor);
        }
    }
}
