namespace Levante.Conteudo.Application.Artigos.CriarArtigo;

/// <summary>Comando CQRS-lite: cria um artigo em rascunho (slug informado pelo admin). Meta SEO opcional.</summary>
public sealed record CriarArtigoCommand(
    string Titulo,
    string Slug,
    string Resumo,
    string Conteudo,
    string? MetaTitulo = null,
    string? MetaDescricao = null,
    string? ImagemOgUrl = null);
