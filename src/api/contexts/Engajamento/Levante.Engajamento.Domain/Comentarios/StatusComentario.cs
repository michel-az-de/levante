namespace Levante.Engajamento.Domain.Comentarios;

/// <summary>Ciclo de moderacao de um <see cref="Comentario"/>.</summary>
public enum StatusComentario
{
    Pendente = 0,
    Aprovado = 1,
    Rejeitado = 2,
}
