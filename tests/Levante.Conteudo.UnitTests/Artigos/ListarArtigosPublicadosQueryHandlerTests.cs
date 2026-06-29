using Levante.Conteudo.Application.Artigos.ListarArtigosPublicados;
using Levante.Conteudo.Domain.Artigos;
using Shouldly;
using Xunit;

namespace Levante.Conteudo.UnitTests.Artigos;

public sealed class ListarArtigosPublicadosQueryHandlerTests
{
    [Fact]
    public async Task Handle_devolveArtigosMapeados()
    {
        var artigo = Artigo.Criar("Titulo", new Slug("titulo"), "Resumo.", "Conteudo.");
        artigo.Publicar();
        var repositorio = new RepositorioFake([artigo]);
        var handler = new ListarArtigosPublicadosQueryHandler(repositorio);

        var resultado = await handler.Handle(new ListarArtigosPublicadosQuery(), CancellationToken.None);

        resultado.Sucesso.ShouldBeTrue();
        resultado.Valor.ShouldNotBeNull();
        var resposta = resultado.Valor.ShouldHaveSingleItem();
        resposta.Slug.ShouldBe("titulo");
        resposta.Titulo.ShouldBe("Titulo");
    }

    [Fact]
    public async Task Handle_devolveVazioQuandoNaoHaArtigos()
    {
        var handler = new ListarArtigosPublicadosQueryHandler(new RepositorioFake([]));

        var resultado = await handler.Handle(new ListarArtigosPublicadosQuery(), CancellationToken.None);

        resultado.Sucesso.ShouldBeTrue();
        resultado.Valor.ShouldBeEmpty();
    }

    private sealed class RepositorioFake(IReadOnlyList<Artigo> artigos) : IArtigoRepository
    {
        public Task<IReadOnlyList<Artigo>> ListPublicadosAsync(CancellationToken ct) =>
            Task.FromResult(artigos);

        public Task<Artigo?> GetBySlugAsync(string slug, CancellationToken ct) =>
            Task.FromResult(artigos.FirstOrDefault(a => a.Slug.Valor == slug));

        public Task AddAsync(Artigo artigo, CancellationToken ct) => Task.CompletedTask;
    }
}
