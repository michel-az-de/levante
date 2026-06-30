namespace Levante.Conteudo.Application.Categorias.CriarCategoria;

/// <summary>Comando CQRS-lite: cria uma categoria (slug informado pelo admin).</summary>
public sealed record CriarCategoriaCommand(string Nome, string Slug, string? Descricao = null);
