using Levante.Engajamento.Domain.Comentarios;
using Levante.SharedKernel;

namespace Levante.Engajamento.Application.Comentarios.RejeitarComentario;

/// <summary>Rejeita o comentario (admin): estado terminal, some do publico.</summary>
public sealed class RejeitarComentarioCommandHandler(IComentarioRepository repositorio)
{
    public async Task<Result> Handle(RejeitarComentarioCommand comando, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(comando);

        var comentario = await repositorio.GetByIdAsync(comando.Id, ct);
        if (comentario is null)
        {
            return Result.Falha(
                Error.NaoEncontrado("comentario_nao_encontrado", $"Comentario '{comando.Id}' nao encontrado."));
        }

        comentario.Rejeitar();
        await repositorio.UpdateAsync(comentario, ct);
        return Result.Ok();
    }
}
