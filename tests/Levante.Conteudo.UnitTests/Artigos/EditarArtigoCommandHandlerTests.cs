using Levante.Conteudo.Application.Artigos.EditarArtigo;
using Levante.Conteudo.Domain.Artigos;
using Shouldly;
using Xunit;

namespace Levante.Conteudo.UnitTests.Artigos;

public sealed class EditarArtigoCommandHandlerTests
{
    private static EditarArtigoCommandHandler Criar(ArtigoRepositorioEmMemoria repo) =>
        new(repo, new EditarArtigoCommandValidator());

    [Fact]
    public async Task Handle_editaArtigoExistente()
    {
        var artigo = Artigo.Criar("Titulo", new Slug("titulo"), "Resumo.", "Conteudo.");
        var repo = new ArtigoRepositorioEmMemoria(artigo);
        var handler = Criar(repo);

        var resultado = await handler.Handle(
            new EditarArtigoCommand(artigo.Id, "Editado", "novo-slug", "Novo resumo.", "Novo conteudo."),
            CancellationToken.None);

        resultado.Sucesso.ShouldBeTrue();
        resultado.Valor!.Titulo.ShouldBe("Editado");
        resultado.Valor.Slug.ShouldBe("novo-slug");
        repo.Atualizados.ShouldBe(1);
    }

    [Fact]
    public async Task Handle_falhaQuandoNaoEncontrado()
    {
        var repo = new ArtigoRepositorioEmMemoria();
        var handler = Criar(repo);

        var resultado = await handler.Handle(
            new EditarArtigoCommand(Guid.NewGuid(), "Titulo", "slug", "Resumo.", "Conteudo."),
            CancellationToken.None);

        resultado.Falhou.ShouldBeTrue();
        resultado.Erro.Codigo.ShouldBe("artigo_nao_encontrado");
    }

    [Fact]
    public async Task Handle_falhaQuandoArquivado()
    {
        var artigo = Artigo.Criar("Titulo", new Slug("titulo"), "Resumo.", "Conteudo.");
        artigo.Arquivar();
        var repo = new ArtigoRepositorioEmMemoria(artigo);
        var handler = Criar(repo);

        var resultado = await handler.Handle(
            new EditarArtigoCommand(artigo.Id, "Editado", "titulo", "Resumo.", "Conteudo."),
            CancellationToken.None);

        resultado.Falhou.ShouldBeTrue();
        resultado.Erro.Codigo.ShouldBe("transicao_invalida");
        repo.Atualizados.ShouldBe(0);
    }

    [Fact]
    public async Task Handle_falhaQuandoSlugDeOutroArtigo()
    {
        var alvo = Artigo.Criar("Alvo", new Slug("alvo"), "Resumo.", "Conteudo.");
        var outro = Artigo.Criar("Outro", new Slug("ocupado"), "Resumo.", "Conteudo.");
        var repo = new ArtigoRepositorioEmMemoria(alvo, outro);
        var handler = Criar(repo);

        var resultado = await handler.Handle(
            new EditarArtigoCommand(alvo.Id, "Alvo", "ocupado", "Resumo.", "Conteudo."),
            CancellationToken.None);

        resultado.Falhou.ShouldBeTrue();
        resultado.Erro.Codigo.ShouldBe("slug_em_uso");
    }

    [Fact]
    public async Task Handle_falhaQuandoCorridaViolouIndice()
    {
        var artigo = Artigo.Criar("Titulo", new Slug("titulo"), "Resumo.", "Conteudo.");
        var repo = new ArtigoRepositorioEmMemoria(artigo) { LancarSlugEmUsoNaEscrita = true };
        var handler = Criar(repo);

        var resultado = await handler.Handle(
            new EditarArtigoCommand(artigo.Id, "Editado", "novo-slug", "Resumo.", "Conteudo."),
            CancellationToken.None);

        resultado.Falhou.ShouldBeTrue();
        resultado.Erro.Codigo.ShouldBe("slug_em_uso");
    }

    [Fact]
    public async Task Handle_permiteManterOProprioSlug()
    {
        var artigo = Artigo.Criar("Titulo", new Slug("titulo"), "Resumo.", "Conteudo.");
        var repo = new ArtigoRepositorioEmMemoria(artigo);
        var handler = Criar(repo);

        var resultado = await handler.Handle(
            new EditarArtigoCommand(artigo.Id, "Editado", "titulo", "Resumo.", "Conteudo."),
            CancellationToken.None);

        resultado.Sucesso.ShouldBeTrue();
    }
}
