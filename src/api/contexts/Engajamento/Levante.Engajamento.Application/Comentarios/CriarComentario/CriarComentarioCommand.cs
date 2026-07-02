namespace Levante.Engajamento.Application.Comentarios.CriarComentario;

/// <summary>
/// Cria um comentario (nasce Pendente). <paramref name="Armadilha"/> e o honeypot
/// (campo escondido; preenchido = bot). <paramref name="Ip"/>/<paramref name="UserAgent"/>
/// viram hash (nunca persistidos crus); <paramref name="Visitante"/> vem do cookie.
/// </summary>
public sealed record CriarComentarioCommand(
    Guid ArtigoId,
    string ArtigoSlug,
    string Autor,
    string Texto,
    string Visitante,
    string Ip,
    string UserAgent,
    string? Armadilha);
