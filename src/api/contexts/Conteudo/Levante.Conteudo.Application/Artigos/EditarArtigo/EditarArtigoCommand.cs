namespace Levante.Conteudo.Application.Artigos.EditarArtigo;

/// <summary>Comando CQRS-lite: edita os campos de um artigo existente (inclui meta SEO opcional).</summary>
public sealed record EditarArtigoCommand(
    Guid Id,
    string Titulo,
    string Slug,
    string Resumo,
    string Conteudo,
    string? MetaTitulo = null,
    string? MetaDescricao = null,
    string? ImagemOgUrl = null);
