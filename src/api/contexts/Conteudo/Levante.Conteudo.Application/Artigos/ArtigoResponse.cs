namespace Levante.Conteudo.Application.Artigos;

/// <summary>
/// Contrato de saida do contexto Conteudo. Vira o tipo TS gerado do OpenAPI
/// (PT) consumido pelo Next.js. Nao expor o agregado diretamente.
/// </summary>
public sealed record ArtigoResponse(
    Guid Id,
    string Titulo,
    string Slug,
    string Conteudo,
    DateTime? DataPublicacao);
