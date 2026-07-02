namespace Levante.Engajamento.Domain.Reacoes;

/// <summary>
/// Lancada quando o visitante ja reagiu com o mesmo tipo ao mesmo artigo
/// (violacao do indice unico). Traduzida para fluxo idempotente na Application.
/// </summary>
public sealed class ReacaoDuplicadaException(Guid artigoId, TipoReacao tipo, string visitante)
    : Exception($"Visitante '{visitante}' ja reagiu com '{tipo}' ao artigo '{artigoId}'.")
{
    public Guid ArtigoId { get; } = artigoId;

    public TipoReacao Tipo { get; } = tipo;

    public string Visitante { get; } = visitante;
}
