namespace Levante.Engajamento.Domain.Reacoes;

/// <summary>Repositorio do agregado <see cref="Reacao"/> (contrato no Domain).</summary>
public interface IReacaoRepository
{
    /// <summary>Persiste a reacao. Lanca <see cref="ReacaoDuplicadaException"/> se ja existir (indice unico).</summary>
    Task AddAsync(Reacao reacao, CancellationToken ct);

    /// <summary>Remove a reacao do visitante (toggle off). No-op se nao existir.</summary>
    Task RemoverAsync(Guid artigoId, TipoReacao tipo, string visitante, CancellationToken ct);

    /// <summary>Conta as reacoes do artigo por tipo (read model das contagens).</summary>
    Task<IReadOnlyDictionary<TipoReacao, int>> ContarPorArtigoAsync(Guid artigoId, CancellationToken ct);

    /// <summary>Tipos que o visitante ja marcou no artigo (para o front exibir o estado preenchido).</summary>
    Task<IReadOnlyList<TipoReacao>> ListarTiposDoVisitanteAsync(Guid artigoId, string visitante, CancellationToken ct);
}
