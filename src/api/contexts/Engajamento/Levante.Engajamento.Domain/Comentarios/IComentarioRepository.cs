namespace Levante.Engajamento.Domain.Comentarios;

/// <summary>Repositorio do agregado <see cref="Comentario"/> (contrato no Domain).</summary>
public interface IComentarioRepository
{
    Task AddAsync(Comentario comentario, CancellationToken ct);

    Task<Comentario?> GetByIdAsync(Guid id, CancellationToken ct);

    Task UpdateAsync(Comentario comentario, CancellationToken ct);

    /// <summary>Comentarios aprovados de um artigo (leitura publica), mais antigos primeiro.</summary>
    Task<IReadOnlyList<Comentario>> ListarAprovadosPorArtigoAsync(Guid artigoId, CancellationToken ct);

    /// <summary>Fila de moderacao: comentarios pendentes (todos os artigos), mais antigos primeiro.</summary>
    Task<IReadOnlyList<Comentario>> ListarPendentesAsync(CancellationToken ct);
}
