using Levante.Conteudo.Domain.Artigos;
using Levante.Conteudo.Domain.Categorias;
using MongoDB.Driver;

namespace Levante.Conteudo.Infrastructure.Persistence;

/// <summary>Repositorio do agregado Categoria (encapsula o MongoDB.Driver).</summary>
internal sealed class CategoriaRepository(ConteudoMongoContext contexto) : ICategoriaRepository
{
    public async Task<IReadOnlyList<Categoria>> ListAsync(CancellationToken ct)
    {
        var docs = await contexto.Categorias
            .Find(FilterDefinition<CategoriaDocument>.Empty)
            .SortBy(d => d.Nome)
            .ToListAsync(ct);

        return [.. docs.Select(d => d.ParaDominio())];
    }

    public async Task<Categoria?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var doc = await contexto.Categorias.Find(d => d.Id == id).FirstOrDefaultAsync(ct);
        return doc?.ParaDominio();
    }

    public async Task<Categoria?> GetBySlugAsync(string slug, CancellationToken ct)
    {
        var doc = await contexto.Categorias.Find(d => d.Slug == slug).FirstOrDefaultAsync(ct);
        return doc?.ParaDominio();
    }

    public async Task AddAsync(Categoria categoria, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(categoria);

        try
        {
            await contexto.Categorias.InsertOneAsync(CategoriaDocument.DeDominio(categoria), options: null, ct);
        }
        catch (MongoWriteException ex) when (ex.WriteError?.Category == ServerErrorCategory.DuplicateKey)
        {
            // Indice unico de slug violado por corrida; a Application converte em Result "slug_em_uso".
            throw new SlugEmUsoException(categoria.Slug.Valor);
        }
    }

    public Task UpdateAsync(Categoria categoria, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(categoria);

        // O slug e imutavel; a edicao nunca colide no indice unico.
        return contexto.Categorias.ReplaceOneAsync(
            d => d.Id == categoria.Id,
            CategoriaDocument.DeDominio(categoria),
            new ReplaceOptions(),
            ct);
    }
}
