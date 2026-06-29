using System.Net;
using System.Net.Http.Json;
using Levante.Api.IntegrationTests.Fixtures;
using Levante.Conteudo.Application.Artigos;
using Shouldly;
using Xunit;

namespace Levante.Api.IntegrationTests;

[Trait("Category", "Integration")]
public sealed class ArtigosEndpointTests(ApiAppFixture fixture) : IClassFixture<ApiAppFixture>
{
    [Fact]
    public async Task Listar_devolveOSeedLidoDoMongo()
    {
        var client = fixture.CreateClient();

        var artigos = await client.GetFromJsonAsync<List<ArtigoResponse>>("/artigos", CancellationToken.None);

        artigos.ShouldNotBeNull();
        artigos.Count.ShouldBeGreaterThanOrEqualTo(2);
        artigos.ShouldContain(a => a.Slug == "clean-architecture-na-pratica");
    }

    [Fact]
    public async Task ObterPorSlug_devolveOArtigoComResumo()
    {
        var client = fixture.CreateClient();

        var artigo = await client.GetFromJsonAsync<ArtigoResponse>(
            "/artigos/clean-architecture-na-pratica", CancellationToken.None);

        artigo.ShouldNotBeNull();
        artigo.Slug.ShouldBe("clean-architecture-na-pratica");
        artigo.Resumo.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task ObterPorSlug_404QuandoInexistente()
    {
        var client = fixture.CreateClient();

        var resposta = await client.GetAsync("/artigos/nao-existe", CancellationToken.None);

        resposta.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}
