using Levante.Conteudo.Domain.Categorias;
using Levante.SharedKernel;

namespace Levante.Conteudo.Application.Categorias.ListarCategorias;

/// <summary>Lista todas as categorias (handler direto, GAP-F). Consumido pelo editor, browse e render.</summary>
public sealed class ListarCategoriasQueryHandler(ICategoriaRepository repositorio)
{
    public async Task<Result<IReadOnlyList<CategoriaResponse>>> Handle(
        ListarCategoriasQuery query,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var categorias = await repositorio.ListAsync(ct);
        IReadOnlyList<CategoriaResponse> resposta = [.. categorias.Select(CategoriaResponse.De)];

        return Result.Ok(resposta);
    }
}
