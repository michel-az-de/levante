using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Levante.SharedKernel.Infrastructure.Outbox;

/// <summary>
/// Documento do Outbox transacional (collection tecnica "outbox"). Gravado na
/// MESMA transacao que o agregado; o relay observa a collection via Change Stream
/// e publica no RabbitMQ. O <see cref="Id"/> e o eventId (idempotencia no consumidor).
/// </summary>
internal sealed class OutboxDocument
{
    [BsonId]
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid Id { get; set; }

    /// <summary>Nome do fato (ex. "ArtigoPublicado"). Vira a routing key no RabbitMQ.</summary>
    [BsonElement("tipo")]
    public string Tipo { get; set; } = string.Empty;

    [BsonElement("versao")]
    public int Versao { get; set; }

    [BsonElement("ocorridoEm")]
    public DateTime OcorridoEm { get; set; }

    /// <summary>Payload do evento (campos do record de dominio serializados).</summary>
    [BsonElement("dados")]
    public BsonDocument Dados { get; set; } = [];

    [BsonElement("dataCriacao")]
    public DateTime DataCriacao { get; set; }
}
