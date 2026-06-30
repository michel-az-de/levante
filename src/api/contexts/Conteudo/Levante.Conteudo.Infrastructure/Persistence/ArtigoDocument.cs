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

    [BsonElement("resumo")]
    public string Resumo { get; set; } = string.Empty;

    [BsonElement("conteudo")]
    public string Conteudo { get; set; } = string.Empty;

    [BsonElement("status")]
    [BsonRepresentation(BsonType.String)]
    public StatusArtigo Status { get; set; }

    [BsonElement("dataCriacao")]
    public DateTime DataCriacao { get; set; }

    [BsonElement("dataPublicacao")]
    public DateTime? DataPublicacao { get; set; }

    [BsonElement("metaTitulo")]
    [BsonIgnoreIfNull]
    public string? MetaTitulo { get; set; }

    [BsonElement("metaDescricao")]
    [BsonIgnoreIfNull]
    public string? MetaDescricao { get; set; }

    [BsonElement("imagemOgUrl")]
    [BsonIgnoreIfNull]
    public string? ImagemOgUrl { get; set; }

    [BsonElement("categoriaId")]
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    [BsonIgnoreIfNull]
    public Guid? CategoriaId { get; set; }

    [BsonElement("tags")]
    public List<string> Tags { get; set; } = [];

    public static ArtigoDocument DeDominio(Artigo artigo) => new()
    {
        Id = artigo.Id,
        Titulo = artigo.Titulo,
        Slug = artigo.Slug.Valor,
        Resumo = artigo.Resumo,
        Conteudo = artigo.Conteudo,
        Status = artigo.Status,
        DataCriacao = artigo.DataCriacao,
        DataPublicacao = artigo.DataPublicacao,
        MetaTitulo = artigo.Meta.Titulo,
        MetaDescricao = artigo.Meta.Descricao,
        ImagemOgUrl = artigo.Meta.ImagemOgUrl,
        CategoriaId = artigo.CategoriaId,
        Tags = [.. artigo.Tags.Select(t => t.Valor)],
    };

    public Artigo ParaDominio() => Artigo.Reconstituir(
        Id,
        Titulo,
        new Slug(Slug),
        Resumo,
        Conteudo,
        Status,
        DataCriacao,
        DataPublicacao,
        MetaSeo.Criar(MetaTitulo, MetaDescricao, ImagemOgUrl),
        CategoriaId,
        [.. Tags.Select(t => new Tag(t))]);
}
