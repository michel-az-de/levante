using Microsoft.Extensions.Hosting;

namespace Levante.Audiencia.Infrastructure.Persistence;

/// <summary>
/// Garante os indices do contexto Audiencia no boot (idempotente). Sem seed:
/// assinantes nascem do uso real, nao de sementes.
/// </summary>
internal sealed class AudienciaInicializacaoHostedService(AudienciaMongoContext contexto) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken) => contexto.EnsureIndexesAsync(cancellationToken);

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
