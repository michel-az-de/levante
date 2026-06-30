using Levante.Conteudo.Application.Artigos.CriarArtigo;
using Levante.Conteudo.Domain.Artigos;
using Shouldly;
using Xunit;

namespace Levante.Conteudo.UnitTests.Artigos;

public sealed class CriarArtigoCommandHandlerTests
{
    private static CriarArtigoCommandHandler Criar(ArtigoRepositorioEmMemoria repo) =>
        new(repo, new CriarArtigoCommandValidator());

    [Fact]
    public async Task Handle_criaArtigoEmRascunho()
    {
        var repo = new ArtigoRepositorioEmMemoria();
        var handler = Criar(repo);

        var resultado = await handler.Handle(
            new CriarArtigoCommand("Titulo", "meu-artigo", "Resumo.", "Conteudo."), CancellationToken.None);

        resultado.Sucesso.ShouldBeTrue();
        resultado.Valor.ShouldNotBeNull();
        resultado.Valor.Slug.ShouldBe("meu-artigo");
        resultado.Valor.Status.ShouldBe(nameof(StatusArtigo.Rascunho));
        repo.Adicionados.ShouldBe(1);
    }

    [Fact]
    public async Task Handle_persisteMetaSeoNaResposta()
    {
        var repo = new ArtigoRepositorioEmMemoria();
        var handler = Criar(repo);

        var resultado = await handler.Handle(
            new CriarArtigoCommand(
                "Titulo", "meu-artigo", "Resumo.", "Conteudo.",
                MetaTitulo: "Titulo SEO", MetaDescricao: "Descricao SEO", ImagemOgUrl: "/og.png"),
            CancellationToken.None);

        resultado.Sucesso.ShouldBeTrue();
        resultado.Valor!.MetaTitulo.ShouldBe("Titulo SEO");
        resultado.Valor.MetaDescricao.ShouldBe("Descricao SEO");
        resultado.Valor.ImagemOgUrl.ShouldBe("/og.png");
    }

    [Fact]
    public async Task Handle_semMeta_respostaComMetaNula()
    {
        var repo = new ArtigoRepositorioEmMemoria();
        var handler = Criar(repo);

        var resultado = await handler.Handle(
            new CriarArtigoCommand("Titulo", "meu-artigo", "Resumo.", "Conteudo."), CancellationToken.None);

        resultado.Sucesso.ShouldBeTrue();
        resultado.Valor!.MetaTitulo.ShouldBeNull();
        resultado.Valor.MetaDescricao.ShouldBeNull();
        resultado.Valor.ImagemOgUrl.ShouldBeNull();
    }

    [Fact]
    public async Task Handle_falhaQuandoSlugDuplicado()
    {
        var existente = Artigo.Criar("Outro", new Slug("meu-artigo"), "Resumo.", "Conteudo.");
        var repo = new ArtigoRepositorioEmMemoria(existente);
        var handler = Criar(repo);

        var resultado = await handler.Handle(
            new CriarArtigoCommand("Titulo", "meu-artigo", "Resumo.", "Conteudo."), CancellationToken.None);

        resultado.Falhou.ShouldBeTrue();
        resultado.Erro.Codigo.ShouldBe("slug_em_uso");
        repo.Adicionados.ShouldBe(0);
    }

    [Fact]
    public async Task Handle_falhaQuandoSlugInvalido()
    {
        var repo = new ArtigoRepositorioEmMemoria();
        var handler = Criar(repo);

        var resultado = await handler.Handle(
            new CriarArtigoCommand("Titulo", "Slug Invalido", "Resumo.", "Conteudo."), CancellationToken.None);

        resultado.Falhou.ShouldBeTrue();
        resultado.Erro.Codigo.ShouldBe("validacao");
        repo.Adicionados.ShouldBe(0);
    }

    [Fact]
    public async Task Handle_falhaQuandoCorridaViolouIndice()
    {
        // Slug livre na pre-checagem, mas a escrita bate no indice unico (corrida).
        var repo = new ArtigoRepositorioEmMemoria { LancarSlugEmUsoNaEscrita = true };
        var handler = Criar(repo);

        var resultado = await handler.Handle(
            new CriarArtigoCommand("Titulo", "meu-artigo", "Resumo.", "Conteudo."), CancellationToken.None);

        resultado.Falhou.ShouldBeTrue();
        resultado.Erro.Codigo.ShouldBe("slug_em_uso");
    }

    [Fact]
    public async Task Handle_falhaQuandoTituloVazio()
    {
        var repo = new ArtigoRepositorioEmMemoria();
        var handler = Criar(repo);

        var resultado = await handler.Handle(
            new CriarArtigoCommand("", "meu-artigo", "Resumo.", "Conteudo."), CancellationToken.None);

        resultado.Falhou.ShouldBeTrue();
        resultado.Erro.Codigo.ShouldBe("validacao");
    }
}
