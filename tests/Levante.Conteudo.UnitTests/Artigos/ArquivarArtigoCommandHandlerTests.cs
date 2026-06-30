using Levante.Conteudo.Application.Artigos.ArquivarArtigo;
using Levante.Conteudo.Domain.Artigos;
using Shouldly;
using Xunit;

namespace Levante.Conteudo.UnitTests.Artigos;

public sealed class ArquivarArtigoCommandHandlerTests
{
    [Fact]
    public async Task Handle_arquivaPublicado()
    {
        var artigo = Artigo.Criar("Titulo", new Slug("titulo"), "Resumo.", "Conteudo.");
        artigo.Publicar();
        var repo = new ArtigoRepositorioEmMemoria(artigo);
        var handler = new ArquivarArtigoCommandHandler(repo);

        var resultado = await handler.Handle(new ArquivarArtigoCommand(artigo.Id), CancellationToken.None);

        resultado.Sucesso.ShouldBeTrue();
        resultado.Valor!.Status.ShouldBe(nameof(StatusArtigo.Arquivado));
        repo.Atualizados.ShouldBe(1);
    }

    [Fact]
    public async Task Handle_falhaQuandoNaoEncontrado()
    {
        var repo = new ArtigoRepositorioEmMemoria();
        var handler = new ArquivarArtigoCommandHandler(repo);

        var resultado = await handler.Handle(new ArquivarArtigoCommand(Guid.NewGuid()), CancellationToken.None);

        resultado.Falhou.ShouldBeTrue();
        resultado.Erro.Codigo.ShouldBe("artigo_nao_encontrado");
    }
}
