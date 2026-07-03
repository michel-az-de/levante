using Levante.Audiencia.Domain.Assinantes;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Levante.Audiencia.Infrastructure.Persistence;

/// <summary>
/// Modelo de persistencia (collection "assinantes"). Campos PT camelCase. O e-mail
/// e unico (uma inscricao por e-mail); o token e o segredo de confirmacao/descadastro.
/// </summary>
internal sealed class AssinanteDocument
{
    [BsonId]
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid Id { get; set; }

    [BsonElement("email")]
    public string Email { get; set; } = string.Empty;

    [BsonElement("status")]
    [BsonRepresentation(BsonType.String)]
    public StatusAssinante Status { get; set; }

    [BsonElement("token")]
    public string Token { get; set; } = string.Empty;

    [BsonElement("dataCriacao")]
    public DateTime DataCriacao { get; set; }

    [BsonElement("dataConfirmacao")]
    [BsonIgnoreIfNull]
    public DateTime? DataConfirmacao { get; set; }

    [BsonElement("dataCancelamento")]
    [BsonIgnoreIfNull]
    public DateTime? DataCancelamento { get; set; }

    public static AssinanteDocument DeDominio(Assinante assinante) => new()
    {
        Id = assinante.Id,
        Email = assinante.Email.Valor,
        Status = assinante.Status,
        Token = assinante.Token.Valor,
        DataCriacao = assinante.DataCriacao,
        DataConfirmacao = assinante.DataConfirmacao,
        DataCancelamento = assinante.DataCancelamento,
    };

    public Assinante ParaDominio() => Assinante.Reconstituir(
        Id,
        new Email(Email),
        Status,
        new TokenConfirmacao(Token),
        DataCriacao,
        DataConfirmacao,
        DataCancelamento);
}
