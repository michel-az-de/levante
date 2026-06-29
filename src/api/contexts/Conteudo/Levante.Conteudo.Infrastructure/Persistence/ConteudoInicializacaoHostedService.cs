using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Levante.Conteudo.Infrastructure.Persistence;

/// <summary>
/// Inicializacao do contexto no boot: garante indices (sempre) e aplica o
/// seed (somente fora de Producao).
/// </summary>
internal sealed class ConteudoInicializacaoHostedService(
    ConteudoMongoContext contexto,
    IHostEnvironment ambiente,
    ILogger<ConteudoInicializacaoHostedService> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await contexto.EnsureIndexesAsync(cancellationToken);

        if (ambiente.IsProduction())
        {
            return;
        }

        await ArtigoSeeder.SeedAsync(contexto, logger, cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
