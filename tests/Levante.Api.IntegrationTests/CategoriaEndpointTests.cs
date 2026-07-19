using System.Net;
using System.Net.Http.Json;
using Levante.Api.Endpoints;
using Levante.Api.IntegrationTests.Fixtures;
using Levante.Conteudo.Application.Artigos;
using Levante.Conteudo.Application.Categorias;
using Shouldly;
using Xunit;

namespace Levante.Api.IntegrationTests;

[Trait("Category", "Integration")]
public sealed class CategoriaEndpointTests(ApiAppFixture fixture) : IClassFixture<ApiAppFixture>
{
    [Fact]
    public async Task Criar_semToken_retorna401()
    {
        var client = fixture.CreateClient();

        var resposta = await client.PostAsJsonAsync(
            "/categorias", new CriarCategoriaRequest("Sem token", "sem-token"), CancellationToken.None);

        resposta.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Criar_eListar()
    {
        var client = await fixture.CriarClienteAutenticadoAsync();
        const string slug = "fatia-2c-ii-categoria";

        var criacao = await client.PostAsJsonAsync(
            "/categorias", new CriarCategoriaRequest("Nova categoria", slug, "Descricao."), CancellationToken.None);
        criacao.StatusCode.ShouldBe(HttpStatusCode.Created);

        var lista = await client.GetFromJsonAsync<List<CategoriaResponse>>("/categorias", CancellationToken.None);
        lista.ShouldNotBeNull();
        lista.ShouldContain(c => c.Slug == slug);
    }

    [Fact]
    public async Task Criar_comSlugDuplicado_retorna409()
    {
        var client = await fixture.CriarClienteAutenticadoAsync();

        // "arquitetura" e semeada em Development.
        var resposta = await client.PostAsJsonAsync(
            "/categorias", new CriarCategoriaRequest("Duplicada", "arquitetura"), CancellationToken.None);

        resposta.StatusCode.ShouldBe(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Artigo_comCategoriaETags_roundtripEbrowse()
    {
        var client = await fixture.CriarClienteAutenticadoAsync();

        var categorias = await client.GetFromJsonAsync<List<CategoriaResponse>>("/categorias", CancellationToken.None);
        var arquitetura = categorias!.First(c => c.Slug == "arquitetura");
        const string slug = "fatia-2c-ii-artigo-categoria";

        var criacao = await client.PostAsJsonAsync(
            "/artigos",
            new CriarArtigoRequest(
                "Artigo com categoria", slug, "Resumo.", "Conteudo.",
                CategoriaId: arquitetura.Id, Tags: ["clean-architecture", "ddd"]),
            CancellationToken.None);
        criacao.StatusCode.ShouldBe(HttpStatusCode.Created);
        var criado = await criacao.Content.ReadFromJsonAsync<ArtigoResponse>(CancellationToken.None);

        var publicacao = await client.PostAsync($"/artigos/{criado!.Id}/publicar", content: null, CancellationToken.None);
        publicacao.StatusCode.ShouldBe(HttpStatusCode.OK);

        // Round-trip pelo Mongo: categoria e tags voltam no GET publico.
        var publico = await client.GetFromJsonAsync<ArtigoResponse>($"/artigos/{slug}", CancellationToken.None);
        publico!.CategoriaId.ShouldBe(arquitetura.Id);
        publico.Tags.ShouldBe(["clean-architecture", "ddd"]);

        // Browse por categoria mostra o artigo publicado.
        var daCategoria = await client.GetFromJsonAsync<List<ArtigoResponse>>(
            "/categorias/arquitetura/artigos", CancellationToken.None);
        daCategoria!.ShouldContain(a => a.Slug == slug);
    }

    [Fact]
    public async Task Criar_artigo_comCategoriaInexistente_retorna400()
    {
        var client = await fixture.CriarClienteAutenticadoAsync();

        var resposta = await client.PostAsJsonAsync(
            "/artigos",
            new CriarArtigoRequest("Sem categoria", "fatia-2c-ii-cat-invalida", "Resumo.", "Conteudo.",
                CategoriaId: Guid.NewGuid()),
            CancellationToken.None);

        resposta.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ListarArtigosPorCategoria_inexistente_retorna404()
    {
        var client = fixture.CreateClient();

        var resposta = await client.GetAsync("/categorias/nao-existe/artigos", CancellationToken.None);

        resposta.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

}
