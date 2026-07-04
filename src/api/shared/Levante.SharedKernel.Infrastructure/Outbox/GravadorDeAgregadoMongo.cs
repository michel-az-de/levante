using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Levante.SharedKernel.Infrastructure.Outbox;

/// <summary>
/// Grava agregado + eventos no Outbox atomicamente. Detecta uma vez (cache) se o
/// Mongo suporta transacao (replica set): com suporte usa transacao; senao degrada
/// para escrita sequencial (dev/test single-node). O <c>emissionSeq</c> monotonico e
/// atribuido aqui, dentro do mesmo escopo transacional. Ver <see cref="IGravadorDeAgregado"/>.
/// </summary>
internal sealed class GravadorDeAgregadoMongo : IGravadorDeAgregado
{
    private static readonly JsonSerializerOptions JsonOpcoes = new(JsonSerializerDefaults.Web);

    private readonly IMongoClient _client;
    private readonly IMongoCollection<OutboxDocument> _outbox;
    private readonly IMongoDatabase _database;
    private readonly ISequenciaDeEmissao _sequencia;
    private readonly ILogger<GravadorDeAgregadoMongo> _logger;
    private readonly Lazy<Task<bool>> _suportaTransacao;

    public GravadorDeAgregadoMongo(
        IMongoClient client,
        IOptions<MongoOptions> options,
        ISequenciaDeEmissao sequencia,
        ILogger<GravadorDeAgregadoMongo> logger)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(sequencia);

        _client = client;
        _database = client.GetDatabase(options.Value.DatabaseName);
        _outbox = _database.GetCollection<OutboxDocument>(NomesOutbox.Collection);
        _sequencia = sequencia;
        _logger = logger;
        _suportaTransacao = new Lazy<Task<bool>>(DetectarSuporteTransacaoAsync);
    }

    public async Task ExecutarAsync(
        Func<IClientSessionHandle?, CancellationToken, Task> escrita,
        IReadOnlyList<IEventoDeDominio> eventos,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(escrita);
        ArgumentNullException.ThrowIfNull(eventos);

        // Sem eventos: escrita simples, sem sessao/transacao (repos sem evento nem chamam aqui).
        if (eventos.Count == 0)
        {
            await escrita(null, ct);
            return;
        }

        if (await _suportaTransacao.Value)
        {
            using var sessao = await _client.StartSessionAsync(cancellationToken: ct);
            await sessao.WithTransactionAsync(
                async (s, c) =>
                {
                    await escrita(s, c);
                    var documentos = await MapearTodosAsync(eventos, s, c);
                    await _outbox.InsertManyAsync(s, documentos, cancellationToken: c);
                    return true;
                },
                cancellationToken: ct);
            return;
        }

        // Fallback single-node (dev/test): sequencial best-effort.
        LogOutbox.GravacaoSequencial(_logger);
        await escrita(null, ct);
        var docsFallback = await MapearTodosAsync(eventos, null, ct);
        await _outbox.InsertManyAsync(docsFallback, cancellationToken: ct);
    }

    private async Task<List<OutboxDocument>> MapearTodosAsync(
        IReadOnlyList<IEventoDeDominio> eventos, IClientSessionHandle? sessao, CancellationToken ct)
    {
        var documentos = new List<OutboxDocument>(eventos.Count);
        foreach (var evento in eventos)
        {
            var seq = await _sequencia.ProximoAsync(sessao, ct);
            documentos.Add(Mapear(evento, seq));
        }

        return documentos;
    }

    private static OutboxDocument Mapear(IEventoDeDominio evento, long emissionSeq)
    {
        var agora = DateTime.UtcNow;
        // Serializa pelo tipo em runtime (IEventoDeDominio nao tem campos). Via JSON
        // limpo (Guid vira string, nao BSON binary) para o mapeador ler campos legiveis.
        var json = JsonSerializer.Serialize(evento, evento.GetType(), JsonOpcoes);
        return new OutboxDocument
        {
            Id = Guid.NewGuid(),
            Tipo = evento.GetType().Name,
            Versao = 1,
            OcorridoEm = agora,
            Dados = BsonDocument.Parse(json),
            DataCriacao = agora,
            EmissionSeq = emissionSeq,
            Status = (int)StatusEmissao.Pendente,
            Tentativas = 0,
        };
    }

    private async Task<bool> DetectarSuporteTransacaoAsync()
    {
        try
        {
            var hello = await _database.RunCommandAsync<BsonDocument>(new BsonDocument("hello", 1));
            // Membro de replica set expoe "setName"; mongos expoe msg="isdbgrid".
            var suporta = hello.Contains("setName")
                || (hello.TryGetValue("msg", out var msg) && string.Equals(msg.AsString, "isdbgrid", StringComparison.Ordinal));
            LogOutbox.CapacidadeDetectada(_logger, suporta);
            return suporta;
        }
        catch (MongoException)
        {
            return false;
        }
    }
}
