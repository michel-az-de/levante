using Levante.Conteudo.Domain.Categorias;

namespace Levante.Conteudo.Application.Categorias;

/// <summary>Contrato de saida da categoria (vira o tipo TS gerado do OpenAPI).</summary>
public sealed record CategoriaResponse(Guid Id, string Nome, string Slug, string? Descricao)
{
    public static CategoriaResponse De(Categoria categoria) =>
        new(categoria.Id, categoria.Nome, categoria.Slug.Valor, categoria.Descricao);
}
