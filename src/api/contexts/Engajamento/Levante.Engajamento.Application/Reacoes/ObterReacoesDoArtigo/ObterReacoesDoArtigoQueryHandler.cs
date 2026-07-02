using Levante.Engajamento.Domain.Reacoes;
using Levante.SharedKernel;

namespace Levante.Engajamento.Application.Reacoes.ObterReacoesDoArtigo;

/// <summary>Le as contagens de reacao do artigo e os tipos ja marcados pelo visitante.</summary>
public sealed class ObterReacoesDoArtigoQueryHandler(IReacaoRepository repositorio)
{
    public async Task<Result<ReacoesResponse>> Handle(ObterReacoesDoArtigoQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var resposta = await MontarRespostaAsync(repositorio, query.ArtigoId, query.Visitante, ct);
        return Result.Ok(resposta);
    }

    /// <summary>Monta a resposta de contagens (reusado pelos handlers de registrar/remover).</summary>
    internal static async Task<ReacoesResponse> MontarRespostaAsync(
        IReacaoRepository repositorio, Guid artigoId, string visitante, CancellationToken ct)
    {
        var contagens = await repositorio.ContarPorArtigoAsync(artigoId, ct);

        IReadOnlyList<TipoReacao> minhas = string.IsNullOrWhiteSpace(visitante)
            ? []
            : await repositorio.ListarTiposDoVisitanteAsync(artigoId, visitante, ct);

        return ReacoesResponse.De(contagens, minhas);
    }
}
