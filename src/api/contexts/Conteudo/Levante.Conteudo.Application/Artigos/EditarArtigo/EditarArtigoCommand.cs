namespace Levante.Conteudo.Application.Artigos.EditarArtigo;

/// <summary>Comando CQRS-lite: edita os campos de um artigo existente.</summary>
public sealed record EditarArtigoCommand(Guid Id, string Titulo, string Slug, string Resumo, string Conteudo);
