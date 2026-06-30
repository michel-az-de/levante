namespace Levante.Conteudo.Application.Artigos.PublicarArtigo;

/// <summary>Comando CQRS-lite: publica um artigo (Rascunho -> Publicado, idempotente).</summary>
public sealed record PublicarArtigoCommand(Guid Id);
