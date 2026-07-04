namespace Levante.SharedKernel.Infrastructure.Outbox;

/// <summary>
/// Opcoes do Outbox/relay. <see cref="RelayHabilitado"/> liga o relay (emissao HTTP
/// para o Hiram); default desligado (testes single-node so gravam no outbox).
/// </summary>
public sealed class OutboxOptions
{
    public const string SecaoConfig = "Outbox";

    public bool RelayHabilitado { get; set; }

    /// <summary>Intervalo de reconciliacao do relay (varre pendentes elegiveis e emite).</summary>
    public int IntervaloSegundos { get; set; } = 2;

    /// <summary>Teto de tentativas antes de marcar a emissao como <c>Falhada</c>.</summary>
    public int MaxTentativas { get; set; } = 10;

    /// <summary>Base do backoff exponencial por evento (segundos): <c>base * 2^(tentativas-1)</c>, com teto.</summary>
    public int BackoffBaseSegundos { get; set; } = 5;

    /// <summary>Teto do backoff por evento (segundos).</summary>
    public int BackoffMaxSegundos { get; set; } = 300;
}
