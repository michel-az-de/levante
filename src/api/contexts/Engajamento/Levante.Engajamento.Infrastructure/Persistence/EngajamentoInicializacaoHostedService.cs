using Microsoft.Extensions.Hosting;

namespace Levante.Engajamento.Infrastructure.Persistence;

/// <summary>
/// Garante os indices do contexto Engajamento no boot (idempotente). Sem seed:
/// reacoes/comentarios nascem do uso real, nao de sementes.
/// </summary>
internal sealed class EngajamentoInicializacaoHostedService(EngajamentoMongoContext contexto) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken) => contexto.EnsureIndexesAsync(cancellationToken);

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
