using Levante.Conteudo.Application.Artigos.ListarArtigosPublicadosPorCategoria;
using Levante.Conteudo.Domain.Artigos;
using Levante.Conteudo.Domain.Categorias;
using Shouldly;
using Xunit;

namespace Levante.Conteudo.UnitTests.Artigos;

public sealed class ListarArtigosPublicadosPorCategoriaQueryHandlerTests
{
    [Fact]
    public async Task Handle_falhaQuandoCategoriaNaoEncontrada()
    {
        var handler = new ListarArtigosPublicadosPorCategoriaQueryHandler(
            new ArtigoRepositorioEmMemoria(), new CategoriaRepositorioEmMemoria());

        var resultado = await handler.Handle(
            new ListarArtigosPublicadosPorCategoriaQuery("inexistente"), CancellationToken.None);

        resultado.Falhou.ShouldBeTrue();
        resultado.Erro.Codigo.ShouldBe("categoria_nao_encontrada");
    }

    [Fact]
    public async Task Handle_filtraPublicadosDaCategoria()
    {
        var categoria = Categoria.Criar("Arquitetura", new Slug("arquitetura"));

        var daCategoria = Artigo.Criar("Da categoria", new Slug("da-categoria"), "R.", "C.", categoriaId: categoria.Id);
        daCategoria.Publicar();
        var rascunhoDaCategoria = Artigo.Criar("Rascunho", new Slug("rascunho"), "R.", "C.", categoriaId: categoria.Id);
        var deOutraCategoria = Artigo.Criar("Outra", new Slug("outra"), "R.", "C.", categoriaId: Guid.NewGuid());
        deOutraCategoria.Publicar();

        var handler = new ListarArtigosPublicadosPorCategoriaQueryHandler(
            new ArtigoRepositorioEmMemoria(daCategoria, rascunhoDaCategoria, deOutraCategoria),
            new CategoriaRepositorioEmMemoria(categoria));

        var resultado = await handler.Handle(
            new ListarArtigosPublicadosPorCategoriaQuery("arquitetura"), CancellationToken.None);

        resultado.Sucesso.ShouldBeTrue();
        var artigo = resultado.Valor!.ShouldHaveSingleItem();
        artigo.Slug.ShouldBe("da-categoria");
        artigo.CategoriaId.ShouldBe(categoria.Id);
    }
}
