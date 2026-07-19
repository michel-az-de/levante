using Levante.Conteudo.Domain.Midias;

namespace Levante.Conteudo.Application.Midias;

/// <summary>Resposta de upload de midia. <see cref="Url"/> e relativa (mesma origem no publico e no admin).</summary>
public sealed record MidiaResponse(Guid Id, string Url, string ContentType, long Tamanho)
{
    public static MidiaResponse DeMidia(MidiaArmazenada midia) =>
        new(midia.Id, $"/midias/{midia.Id}", midia.ContentType, midia.Tamanho);
}
