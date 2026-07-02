using Levante.Engajamento.Domain.Comentarios;
using Levante.SharedKernel;

namespace Levante.Engajamento.Application.Comentarios.ListarComentariosPendentes;

/// <summary>Lista a fila de moderacao (comentarios pendentes) para o admin.</summary>
public sealed class ListarComentariosPendentesQueryHandler(IComentarioRepository repositorio)
{
    public async Task<Result<IReadOnlyList<ComentarioResponse>>> Handle(
        ListarComentariosPendentesQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var comentarios = await repositorio.ListarPendentesAsync(ct);
        IReadOnlyList<ComentarioResponse> resposta = [.. comentarios.Select(ComentarioResponse.De)];

        return Result.Ok(resposta);
    }
}
