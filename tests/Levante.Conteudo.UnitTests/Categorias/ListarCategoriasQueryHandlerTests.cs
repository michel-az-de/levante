using Levante.Conteudo.Application.Categorias.ListarCategorias;
using Levante.Conteudo.Domain.Artigos;
using Levante.Conteudo.Domain.Categorias;
using Levante.Conteudo.UnitTests.Artigos;
using Shouldly;
using Xunit;

namespace Levante.Conteudo.UnitTests.Categorias;

[Trait("Category", "Unit")]
public sealed class ListarCategoriasQueryHandlerTests
{
    [Fact]
    public async Task Handle_devolveCategoriasMapeadas()
    {
        var repo = new CategoriaRepositorioEmMemoria(
            Categoria.Criar("Arquitetura", new Slug("arquitetura")),
            Categoria.Criar("DevOps", new Slug("devops")));
        var handler = new ListarCategoriasQueryHandler(repo);

        var resultado = await handler.Handle(new ListarCategoriasQuery(), CancellationToken.None);

        resultado.Sucesso.ShouldBeTrue();
        resultado.Valor!.Count.ShouldBe(2);
        resultado.Valor.ShouldContain(c => c.Slug == "arquitetura");
    }

    [Fact]
    public async Task Handle_devolveVazioQuandoNaoHaCategorias()
    {
        var handler = new ListarCategoriasQueryHandler(new CategoriaRepositorioEmMemoria());

        var resultado = await handler.Handle(new ListarCategoriasQuery(), CancellationToken.None);

        resultado.Sucesso.ShouldBeTrue();
        resultado.Valor.ShouldBeEmpty();
    }
}
