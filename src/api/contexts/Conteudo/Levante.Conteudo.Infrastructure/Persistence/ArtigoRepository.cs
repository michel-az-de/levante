using Levante.Conteudo.Domain.Artigos;
using Levante.SharedKernel.Infrastructure.Outbox;
using MongoDB.Driver;

namespace Levante.Conteudo.Infrastructure.Persistence;

/// <summary>
/// Repositorio do agregado Artigo (encapsula o MongoDB.Driver). As escritas passam
/// pelo <see cref="IGravadorDeAgregado"/>: os eventos de dominio do artigo (ex.
/// ArtigoPublicado) vao ao Outbox na mesma transacao da gravacao.
/// </summary>
internal sealed class ArtigoRepository(ConteudoMongoContext contexto, IGravadorDeAgregado gravador) : IArtigoRepository
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

        var documento = ArtigoDocument.DeDominio(artigo);
        try
        {
            await gravador.ExecutarAsync(
                async (sessao, c) =>
                {
                    if (sessao is null)
                    {
                        await contexto.Artigos.InsertOneAsync(documento, options: null, c);
                    }
                    else
                    {
                        await contexto.Artigos.InsertOneAsync(sessao, documento, options: null, c);
                    }
                },
                artigo.Eventos,
                ct);
        }
        catch (MongoWriteException ex) when (ex.WriteError?.Category == ServerErrorCategory.DuplicateKey)
        {
            // Indice unico de slug violado por corrida (apos a pre-checagem). Traduz
            // para excecao de dominio; a Application converte em Result "slug_em_uso".
            throw new SlugEmUsoException(artigo.Slug.Valor);
        }

        artigo.LimparEventos();
    }

    public async Task UpdateAsync(Artigo artigo, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(artigo);

        var documento = ArtigoDocument.DeDominio(artigo);
        try
        {
            await gravador.ExecutarAsync(
                async (sessao, c) =>
                {
                    if (sessao is null)
                    {
                        await contexto.Artigos.ReplaceOneAsync(d => d.Id == documento.Id, documento, new ReplaceOptions(), c);
                    }
                    else
                    {
                        await contexto.Artigos.ReplaceOneAsync(sessao, d => d.Id == documento.Id, documento, new ReplaceOptions(), c);
                    }
                },
                artigo.Eventos,
                ct);
        }
        catch (MongoWriteException ex) when (ex.WriteError?.Category == ServerErrorCategory.DuplicateKey)
        {
            throw new SlugEmUsoException(artigo.Slug.Valor);
        }

        artigo.LimparEventos();
    }
}
