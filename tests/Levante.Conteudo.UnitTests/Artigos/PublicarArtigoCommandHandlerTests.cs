using Levante.Conteudo.Application.Artigos.PublicarArtigo;
using Levante.Conteudo.Domain.Artigos;
using Shouldly;
using Xunit;

namespace Levante.Conteudo.UnitTests.Artigos;

[Trait("Category", "Unit")]
public sealed class PublicarArtigoCommandHandlerTests
{
    [Fact]
    public async Task Handle_publicaRascunho()
    {
        var artigo = Artigo.Criar("Titulo", new Slug("titulo"), "Resumo.", "Conteudo.");
        var repo = new ArtigoRepositorioEmMemoria(artigo);
        var handler = new PublicarArtigoCommandHandler(repo);

        var resultado = await handler.Handle(new PublicarArtigoCommand(artigo.Id), CancellationToken.None);

        resultado.Sucesso.ShouldBeTrue();
        resultado.Valor!.Status.ShouldBe(nameof(StatusArtigo.Publicado));
        repo.Atualizados.ShouldBe(1);
    }

    [Fact]
    public async Task Handle_falhaQuandoNaoEncontrado()
    {
        var repo = new ArtigoRepositorioEmMemoria();
        var handler = new PublicarArtigoCommandHandler(repo);

        var resultado = await handler.Handle(new PublicarArtigoCommand(Guid.NewGuid()), CancellationToken.None);

        resultado.Falhou.ShouldBeTrue();
        resultado.Erro.Codigo.ShouldBe("artigo_nao_encontrado");
    }

    [Fact]
    public async Task Handle_falhaQuandoArquivado()
    {
        var artigo = Artigo.Criar("Titulo", new Slug("titulo"), "Resumo.", "Conteudo.");
        artigo.Arquivar();
        var repo = new ArtigoRepositorioEmMemoria(artigo);
        var handler = new PublicarArtigoCommandHandler(repo);

        var resultado = await handler.Handle(new PublicarArtigoCommand(artigo.Id), CancellationToken.None);

        resultado.Falhou.ShouldBeTrue();
        resultado.Erro.Codigo.ShouldBe("transicao_invalida");
        repo.Atualizados.ShouldBe(0);
    }
}
