using Levante.Conteudo.Domain.Artigos;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Levante.Conteudo.Infrastructure.Persistence;

/// <summary>
/// Modelo de persistencia (collection "artigos"). Campos PT camelCase.
/// Isola o BSON do agregado: o driver nunca toca no <see cref="Artigo"/>.
/// </summary>
internal sealed class ArtigoDocument
{
    [BsonId]
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid Id { get; set; }

    [BsonElement("titulo")]
    public string Titulo { get; set; } = string.Empty;

    [BsonElement("slug")]
    public string Slug { get; set; } = string.Empty;

    [BsonElement("conteudo")]
    public string Conteudo { get; set; } = string.Empty;

    [BsonElement("status")]
    [BsonRepresentation(BsonType.String)]
    public StatusArtigo Status { get; set; }

    [BsonElement("dataCriacao")]
    public DateTime DataCriacao { get; set; }

    [BsonElement("dataPublicacao")]
    public DateTime? DataPublicacao { get; set; }

    public static ArtigoDocument DeDominio(Artigo artigo) => new()
    {
        Id = artigo.Id,
        Titulo = artigo.Titulo,
        Slug = artigo.Slug.Valor,
        Conteudo = artigo.Conteudo,
        Status = artigo.Status,
        DataCriacao = artigo.DataCriacao,
        DataPublicacao = artigo.DataPublicacao,
    };

    public Artigo ParaDominio() => Artigo.Reconstituir(
        Id,
        Titulo,
        new Slug(Slug),
        Conteudo,
        Status,
        DataCriacao,
        DataPublicacao);
}
