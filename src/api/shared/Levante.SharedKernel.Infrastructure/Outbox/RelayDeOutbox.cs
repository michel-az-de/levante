using System.Diagnostics;
using Levante.SharedKernel.Infrastructure.Hiram;
using Levante.SharedKernel.Infrastructure.Telemetry;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Levante.SharedKernel.Infrastructure.Outbox;

/// <summary>
/// Relay do Outbox (flag-based): a cada tick le as emissoes <c>Pendente</c> elegiveis
/// (por <c>emissionSeq</c> asc, nunca cursor &gt; last — pularia late-committer), faz
/// <c>POST /v1/events</c> no Hiram e MARCA o resultado (Enviada/Falhada/Ignorada) em
/// vez de deletar. Todo o retry mora aqui (backoff por evento via <c>proximaTentativaEm</c>);
/// o cliente do Hiram nao re-tenta. So roda quando <c>Outbox:RelayHabilitado</c>.
/// </summary>
internal sealed class RelayDeOutbox : BackgroundService
{
    private readonly IMongoCollection<OutboxDocument> _outbox;
    private readonly IMapeadorDeEmissao _mapeador;
    private readonly IServiceScopeFactory _escopos;
    private readonly ILogger<RelayDeOutbox> _logger;
    private readonly bool _habilitado;
    private readonly TimeSpan _intervalo;
    private readonly int _maxTentativas;
    private readonly int _backoffBaseSegundos;
    private readonly int _backoffMaxSegundos;

    public RelayDeOutbox(
        IMongoClient client,
        IOptions<MongoOptions> mongo,
        IOptions<OutboxOptions> outbox,
        IMapeadorDeEmissao mapeador,
        IServiceScopeFactory escopos,
        ILogger<RelayDeOutbox> logger)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(mongo);
        ArgumentNullException.ThrowIfNull(outbox);

        _outbox = client.GetDatabase(mongo.Value.DatabaseName).GetCollection<OutboxDocument>(NomesOutbox.Collection);
        _mapeador = mapeador;
        _escopos = escopos;
        _logger = logger;
        _habilitado = outbox.Value.RelayHabilitado;
        _intervalo = TimeSpan.FromSeconds(Math.Max(1, outbox.Value.IntervaloSegundos));
        _maxTentativas = Math.Max(1, outbox.Value.MaxTentativas);
        _backoffBaseSegundos = Math.Max(1, outbox.Value.BackoffBaseSegundos);
        _backoffMaxSegundos = Math.Max(_backoffBaseSegundos, outbox.Value.BackoffMaxSegundos);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Decisao de ligar e em runtime (config so fica completa apos o build do host).
        if (!_habilitado)
        {
            return;
        }

        LogOutbox.RelayIniciado(_logger);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ReconciliarAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                LogOutbox.RelayFalhou(_logger, ex);
            }

            try
            {
                await Task.Delay(_intervalo, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    private async Task ReconciliarAsync(CancellationToken ct)
    {
        var agora = DateTime.UtcNow;
        var pendentes = Builders<OutboxDocument>.Filter.And(
            Builders<OutboxDocument>.Filter.Eq(d => d.Status, (int)StatusEmissao.Pendente),
            Builders<OutboxDocument>.Filter.Or(
                Builders<OutboxDocument>.Filter.Eq(d => d.ProximaTentativaEm, null),
                Builders<OutboxDocument>.Filter.Lte(d => d.ProximaTentativaEm, agora)));

        var docs = await _outbox.Find(pendentes).SortBy(d => d.EmissionSeq).ToListAsync(ct);
        if (docs.Count == 0)
        {
            return;
        }

        // Um typed client por tick (deixa o HttpClientFactory rotacionar o handler).
        using var escopo = _escopos.CreateScope();
        var hiram = escopo.ServiceProvider.GetRequiredService<IHiramClient>();

        foreach (var doc in docs)
        {
            await ProcessarAsync(hiram, doc, ct);
        }
    }

    private async Task ProcessarAsync(IHiramClient hiram, OutboxDocument doc, CancellationToken ct)
    {
        if (!_mapeador.TryMapear(doc, out var emissao))
        {
            await AtualizarAsync(doc.Id, Builders<OutboxDocument>.Update.Set(d => d.Status, (int)StatusEmissao.Ignorada), ct);
            DiagnosticoDaEmissao.Ignoradas.Add(1, Tags(doc.Tipo, "ignorada"));
            LogOutbox.EmissaoIgnorada(_logger, doc.Tipo, doc.Id);
            return;
        }

        var requisicao = new HiramEventRequest(
            emissao.EventType,
            doc.Id.ToString(),
            doc.EmissionSeq,
            new HiramRecipient(UserId: null, emissao.RecipientEmail, Phone: null),
            LogicalAlertId: null,
            Timezone: null,
            emissao.Data);

        ResultadoEmissao resultado;
        var inicio = Stopwatch.GetTimestamp();
        using (var atividade = DiagnosticoDaEmissao.Fonte.StartActivity("emissao.hiram"))
        {
            atividade?.SetTag("event_type", emissao.EventType);
            try
            {
                resultado = await hiram.EnviarAsync(requisicao, ct);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // Broken circuit / inesperado: reprocessa por backoff.
                resultado = new ResultadoEmissao(ClasseResultado.Transitoria);
                LogOutbox.EmissaoErro(_logger, ex, emissao.EventType, doc.Id);
            }
        }

        DiagnosticoDaEmissao.LatenciaRelayMs.Record(
            Stopwatch.GetElapsedTime(inicio).TotalMilliseconds, Tags(emissao.EventType, resultado.Classe.ToString()));

        switch (resultado.Classe)
        {
            case ClasseResultado.Aceito:
                await AtualizarAsync(
                    doc.Id,
                    Builders<OutboxDocument>.Update
                        .Set(d => d.Status, (int)StatusEmissao.Enviada)
                        .Set(d => d.EnviadoEm, DateTime.UtcNow),
                    ct);
                DiagnosticoDaEmissao.Enviadas.Add(1, Tags(emissao.EventType, "enviada"));
                LogOutbox.EmissaoEnviada(_logger, emissao.EventType, doc.Id);
                break;

            case ClasseResultado.Permanente:
                await FalharAsync(doc.Id, doc.Tentativas + 1, "resposta 4xx permanente do Hiram", emissao.EventType, ct);
                break;

            default:
                await AgendarRetentativaAsync(doc, resultado.RetryAfter, emissao.EventType, ct);
                break;
        }
    }

    private async Task AgendarRetentativaAsync(OutboxDocument doc, TimeSpan? retryAfter, string eventType, CancellationToken ct)
    {
        var tentativas = doc.Tentativas + 1;
        if (tentativas >= _maxTentativas)
        {
            await FalharAsync(doc.Id, tentativas, $"teto de {_maxTentativas} tentativas atingido", eventType, ct);
            return;
        }

        var espera = retryAfter ?? Backoff(tentativas);
        await AtualizarAsync(
            doc.Id,
            Builders<OutboxDocument>.Update
                .Set(d => d.Tentativas, tentativas)
                .Set(d => d.ProximaTentativaEm, DateTime.UtcNow + espera)
                .Set(d => d.ErroUltimaTentativa, "falha transitoria; reprocessando"),
            ct);
        LogOutbox.EmissaoAdiada(_logger, eventType, doc.Id, tentativas);
    }

    private async Task FalharAsync(Guid id, int tentativas, string motivo, string eventType, CancellationToken ct)
    {
        await AtualizarAsync(
            id,
            Builders<OutboxDocument>.Update
                .Set(d => d.Status, (int)StatusEmissao.Falhada)
                .Set(d => d.Tentativas, tentativas)
                .Set(d => d.ErroUltimaTentativa, motivo),
            ct);
        DiagnosticoDaEmissao.Falhadas.Add(1, Tags(eventType, "falhada"));
        LogOutbox.EmissaoFalhada(_logger, eventType, id, motivo);
    }

    private Task<UpdateResult> AtualizarAsync(Guid id, UpdateDefinition<OutboxDocument> atualizacao, CancellationToken ct) =>
        _outbox.UpdateOneAsync(d => d.Id == id, atualizacao, cancellationToken: ct);

    private TimeSpan Backoff(int tentativas)
    {
        var expoente = Math.Min(tentativas - 1, 20);
        var segundos = Math.Min(_backoffMaxSegundos, _backoffBaseSegundos * Math.Pow(2, expoente));
        return TimeSpan.FromSeconds(segundos);
    }

    private static TagList Tags(string eventType, string outcome) => new()
    {
        { "event_type", eventType },
        { "channel", "email" },
        { "outcome", outcome },
    };
}
