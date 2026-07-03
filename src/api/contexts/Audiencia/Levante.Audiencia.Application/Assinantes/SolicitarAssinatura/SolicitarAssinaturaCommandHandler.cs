using FluentValidation;
using Levante.Audiencia.Domain.Assinantes;
using Levante.SharedKernel;

namespace Levante.Audiencia.Application.Assinantes.SolicitarAssinatura;

/// <summary>
/// Solicita a assinatura (handler direto, GAP-F). Anti-spam: honeypot (aceita e
/// descarta em silencio). Idempotente e sem vazar: um e-mail ja cadastrado e
/// aceito sem novo evento (privacidade/LGPD — a resposta publica e sempre a mesma).
/// </summary>
public sealed class SolicitarAssinaturaCommandHandler(
    IAssinanteRepository repositorio,
    IValidator<SolicitarAssinaturaCommand> validador)
{
    public async Task<Result> Handle(SolicitarAssinaturaCommand comando, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(comando);

        // Honeypot: bots preenchem o campo escondido. Aceita em silencio e descarta.
        if (!string.IsNullOrWhiteSpace(comando.Armadilha))
        {
            return Result.Ok();
        }

        var validacao = await validador.ValidateAsync(comando, ct);
        if (!validacao.IsValid)
        {
            return Result.Falha(ErroDeValidacao.De(validacao));
        }

        var assinante = Assinante.Solicitar(new Email(comando.Email));
        try
        {
            await repositorio.AddAsync(assinante, ct);
        }
        catch (AssinanteJaExisteException)
        {
            // E-mail ja cadastrado: idempotente e sem revelar que existe (LGPD/privacidade).
        }

        return Result.Ok();
    }
}
