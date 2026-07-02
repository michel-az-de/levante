using Levante.Engajamento.Domain.Comentarios;
using Levante.SharedKernel;

namespace Levante.Engajamento.Application.Comentarios.ListarComentariosAprovados;

/// <summary>Lista os comentarios aprovados de um artigo (publico).</summary>
public sealed class ListarComentariosAprovadosQueryHandler(IComentarioRepository repositorio)
{
    public async Task<Result<IReadOnlyList<ComentarioResponse>>> Handle(
        ListarComentariosAprovadosQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var comentarios = await repositorio.ListarAprovadosPorArtigoAsync(query.ArtigoId, ct);
        IReadOnlyList<ComentarioResponse> resposta = [.. comentarios.Select(ComentarioResponse.De)];

        return Result.Ok(resposta);
    }
}
