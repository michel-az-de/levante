using Levante.Conteudo.Application.Categorias.CriarCategoria;
using Levante.Conteudo.Domain.Artigos;
using Levante.Conteudo.Domain.Categorias;
using Levante.Conteudo.UnitTests.Artigos;
using Shouldly;
using Xunit;

namespace Levante.Conteudo.UnitTests.Categorias;

public sealed class CriarCategoriaCommandHandlerTests
{
    private static CriarCategoriaCommandHandler Criar(CategoriaRepositorioEmMemoria repo) =>
        new(repo, new CriarCategoriaCommandValidator());

    [Fact]
    public async Task Handle_criaCategoria()
    {
        var repo = new CategoriaRepositorioEmMemoria();
        var handler = Criar(repo);

        var resultado = await handler.Handle(
            new CriarCategoriaCommand("Arquitetura", "arquitetura", "Desc."), CancellationToken.None);

        resultado.Sucesso.ShouldBeTrue();
        resultado.Valor!.Slug.ShouldBe("arquitetura");
        repo.Adicionadas.ShouldBe(1);
    }

    [Fact]
    public async Task Handle_falhaQuandoSlugDuplicado()
    {
        var existente = Categoria.Criar("Arquitetura", new Slug("arquitetura"));
        var repo = new CategoriaRepositorioEmMemoria(existente);
        var handler = Criar(repo);

        var resultado = await handler.Handle(
            new CriarCategoriaCommand("Outra", "arquitetura"), CancellationToken.None);

        resultado.Falhou.ShouldBeTrue();
        resultado.Erro.Codigo.ShouldBe("slug_em_uso");
        repo.Adicionadas.ShouldBe(0);
    }

    [Fact]
    public async Task Handle_falhaQuandoNomeVazio()
    {
        var handler = Criar(new CategoriaRepositorioEmMemoria());

        var resultado = await handler.Handle(
            new CriarCategoriaCommand("", "arquitetura"), CancellationToken.None);

        resultado.Falhou.ShouldBeTrue();
        resultado.Erro.Codigo.ShouldBe("validacao");
    }

    [Fact]
    public async Task Handle_falhaQuandoCorridaViolouIndice()
    {
        var repo = new CategoriaRepositorioEmMemoria { LancarSlugEmUsoNaEscrita = true };
        var handler = Criar(repo);

        var resultado = await handler.Handle(
            new CriarCategoriaCommand("Arquitetura", "arquitetura"), CancellationToken.None);

        resultado.Falhou.ShouldBeTrue();
        resultado.Erro.Codigo.ShouldBe("slug_em_uso");
    }
}
