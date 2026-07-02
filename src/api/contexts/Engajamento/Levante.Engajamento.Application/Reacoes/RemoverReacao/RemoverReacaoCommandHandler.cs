using Levante.Engajamento.Application.Reacoes.ObterReacoesDoArtigo;
using Levante.Engajamento.Domain.Reacoes;
using Levante.SharedKernel;

namespace Levante.Engajamento.Application.Reacoes.RemoverReacao;

/// <summary>Remove a reacao do visitante (toggle off; no-op se nao existir) e devolve as contagens.</summary>
public sealed class RemoverReacaoCommandHandler(IReacaoRepository repositorio)
{
    public async Task<Result<ReacoesResponse>> Handle(RemoverReacaoCommand comando, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(comando);

        if (string.IsNullOrWhiteSpace(comando.Visitante))
        {
            return Result.Falha<ReacoesResponse>(
                Error.Validacao("validacao", "Visitante obrigatorio."));
        }

        await repositorio.RemoverAsync(comando.ArtigoId, comando.Tipo, comando.Visitante, ct);

        var resposta = await ObterReacoesDoArtigoQueryHandler.MontarRespostaAsync(
            repositorio, comando.ArtigoId, comando.Visitante, ct);

        return Result.Ok(resposta);
    }
}
