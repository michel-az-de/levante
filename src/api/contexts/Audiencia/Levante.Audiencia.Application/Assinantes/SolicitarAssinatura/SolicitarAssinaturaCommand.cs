namespace Levante.Audiencia.Application.Assinantes.SolicitarAssinatura;

/// <summary>
/// Solicita a assinatura da newsletter (double opt-in). <paramref name="Armadilha"/>
/// e o honeypot (campo escondido; preenchido = bot).
/// </summary>
public sealed record SolicitarAssinaturaCommand(string Email, string? Armadilha);
