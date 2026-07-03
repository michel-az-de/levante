namespace Levante.Audiencia.Application.Assinantes.CancelarAssinatura;

/// <summary>Cancela a assinatura (opt-out) a partir do <paramref name="Token"/> de descadastro.</summary>
public sealed record CancelarAssinaturaCommand(string Token);
