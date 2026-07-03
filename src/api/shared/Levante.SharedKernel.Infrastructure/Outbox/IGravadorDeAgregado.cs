using MongoDB.Driver;

namespace Levante.SharedKernel.Infrastructure.Outbox;

/// <summary>
/// Grava um agregado e os seus eventos de dominio no Outbox de forma atomica.
/// Onde o servidor suporta transacao (replica set), a escrita do agregado e a
/// insercao dos eventos acontecem na mesma transacao; caso contrario, degrada
/// para escrita sequencial best-effort (dev/test single-node).
/// </summary>
public interface IGravadorDeAgregado
{
    /// <summary>
    /// Executa <paramref name="escrita"/> (a persistencia do agregado, recebendo a
    /// sessao quando ha transacao) e grava <paramref name="eventos"/> no outbox no
    /// mesmo escopo transacional.
    /// </summary>
    Task ExecutarAsync(
        Func<IClientSessionHandle?, CancellationToken, Task> escrita,
        IReadOnlyList<IEventoDeDominio> eventos,
        CancellationToken ct);
}
