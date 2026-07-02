using Levante.Engajamento.Domain.Reacoes;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Levante.Engajamento.Infrastructure.Persistence;

/// <summary>
/// Modelo de persistencia (collection "reacoes"). Campos PT camelCase.
/// Isola o BSON do agregado: o driver nunca toca no <see cref="Reacao"/>.
/// </summary>
internal sealed class ReacaoDocument
{
    [BsonId]
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid Id { get; set; }

    [BsonElement("artigoId")]
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid ArtigoId { get; set; }

    [BsonElement("tipo")]
    [BsonRepresentation(BsonType.String)]
    public TipoReacao Tipo { get; set; }

    [BsonElement("visitante")]
    public string Visitante { get; set; } = string.Empty;

    [BsonElement("origemHash")]
    public string OrigemHash { get; set; } = string.Empty;

    [BsonElement("dataCriacao")]
    public DateTime DataCriacao { get; set; }

    public static ReacaoDocument DeDominio(Reacao reacao) => new()
    {
        Id = reacao.Id,
        ArtigoId = reacao.ArtigoId,
        Tipo = reacao.Tipo,
        Visitante = reacao.Visitante,
        OrigemHash = reacao.OrigemHash,
        DataCriacao = reacao.DataCriacao,
    };

    public Reacao ParaDominio() =>
        Reacao.Reconstituir(Id, ArtigoId, Tipo, Visitante, OrigemHash, DataCriacao);
}
