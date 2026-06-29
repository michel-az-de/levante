namespace Levante.Conteudo.Application.Artigos.ObterArtigoPorSlug;

/// <summary>Query CQRS-lite: obtem um artigo publicado pelo slug.</summary>
public sealed record ObterArtigoPorSlugQuery(string Slug);
