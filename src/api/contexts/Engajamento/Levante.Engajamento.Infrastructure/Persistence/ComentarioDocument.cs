using Levante.Engajamento.Domain.Comentarios;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Levante.Engajamento.Infrastructure.Persistence;

/// <summary>
/// Modelo de persistencia (collection "comentarios"). Campos PT camelCase.
/// Sem e-mail: apenas nome de exibicao e texto (superficie LGPD minima).
/// </summary>
internal sealed class ComentarioDocument
{
    [BsonId]
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid Id { get; set; }

    [BsonElement("artigoId")]
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid ArtigoId { get; set; }

    [BsonElement("artigoSlug")]
    public string ArtigoSlug { get; set; } = string.Empty;

    [BsonElement("autor")]
    public string Autor { get; set; } = string.Empty;

    [BsonElement("texto")]
    public string Texto { get; set; } = string.Empty;

    [BsonElement("status")]
    [BsonRepresentation(BsonType.String)]
    public StatusComentario Status { get; set; }

    [BsonElement("visitante")]
    public string Visitante { get; set; } = string.Empty;

    [BsonElement("origemHash")]
    public string OrigemHash { get; set; } = string.Empty;

    [BsonElement("dataCriacao")]
    public DateTime DataCriacao { get; set; }

    [BsonElement("dataModeracao")]
    [BsonIgnoreIfNull]
    public DateTime? DataModeracao { get; set; }

    public static ComentarioDocument DeDominio(Comentario comentario) => new()
    {
        Id = comentario.Id,
        ArtigoId = comentario.ArtigoId,
        ArtigoSlug = comentario.ArtigoSlug,
        Autor = comentario.Autor,
        Texto = comentario.Texto.Valor,
        Status = comentario.Status,
        Visitante = comentario.Visitante,
        OrigemHash = comentario.OrigemHash,
        DataCriacao = comentario.DataCriacao,
        DataModeracao = comentario.DataModeracao,
    };

    public Comentario ParaDominio() => Comentario.Reconstituir(
        Id,
        ArtigoId,
        ArtigoSlug,
        Autor,
        new TextoComentario(Texto),
        Status,
        Visitante,
        OrigemHash,
        DataCriacao,
        DataModeracao);
}
