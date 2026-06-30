namespace Levante.Conteudo.Application.Categorias.EditarCategoria;

/// <summary>Comando CQRS-lite: edita nome/descricao de uma categoria (slug imutavel).</summary>
public sealed record EditarCategoriaCommand(Guid Id, string Nome, string? Descricao = null);
