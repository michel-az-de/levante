namespace Levante.SharedKernel.Infrastructure.Outbox;

/// <summary>
/// Opcoes do Outbox. <see cref="RelayHabilitado"/> liga o relay (Change Stream ->
/// RabbitMQ); default desligado (testes single-node so gravam no outbox, sem publicar).
/// </summary>
public sealed class OutboxOptions
{
    public const string SecaoConfig = "Outbox";

    public bool RelayHabilitado { get; set; }

    /// <summary>Intervalo de reconciliacao do relay (varre a fila e publica pendentes).</summary>
    public int IntervaloSegundos { get; set; } = 2;
}
