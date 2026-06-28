using Levante.SharedKernel;

namespace Levante.Conteudo.Domain.Artigos;

/// <summary>
/// Evento de dominio: um artigo foi publicado. Fato PT, sem sufixo.
/// Vira <c>ArtigoPublicadoIntegrationEvent</c> no Outbox (fora da Fatia 0).
/// </summary>
public sealed record ArtigoPublicado(Guid ArtigoId, string Slug, DateTime DataPublicacao)
    : IEventoDeDominio;
