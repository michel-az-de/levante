using Levante.Conteudo.Domain.Artigos;
using Levante.SharedKernel;

namespace Levante.Conteudo.Application.Artigos.ObterArtigoPorSlug;

/// <summary>
/// Handler chamado direto (sem mediator, GAP-F). So expoe artigos publicados;
/// rascunho/arquivado retornam falha (mapeada para 404 no endpoint).
/// </summary>
public sealed class ObterArtigoPorSlugQueryHandler(IArtigoRepository repositorio)
{
    public async Task<Result<ArtigoResponse>> Handle(ObterArtigoPorSlugQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var artigo = await repositorio.GetBySlugAsync(query.Slug, ct);

        if (artigo is null || artigo.Status != StatusArtigo.Publicado)
        {
            return Result.Falha<ArtigoResponse>(
                Error.NaoEncontrado("artigo_nao_encontrado", $"Artigo '{query.Slug}' nao encontrado."));
        }

        return Result.Ok(ArtigoResponse.DeArtigo(artigo));
    }
}
