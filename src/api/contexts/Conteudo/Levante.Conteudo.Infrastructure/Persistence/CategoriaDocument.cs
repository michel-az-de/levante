using Levante.Conteudo.Domain.Artigos;
using Levante.Conteudo.Domain.Categorias;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Levante.Conteudo.Infrastructure.Persistence;

/// <summary>
/// Modelo de persistencia da categoria (collection "categorias"). Campos PT camelCase.
/// </summary>
internal sealed class CategoriaDocument
{
    [BsonId]
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid Id { get; set; }

    [BsonElement("nome")]
    public string Nome { get; set; } = string.Empty;

    [BsonElement("slug")]
    public string Slug { get; set; } = string.Empty;

    [BsonElement("descricao")]
    [BsonIgnoreIfNull]
    public string? Descricao { get; set; }

    [BsonElement("dataCriacao")]
    public DateTime DataCriacao { get; set; }

    public static CategoriaDocument DeDominio(Categoria categoria) => new()
    {
        Id = categoria.Id,
        Nome = categoria.Nome,
        Slug = categoria.Slug.Valor,
        Descricao = categoria.Descricao,
        DataCriacao = categoria.DataCriacao,
    };

    public Categoria ParaDominio() =>
        Categoria.Reconstituir(Id, Nome, new Slug(Slug), Descricao, DataCriacao);
}
