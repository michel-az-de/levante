using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Levante.SharedKernel.Infrastructure.Outbox;

/// <summary>Fonte de <c>emissionSeq</c> monotonico por tenant (o Mongo nao tem bigserial).</summary>
internal interface ISequenciaDeEmissao
{
    /// <summary>
    /// Proximo numero da sequencia. Quando <paramref name="sessao"/> vem preenchida, o
    /// incremento roda DENTRO da transacao do agregado (atomico com a gravacao); num
    /// abort a alocacao e desfeita, entao a sequencia nao "vaza" numeros perdidos.
    /// </summary>
    Task<long> ProximoAsync(IClientSessionHandle? sessao, CancellationToken ct);
}

/// <summary>
/// Contador monotonico via <c>findOneAndUpdate($inc)</c> num doc unico da collection
/// <c>sequencias</c>. Ordem de <c>emissionSeq</c> e de AUDITORIA, nao de envio: um
/// late-committer com seq menor pode commitar depois — por isso o relay le por
/// <c>status == Pendente</c> (nunca cursor <c>&gt; last</c>), pegando-o no proximo tick.
/// </summary>
internal sealed class SequenciaMongo : ISequenciaDeEmissao
{
    private static readonly FilterDefinition<BsonDocument> FiltroEmissao =
        Builders<BsonDocument>.Filter.Eq("_id", NomesOutbox.ChaveEmissionSeq);

    private static readonly UpdateDefinition<BsonDocument> Incremento =
        Builders<BsonDocument>.Update.Inc("valor", 1L);

    private static readonly FindOneAndUpdateOptions<BsonDocument> Opcoes = new()
    {
        IsUpsert = true,
        ReturnDocument = ReturnDocument.After,
    };

    private readonly IMongoCollection<BsonDocument> _sequencias;

    public SequenciaMongo(IMongoClient client, IOptions<MongoOptions> options)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(options);

        _sequencias = client.GetDatabase(options.Value.DatabaseName)
            .GetCollection<BsonDocument>(NomesOutbox.Sequencias);
    }

    public async Task<long> ProximoAsync(IClientSessionHandle? sessao, CancellationToken ct)
    {
        var doc = sessao is null
            ? await _sequencias.FindOneAndUpdateAsync(FiltroEmissao, Incremento, Opcoes, ct)
            : await _sequencias.FindOneAndUpdateAsync(sessao, FiltroEmissao, Incremento, Opcoes, ct);

        return doc["valor"].AsInt64;
    }
}
