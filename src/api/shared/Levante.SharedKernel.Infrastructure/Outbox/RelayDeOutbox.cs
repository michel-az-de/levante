using System.Globalization;
using System.Text;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Driver;

namespace Levante.SharedKernel.Infrastructure.Outbox;

/// <summary>
/// Relay do Outbox: reconcilia a fila periodicamente (varre <c>outbox</c>, publica
/// cada evento no RabbitMQ e APAGA o doc). Entrega at-least-once (o consumidor
/// deduplica por eventId). Polling e escolhido no lugar de Change Streams por ser
/// robusto em qualquer topologia e a rede de seguranca contra perdas em failover;
/// a latencia (segundos) e irrelevante para notificacao. So roda quando habilitado.
/// </summary>
internal sealed class RelayDeOutbox : BackgroundService
{
    private static readonly JsonWriterSettings JsonRelaxado = new() { OutputMode = JsonOutputMode.RelaxedExtendedJson };

    private readonly IMongoCollection<OutboxDocument> _outbox;
    private readonly IPublicadorDeEventos _publicador;
    private readonly ILogger<RelayDeOutbox> _logger;
    private readonly string _exchange;
    private readonly bool _habilitado;
    private readonly TimeSpan _intervalo;

    public RelayDeOutbox(
        IMongoClient client,
        IOptions<MongoOptions> mongo,
        IOptions<RabbitMqOptions> rabbit,
        IOptions<OutboxOptions> outbox,
        IPublicadorDeEventos publicador,
        ILogger<RelayDeOutbox> logger)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(mongo);
        ArgumentNullException.ThrowIfNull(rabbit);
        ArgumentNullException.ThrowIfNull(outbox);

        _outbox = client.GetDatabase(mongo.Value.DatabaseName).GetCollection<OutboxDocument>(NomesOutbox.Collection);
        _publicador = publicador;
        _logger = logger;
        _exchange = rabbit.Value.Exchange;
        _habilitado = outbox.Value.RelayHabilitado;
        _intervalo = TimeSpan.FromSeconds(Math.Max(1, outbox.Value.IntervaloSegundos));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Decisao de ligar e em runtime (config so fica completa apos o build do host).
        if (!_habilitado)
        {
            return;
        }

        LogOutbox.RelayIniciado(_logger, _exchange);

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
            catch (Exception ex) when (ex is MongoException or RabbitMQ.Client.Exceptions.RabbitMQClientException)
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
        // Ordem de insercao: publica os mais antigos primeiro.
        var pendentes = await _outbox
            .Find(FilterDefinition<OutboxDocument>.Empty)
            .SortBy(d => d.DataCriacao)
            .ToListAsync(ct);

        foreach (var doc in pendentes)
        {
            await _publicador.PublicarAsync(doc.Tipo, doc.Id, MontarEnvelope(doc), ct);
            await _outbox.DeleteOneAsync(d => d.Id == doc.Id, ct);
            LogOutbox.EventoPublicado(_logger, doc.Tipo, doc.Id);
        }
    }

    private static ReadOnlyMemory<byte> MontarEnvelope(OutboxDocument doc)
    {
        var envelope = new BsonDocument
        {
            { "eventId", doc.Id.ToString() },
            { "tipo", doc.Tipo },
            { "versao", doc.Versao },
            { "ocorridoEm", doc.OcorridoEm.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture) },
            { "dados", doc.Dados },
        };

        return Encoding.UTF8.GetBytes(envelope.ToJson(JsonRelaxado));
    }
}
