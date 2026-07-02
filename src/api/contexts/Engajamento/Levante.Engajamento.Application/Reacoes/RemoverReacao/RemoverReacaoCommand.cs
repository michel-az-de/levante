using Levante.Engajamento.Domain.Reacoes;

namespace Levante.Engajamento.Application.Reacoes.RemoverReacao;

/// <summary>Remove a reacao do visitante (toggle off). Idempotente.</summary>
public sealed record RemoverReacaoCommand(Guid ArtigoId, TipoReacao Tipo, string Visitante);
