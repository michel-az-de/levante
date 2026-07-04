using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Levante.SharedKernel.Infrastructure.Telemetry;

/// <summary>
/// OpenTelemetry do Levante: traces + metricas (+ logs quando ha coletor). O
/// <c>traceparent</c> W3C e injetado automaticamente no POST ao Hiram pela
/// instrumentacao de HttpClient (o Hiram le <c>Activity.Current.Id</c>), formando
/// um trace unico Levante -> Hiram -> provider. So exporta quando
/// <c>OTEL_EXPORTER_OTLP_ENDPOINT</c> esta setado (sem coletor, testes nao ruidam).
/// </summary>
public static class TelemetryDependencyInjection
{
    public static IServiceCollection AddLevanteTelemetry(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var exportar = !string.IsNullOrWhiteSpace(configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);
        var servico = configuration["OTEL_SERVICE_NAME"] ?? "levante-api";

        services.AddOpenTelemetry()
            .ConfigureResource(r => r.AddService(servico))
            .WithTracing(t =>
            {
                t.AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddSource(DiagnosticoDaEmissao.Nome);
                if (exportar)
                {
                    t.AddOtlpExporter();
                }
            })
            .WithMetrics(m =>
            {
                m.AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddMeter(DiagnosticoDaEmissao.Nome);
                if (exportar)
                {
                    m.AddOtlpExporter();
                }
            });

        if (exportar)
        {
            services.AddLogging(b => b.AddOpenTelemetry(o =>
            {
                o.IncludeScopes = true;
                o.AddOtlpExporter();
            }));
        }

        return services;
    }
}
