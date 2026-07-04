using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Levante.SharedKernel.Infrastructure.Telemetry;

/// <summary>
/// Fonte de traces e metricas da emissao para o Hiram. Labels alinhados ao Hiram
/// (<c>event_type</c>/<c>channel</c>/<c>outcome</c>), sem alta cardinalidade
/// (nada de tenant/event_id em serie de volume).
/// </summary>
internal static class DiagnosticoDaEmissao
{
    public const string Nome = "Levante.Outbox";

    public static readonly ActivitySource Fonte = new(Nome);

    private static readonly Meter Medidor = new(Nome);

    public static readonly Counter<long> Enviadas =
        Medidor.CreateCounter<long>("levante.emissoes.enviadas");

    public static readonly Counter<long> Falhadas =
        Medidor.CreateCounter<long>("levante.emissoes.falhadas");

    public static readonly Counter<long> Ignoradas =
        Medidor.CreateCounter<long>("levante.emissoes.ignoradas");

    public static readonly Histogram<double> LatenciaRelayMs =
        Medidor.CreateHistogram<double>("levante.emissoes.latencia_ms");
}
