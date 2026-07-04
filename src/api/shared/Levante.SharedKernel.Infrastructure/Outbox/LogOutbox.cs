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

    [LoggerMessage(Level = LogLevel.Information, Message = "Relay do Outbox iniciado (emissao HTTP para o Hiram).")]
    public static partial void RelayIniciado(ILogger logger);

    [LoggerMessage(Level = LogLevel.Information, Message = "Emissao {EventType} ({EventId}) enviada ao Hiram.")]
    public static partial void EmissaoEnviada(ILogger logger, string eventType, Guid eventId);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Emissao {Tipo} ({EventId}) sem mapeador; marcada Ignorada.")]
    public static partial void EmissaoIgnorada(ILogger logger, string tipo, Guid eventId);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Emissao {EventType} ({EventId}) adiada (tentativa {Tentativas}); reprocessando por backoff.")]
    public static partial void EmissaoAdiada(ILogger logger, string eventType, Guid eventId, int tentativas);

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "Emissao {EventType} ({EventId}) marcada Falhada: {Motivo}.")]
    public static partial void EmissaoFalhada(ILogger logger, string eventType, Guid eventId, string motivo);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Erro ao emitir {EventType} ({EventId}) para o Hiram; tratado como transitorio.")]
    public static partial void EmissaoErro(ILogger logger, Exception excecao, string eventType, Guid eventId);

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "Relay do Outbox falhou; refazendo apos backoff.")]
    public static partial void RelayFalhou(ILogger logger, Exception excecao);
}
