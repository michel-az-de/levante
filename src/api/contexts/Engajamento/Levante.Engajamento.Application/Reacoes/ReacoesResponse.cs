using Levante.Engajamento.Domain.Reacoes;

namespace Levante.Engajamento.Application.Reacoes;

/// <summary>
/// Contagem de reacoes de um artigo por tipo + os tipos que o visitante atual
/// ja marcou (nomes do enum, ex. "Curtir"; para o front destacar os botoes ativos).
/// </summary>
public sealed record ReacoesResponse(int Curtir, int Amei, int Relevante, IReadOnlyList<string> Minhas)
{
    public static ReacoesResponse De(
        IReadOnlyDictionary<TipoReacao, int> contagens, IReadOnlyList<TipoReacao> minhas) =>
        new(
            contagens.GetValueOrDefault(TipoReacao.Curtir),
            contagens.GetValueOrDefault(TipoReacao.Amei),
            contagens.GetValueOrDefault(TipoReacao.Relevante),
            [.. minhas.Select(t => t.ToString())]);
}
