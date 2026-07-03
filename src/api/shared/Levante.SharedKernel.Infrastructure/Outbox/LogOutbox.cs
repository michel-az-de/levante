using Microsoft.Extensions.Logging;

namespace Levante.SharedKernel.Infrastructure.Outbox;

/// <summary>Logs de alta performance (source-generated) do Outbox/relay.</summary>
internal static partial class LogOutbox
{
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Outbox: servidor Mongo suporta transacoes (replica set) = {Suporta}.")]
    public static partial void CapacidadeDetectada(ILogger logger, bool suporta);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Outbox sem transacao (Mongo single-node): gravando agregado e evento em sequencia " +
                  "(best-effort). Em producao exige-se replica set.")]
    public static partial void GravacaoSequencial(ILogger logger);

    [LoggerMessage(Level = LogLevel.Information, Message = "Relay do Outbox iniciado (exchange {Exchange}).")]
    public static partial void RelayIniciado(ILogger logger, string exchange);

    [LoggerMessage(Level = LogLevel.Information, Message = "Relay publicou evento {Tipo} ({EventId}).")]
    public static partial void EventoPublicado(ILogger logger, string tipo, Guid eventId);

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "Relay do Outbox falhou; refazendo apos backoff.")]
    public static partial void RelayFalhou(ILogger logger, Exception excecao);
}
