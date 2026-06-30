using Levante.Conteudo.Domain.Artigos;
using Levante.Conteudo.Domain.Categorias;
using Levante.SharedKernel;

namespace Levante.Conteudo.Application.Artigos.ListarArtigosPublicadosPorCategoria;

/// <summary>
/// Lista os artigos publicados de uma categoria (handler direto, GAP-F). Categoria
/// inexistente retorna falha (mapeada para 404). So expoe publicados.
/// </summary>
public sealed class ListarArtigosPublicadosPorCategoriaQueryHandler(
    IArtigoRepository artigos,
    ICategoriaRepository categorias)
{
    public async Task<Result<IReadOnlyList<ArtigoResponse>>> Handle(
        ListarArtigosPublicadosPorCategoriaQuery query,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var categoria = await categorias.GetBySlugAsync(query.Slug, ct);
        if (categoria is null)
        {
            return Result.Falha<IReadOnlyList<ArtigoResponse>>(
                new Error("categoria_nao_encontrada", $"Categoria '{query.Slug}' nao encontrada."));
        }

        var publicados = await artigos.ListPublicadosAsync(ct);
        IReadOnlyList<ArtigoResponse> resposta =
            [.. publicados.Where(a => a.CategoriaId == categoria.Id).Select(ArtigoResponse.DeArtigo)];

        return Result.Ok(resposta);
    }
}
