using FluentValidation;
using Levante.Engajamento.Application.Ports;
using Levante.Engajamento.Application.Reacoes.ObterReacoesDoArtigo;
using Levante.Engajamento.Domain.Reacoes;
using Levante.SharedKernel;

namespace Levante.Engajamento.Application.Reacoes.RegistrarReacao;

/// <summary>
/// Registra a reacao (handler direto, GAP-F). O IP+User-Agent viram um hash
/// (nunca persistidos crus). Reagir de novo o mesmo tipo e idempotente: o
/// indice unico ja garante uma reacao por visitante/tipo. Retorna as contagens
/// atualizadas.
/// </summary>
public sealed class RegistrarReacaoCommandHandler(
    IReacaoRepository repositorio,
    IGeradorDeOrigemHash gerador,
    IValidator<RegistrarReacaoCommand> validador)
{
    public async Task<Result<ReacoesResponse>> Handle(RegistrarReacaoCommand comando, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(comando);

        var validacao = await validador.ValidateAsync(comando, ct);
        if (!validacao.IsValid)
        {
            return Result.Falha<ReacoesResponse>(ErroDeValidacao.De(validacao));
        }

        var origemHash = gerador.Gerar(comando.Ip, comando.UserAgent);
        var reacao = Reacao.Registrar(comando.ArtigoId, comando.Tipo, comando.Visitante, origemHash);

        try
        {
            await repositorio.AddAsync(reacao, ct);
        }
        catch (ReacaoDuplicadaException)
        {
            // Ja reagiu esse tipo: idempotente. Segue e devolve as contagens atuais.
        }

        var resposta = await ObterReacoesDoArtigoQueryHandler.MontarRespostaAsync(
            repositorio, comando.ArtigoId, comando.Visitante, ct);

        return Result.Ok(resposta);
    }
}
