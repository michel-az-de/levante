namespace Levante.SharedKernel.Infrastructure.Outbox;

/// <summary>Publica um evento de integracao (envelope JSON pronto) no broker.</summary>
public interface IPublicadorDeEventos
{
    /// <summary><paramref name="tipo"/> vira routing key; <paramref name="eventId"/> vira message id (idempotencia).</summary>
    Task PublicarAsync(string tipo, Guid eventId, ReadOnlyMemory<byte> corpoJson, CancellationToken ct);
}
