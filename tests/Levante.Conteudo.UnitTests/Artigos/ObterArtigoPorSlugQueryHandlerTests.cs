using Levante.Conteudo.Application.Artigos.ObterArtigoPorSlug;
using Levante.Conteudo.Domain.Artigos;
using Shouldly;
using Xunit;

namespace Levante.Conteudo.UnitTests.Artigos;

public sealed class ObterArtigoPorSlugQueryHandlerTests
{
    [Fact]
    public async Task Handle_devolveArtigoPublicado()
    {
        var artigo = Artigo.Criar("Titulo", new Slug("meu-artigo"), "Resumo.", "Conteudo.");
        artigo.Publicar();
        var handler = new ObterArtigoPorSlugQueryHandler(new RepositorioFake(artigo));

        var resultado = await handler.Handle(new ObterArtigoPorSlugQuery("meu-artigo"), CancellationToken.None);

        resultado.Sucesso.ShouldBeTrue();
        resultado.Valor.ShouldNotBeNull();
        resultado.Valor.Slug.ShouldBe("meu-artigo");
        resultado.Valor.Resumo.ShouldBe("Resumo.");
    }

    [Fact]
    public async Task Handle_falhaQuandoNaoEncontrado()
    {
        var handler = new ObterArtigoPorSlugQueryHandler(new RepositorioFake(artigoFixo: null));

        var resultado = await handler.Handle(new ObterArtigoPorSlugQuery("inexistente"), CancellationToken.None);

        resultado.Falhou.ShouldBeTrue();
        resultado.Erro.Codigo.ShouldBe("artigo_nao_encontrado");
    }

    [Fact]
    public async Task Handle_falhaQuandoNaoPublicado()
    {
        var rascunho = Artigo.Criar("Titulo", new Slug("rascunho"), "Resumo.", "Conteudo.");
        var handler = new ObterArtigoPorSlugQueryHandler(new RepositorioFake(rascunho));

        var resultado = await handler.Handle(new ObterArtigoPorSlugQuery("rascunho"), CancellationToken.None);

        resultado.Falhou.ShouldBeTrue();
    }

    private sealed class RepositorioFake(Artigo? artigoFixo) : IArtigoRepository
    {
        public Task<IReadOnlyList<Artigo>> ListPublicadosAsync(CancellationToken ct) =>
            Task.FromResult<IReadOnlyList<Artigo>>(artigoFixo is null ? [] : [artigoFixo]);

        public Task<Artigo?> GetBySlugAsync(string slug, CancellationToken ct) =>
            Task.FromResult(artigoFixo is not null && artigoFixo.Slug.Valor == slug ? artigoFixo : null);

        public Task AddAsync(Artigo artigo, CancellationToken ct) => Task.CompletedTask;
    }
}
