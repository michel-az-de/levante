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
/// Relay do Outbox: observa a collection <c>outbox</c> por Change Stream (exige
/// replica set) e publica cada evento no RabbitMQ, apagando o doc apos publicar.
/// Um sweep no start recupera o que sobrou de uma queda (at-least-once; o
/// consumidor deduplica por eventId). So roda quando o relay esta habilitado.
/// </summary>
internal sealed class RelayDeOutbox : BackgroundService
{
    private static readonly JsonWriterSettings JsonRelaxado = new() { OutputMode = JsonOutputMode.RelaxedExtendedJson };

    private readonly IMongoCollection<OutboxDocument> _outbox;
    private readonly IPublicadorDeEventos _publicador;
    private readonly ILogger<RelayDeOutbox> _logger;
    private readonly string _exchange;

    public RelayDeOutbox(
        IMongoClient client,
        IOptions<MongoOptions> mongo,
        IOptions<RabbitMqOptions> rabbit,
        IPublicadorDeEventos publicador,
        ILogger<RelayDeOutbox> logger)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(mongo);
        ArgumentNullException.ThrowIfNull(rabbit);

        _outbox = client.GetDatabase(mongo.Value.DatabaseName).GetCollection<OutboxDocument>(NomesOutbox.Collection);
        _publicador = publicador;
        _logger = logger;
        _exchange = rabbit.Value.Exchange;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        LogOutbox.RelayIniciado(_logger, _exchange);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ObservarAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex) when (ex is MongoException or RabbitMQ.Client.Exceptions.RabbitMQClientException)
            {
                LogOutbox.RelayFalhou(_logger, ex);
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }

    private async Task ObservarAsync(CancellationToken ct)
    {
        var pipeline = new EmptyPipelineDefinition<ChangeStreamDocument<OutboxDocument>>()
            .Match(c => c.OperationType == ChangeStreamOperationType.Insert);

        // Abre o stream ANTES do sweep: inserts durante o sweep tambem sao capturados
        // (duplicata tolerada; o consumidor deduplica por eventId).
        using var cursor = await _outbox.WatchAsync(pipeline, cancellationToken: ct);

        await VarrerPendentesAsync(ct);

        while (await cursor.MoveNextAsync(ct))
        {
            foreach (var mudanca in cursor.Current)
            {
                if (mudanca.FullDocument is { } doc)
                {
                    await PublicarEApagarAsync(doc, ct);
                }
            }
        }
    }

    private async Task VarrerPendentesAsync(CancellationToken ct)
    {
        var pendentes = await _outbox.Find(FilterDefinition<OutboxDocument>.Empty).ToListAsync(ct);
        foreach (var doc in pendentes)
        {
            await PublicarEApagarAsync(doc, ct);
        }
    }

    private async Task PublicarEApagarAsync(OutboxDocument doc, CancellationToken ct)
    {
        await _publicador.PublicarAsync(doc.Tipo, doc.Id, MontarEnvelope(doc), ct);
        await _outbox.DeleteOneAsync(d => d.Id == doc.Id, ct);
        LogOutbox.EventoPublicado(_logger, doc.Tipo, doc.Id);
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
