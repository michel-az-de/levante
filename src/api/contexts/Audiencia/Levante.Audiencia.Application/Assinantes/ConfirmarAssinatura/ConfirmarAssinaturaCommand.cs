namespace Levante.Audiencia.Application.Assinantes.ConfirmarAssinatura;

/// <summary>Confirma o double opt-in a partir do <paramref name="Token"/> do link enviado por e-mail.</summary>
public sealed record ConfirmarAssinaturaCommand(string Token);
