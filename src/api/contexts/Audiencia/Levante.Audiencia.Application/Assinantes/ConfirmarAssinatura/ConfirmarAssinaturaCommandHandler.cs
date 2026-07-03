using FluentValidation;
using Levante.Audiencia.Domain.Assinantes;
using Levante.SharedKernel;

namespace Levante.Audiencia.Application.Assinantes.ConfirmarAssinatura;

/// <summary>
/// Confirma o double opt-in (handler direto, GAP-F). O token E a autorizacao: um
/// token invalido/expirado vira 404 (sem revelar se existe). Idempotente.
/// </summary>
public sealed class ConfirmarAssinaturaCommandHandler(
    IAssinanteRepository repositorio,
    IValidator<ConfirmarAssinaturaCommand> validador)
{
    public async Task<Result> Handle(ConfirmarAssinaturaCommand comando, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(comando);

        var validacao = await validador.ValidateAsync(comando, ct);
        if (!validacao.IsValid)
        {
            return Result.Falha(ErroDeValidacao.De(validacao));
        }

        var assinante = await repositorio.GetByTokenAsync(comando.Token, ct);
        if (assinante is null)
        {
            return Result.Falha(Error.NaoEncontrado("token_invalido", "Link de confirmacao invalido ou expirado."));
        }

        assinante.Confirmar();
        if (assinante.Eventos.Count == 0)
        {
            // Idempotente: ja confirmado (ou cancelado). Sem mudanca -> sem escrita nem evento.
            return Result.Ok();
        }

        await repositorio.UpdateAsync(assinante, ct);
        return Result.Ok();
    }
}
