namespace Levante.Conteudo.Application.Artigos.ArquivarArtigo;

/// <summary>Comando CQRS-lite: arquiva um artigo (estado terminal; a acao "despublicar").</summary>
public sealed record ArquivarArtigoCommand(Guid Id);
