using Levante.Conteudo.Domain.Artigos;
using Levante.SharedKernel;

namespace Levante.Conteudo.Application.Artigos.ListarArtigosPublicados;

/// <summary>
/// Handler chamado direto pelo endpoint (sem lib de mediator por ora, ver GAP-F
/// em docs/mapa-tecnico.md). Retorna Result; mapeamento manual (Mapster depois).
/// </summary>
public sealed class ListarArtigosPublicadosQueryHandler(IArtigoRepository repositorio)
{
    public async Task<Result<IReadOnlyList<ArtigoResponse>>> Handle(
        ListarArtigosPublicadosQuery query,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var artigos = await repositorio.ListPublicadosAsync(ct);
        IReadOnlyList<ArtigoResponse> resposta = [.. artigos.Select(Mapear)];

        return Result.Ok(resposta);
    }

    private static ArtigoResponse Mapear(Artigo artigo) => new(
        artigo.Id,
        artigo.Titulo,
        artigo.Slug.Valor,
        artigo.Conteudo,
        artigo.DataPublicacao);
}
