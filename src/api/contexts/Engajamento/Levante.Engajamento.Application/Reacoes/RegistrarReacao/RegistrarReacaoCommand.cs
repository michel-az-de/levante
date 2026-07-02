using Levante.Engajamento.Domain.Reacoes;

namespace Levante.Engajamento.Application.Reacoes.RegistrarReacao;

/// <summary>
/// Registra a reacao do visitante ao artigo. <paramref name="Ip"/> e
/// <paramref name="UserAgent"/> chegam da borda e viram um hash (nunca sao
/// persistidos crus); <paramref name="Visitante"/> e a chave de unicidade.
/// </summary>
public sealed record RegistrarReacaoCommand(
    Guid ArtigoId,
    TipoReacao Tipo,
    string Visitante,
    string Ip,
    string UserAgent);
