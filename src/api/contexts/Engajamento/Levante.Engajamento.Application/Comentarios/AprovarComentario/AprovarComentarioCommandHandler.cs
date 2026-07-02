using Levante.Engajamento.Domain.Comentarios;
using Levante.SharedKernel;

namespace Levante.Engajamento.Application.Comentarios.AprovarComentario;

/// <summary>Aprova o comentario (admin). O evento ComentarioAprovado nasce no agregado.</summary>
public sealed class AprovarComentarioCommandHandler(IComentarioRepository repositorio)
{
    public async Task<Result> Handle(AprovarComentarioCommand comando, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(comando);

        var comentario = await repositorio.GetByIdAsync(comando.Id, ct);
        if (comentario is null)
        {
            return Result.Falha(
                Error.NaoEncontrado("comentario_nao_encontrado", $"Comentario '{comando.Id}' nao encontrado."));
        }

        comentario.Aprovar();
        await repositorio.UpdateAsync(comentario, ct);
        return Result.Ok();
    }
}
