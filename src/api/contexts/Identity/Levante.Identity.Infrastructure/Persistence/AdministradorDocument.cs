using Levante.Identity.Domain.Administradores;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Levante.Identity.Infrastructure.Persistence;

/// <summary>Modelo de persistencia do admin (collection "administradores", campos PT camelCase).</summary>
internal sealed class AdministradorDocument
{
    [BsonId]
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid Id { get; set; }

    [BsonElement("email")]
    public string Email { get; set; } = string.Empty;

    [BsonElement("senhaHash")]
    public string SenhaHash { get; set; } = string.Empty;

    [BsonElement("tentativasDeLoginFalhas")]
    public int TentativasDeLoginFalhas { get; set; }

    [BsonElement("bloqueadoAte")]
    public DateTime? BloqueadoAte { get; set; }

    [BsonElement("ativo")]
    public bool Ativo { get; set; }

    [BsonElement("dataCriacao")]
    public DateTime DataCriacao { get; set; }

    public static AdministradorDocument DeDominio(Administrador administrador) => new()
    {
        Id = administrador.Id,
        Email = administrador.Email.Valor,
        SenhaHash = administrador.SenhaHash,
        TentativasDeLoginFalhas = administrador.TentativasDeLoginFalhas,
        BloqueadoAte = administrador.BloqueadoAte,
        Ativo = administrador.Ativo,
        DataCriacao = administrador.DataCriacao,
    };

    public Administrador ParaDominio() => Administrador.Reconstituir(
        Id,
        new Email(Email),
        SenhaHash,
        TentativasDeLoginFalhas,
        BloqueadoAte,
        Ativo,
        DataCriacao);
}
