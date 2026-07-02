using Levante.Engajamento.Domain.Comentarios;

namespace Levante.Engajamento.Application.Comentarios;

/// <summary>Contrato de saida de comentario (publico e admin). Status vira string (convencao).</summary>
public sealed record ComentarioResponse(
    Guid Id,
    Guid ArtigoId,
    string ArtigoSlug,
    string Autor,
    string Texto,
    string Status,
    DateTime DataCriacao)
{
    public static ComentarioResponse De(Comentario comentario) => new(
        comentario.Id,
        comentario.ArtigoId,
        comentario.ArtigoSlug,
        comentario.Autor,
        comentario.Texto.Valor,
        comentario.Status.ToString(),
        comentario.DataCriacao);
}
