using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Levante.SharedKernel.Infrastructure.Outbox;

/// <summary>
/// Documento do Outbox transacional (collection tecnica "outbox"). Gravado na
/// MESMA transacao que o agregado. O relay le os pendentes por <see cref="EmissionSeq"/>,
/// faz POST no Hiram e MARCA o <see cref="Status"/> (relay flag-based, ADR 0002).
/// O <see cref="Id"/> e o eventId (idempotencia no Hiram, estavel entre retentativas).
/// </summary>
internal sealed class OutboxDocument
{
    [BsonId]
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid Id { get; set; }

    /// <summary>Nome do fato de dominio (ex. "AssinaturaSolicitada"). Resolve o mapeador de emissao.</summary>
    [BsonElement("tipo")]
    public string Tipo { get; set; } = string.Empty;

    [BsonElement("versao")]
    public int Versao { get; set; }

    [BsonElement("ocorridoEm")]
    public DateTime OcorridoEm { get; set; }

    /// <summary>Payload do evento (campos do record de dominio serializados; camelCase).</summary>
    [BsonElement("dados")]
    public BsonDocument Dados { get; set; } = [];

    [BsonElement("dataCriacao")]
    public DateTime DataCriacao { get; set; }

    /// <summary>Sequencia monotonica (watermark/auditoria). Atribuida no gravador, dentro da transacao.</summary>
    [BsonElement("emissionSeq")]
    public long EmissionSeq { get; set; }

    /// <summary>Estado da emissao (<see cref="StatusEmissao"/> como int).</summary>
    [BsonElement("status")]
    public int Status { get; set; }

    [BsonElement("tentativas")]
    public int Tentativas { get; set; }

    /// <summary>Quando o relay pode tentar de novo (backoff). Null = elegivel agora.</summary>
    [BsonElement("proximaTentativaEm")]
    public DateTime? ProximaTentativaEm { get; set; }

    [BsonElement("enviadoEm")]
    public DateTime? EnviadoEm { get; set; }

    /// <summary>Ultimo erro visto (historico, NAO estado atual): nao e limpo ao virar Enviada.</summary>
    [BsonElement("erroUltimaTentativa")]
    public string? ErroUltimaTentativa { get; set; }
}
