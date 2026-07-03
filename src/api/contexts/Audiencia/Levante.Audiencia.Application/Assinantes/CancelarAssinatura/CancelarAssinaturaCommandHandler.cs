using FluentValidation;
using Levante.Audiencia.Domain.Assinantes;
using Levante.SharedKernel;

namespace Levante.Audiencia.Application.Assinantes.CancelarAssinatura;

/// <summary>
/// Cancela a assinatura (opt-out; handler direto, GAP-F). O token de descadastro E
/// a autorizacao: invalido vira 404. Idempotente (cancelar de novo nao falha).
/// </summary>
public sealed class CancelarAssinaturaCommandHandler(
    IAssinanteRepository repositorio,
    IValidator<CancelarAssinaturaCommand> validador)
{
    public async Task<Result> Handle(CancelarAssinaturaCommand comando, CancellationToken ct)
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
            return Result.Falha(Error.NaoEncontrado("token_invalido", "Link de descadastro invalido ou expirado."));
        }

        assinante.Cancelar();
        if (assinante.Eventos.Count == 0)
        {
            // Idempotente: ja cancelado. Sem mudanca -> sem escrita nem evento.
            return Result.Ok();
        }

        await repositorio.UpdateAsync(assinante, ct);
        return Result.Ok();
    }
}
