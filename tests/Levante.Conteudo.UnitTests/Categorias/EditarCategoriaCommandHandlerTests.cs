using Levante.Conteudo.Application.Categorias.EditarCategoria;
using Levante.Conteudo.Domain.Artigos;
using Levante.Conteudo.Domain.Categorias;
using Levante.Conteudo.UnitTests.Artigos;
using Shouldly;
using Xunit;

namespace Levante.Conteudo.UnitTests.Categorias;

[Trait("Category", "Unit")]
public sealed class EditarCategoriaCommandHandlerTests
{
    private static EditarCategoriaCommandHandler Criar(CategoriaRepositorioEmMemoria repo) =>
        new(repo, new EditarCategoriaCommandValidator());

    [Fact]
    public async Task Handle_editaCategoria()
    {
        var categoria = Categoria.Criar("Arquitetura", new Slug("arquitetura"), "Antiga.");
        var repo = new CategoriaRepositorioEmMemoria(categoria);
        var handler = Criar(repo);

        var resultado = await handler.Handle(
            new EditarCategoriaCommand(categoria.Id, "Arquitetura de Software", "Nova."), CancellationToken.None);

        resultado.Sucesso.ShouldBeTrue();
        resultado.Valor!.Nome.ShouldBe("Arquitetura de Software");
        resultado.Valor.Slug.ShouldBe("arquitetura");
        repo.Atualizadas.ShouldBe(1);
    }

    [Fact]
    public async Task Handle_falhaQuandoNaoEncontrada()
    {
        var handler = Criar(new CategoriaRepositorioEmMemoria());

        var resultado = await handler.Handle(
            new EditarCategoriaCommand(Guid.NewGuid(), "Nome"), CancellationToken.None);

        resultado.Falhou.ShouldBeTrue();
        resultado.Erro.Codigo.ShouldBe("categoria_nao_encontrada");
    }
}
