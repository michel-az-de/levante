using System.Net;
using System.Net.Http.Json;
using Levante.Api.Endpoints;
using Levante.Api.IntegrationTests.Fixtures;
using Levante.Conteudo.Application.Artigos;
using Shouldly;
using Xunit;

namespace Levante.Api.IntegrationTests;

[Trait("Category", "Integration")]
public sealed class ArtigoAdminEndpointTests(ApiAppFixture fixture) : IClassFixture<ApiAppFixture>
{
    [Fact]
    public async Task Criar_semToken_retorna401()
    {
        var client = fixture.CreateClient();

        var resposta = await client.PostAsJsonAsync(
            "/artigos",
            new CriarArtigoRequest("Sem token", "sem-token", "Resumo.", "Conteudo."),
            CancellationToken.None);

        resposta.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task FluxoCompleto_criarPublicarEditarArquivar()
    {
        var client = await fixture.CriarClienteAutenticadoAsync();
        const string slug = "fatia-2b-fluxo-completo";

        // Cria (rascunho) -> 201
        var criacao = await client.PostAsJsonAsync(
            "/artigos",
            new CriarArtigoRequest("Fluxo 2b", slug, "Resumo do fluxo.", "# Conteudo\n\nMarkdown."),
            CancellationToken.None);
        criacao.StatusCode.ShouldBe(HttpStatusCode.Created);
        var criado = await criacao.Content.ReadFromJsonAsync<ArtigoResponse>(CancellationToken.None);
        criado.ShouldNotBeNull();
        criado.Status.ShouldBe("Rascunho");

        // Aparece na lista admin
        var listaAdmin = await client.GetFromJsonAsync<List<ArtigoResponse>>("/admin/artigos", CancellationToken.None);
        listaAdmin.ShouldNotBeNull();
        listaAdmin.ShouldContain(a => a.Slug == slug);

        // Rascunho NAO aparece no publico
        var publicoAntes = await client.GetFromJsonAsync<List<ArtigoResponse>>("/artigos", CancellationToken.None);
        publicoAntes.ShouldNotBeNull();
        publicoAntes.ShouldNotContain(a => a.Slug == slug);

        // Publica -> aparece no publico
        var publicacao = await client.PostAsync($"/artigos/{criado.Id}/publicar", content: null, CancellationToken.None);
        publicacao.StatusCode.ShouldBe(HttpStatusCode.OK);
        var publicado = await publicacao.Content.ReadFromJsonAsync<ArtigoResponse>(CancellationToken.None);
        publicado!.Status.ShouldBe("Publicado");

        var publicoDepois = await client.GetFromJsonAsync<List<ArtigoResponse>>("/artigos", CancellationToken.None);
        publicoDepois!.ShouldContain(a => a.Slug == slug);

        // Edita
        var edicao = await client.PutAsJsonAsync(
            $"/artigos/{criado.Id}",
            new EditarArtigoRequest("Fluxo 2b (editado)", slug, "Resumo editado.", "Conteudo editado."),
            CancellationToken.None);
        edicao.StatusCode.ShouldBe(HttpStatusCode.OK);

        // Arquiva (despublicar) -> some do publico
        var arquivamento = await client.PostAsync($"/artigos/{criado.Id}/arquivar", content: null, CancellationToken.None);
        arquivamento.StatusCode.ShouldBe(HttpStatusCode.OK);
        var arquivado = await arquivamento.Content.ReadFromJsonAsync<ArtigoResponse>(CancellationToken.None);
        arquivado!.Status.ShouldBe("Arquivado");

        var publicoFinal = await client.GetFromJsonAsync<List<ArtigoResponse>>("/artigos", CancellationToken.None);
        publicoFinal!.ShouldNotContain(a => a.Slug == slug);
    }

    [Fact]
    public async Task Criar_comMetaSeo_persisteERetornaNoPublico()
    {
        var client = await fixture.CriarClienteAutenticadoAsync();
        const string slug = "fatia-2c-meta-seo";

        var criacao = await client.PostAsJsonAsync(
            "/artigos",
            new CriarArtigoRequest(
                "Artigo com SEO", slug, "Resumo.", "Conteudo.",
                MetaTitulo: "Titulo SEO custom", MetaDescricao: "Descricao SEO custom", ImagemOgUrl: "/og/custom.png"),
            CancellationToken.None);
        criacao.StatusCode.ShouldBe(HttpStatusCode.Created);
        var criado = await criacao.Content.ReadFromJsonAsync<ArtigoResponse>(CancellationToken.None);
        criado.ShouldNotBeNull();

        var publicacao = await client.PostAsync($"/artigos/{criado.Id}/publicar", content: null, CancellationToken.None);
        publicacao.StatusCode.ShouldBe(HttpStatusCode.OK);

        // Round-trip pelo Mongo: o publico devolve a meta SEO gravada.
        var publico = await client.GetFromJsonAsync<ArtigoResponse>($"/artigos/{slug}", CancellationToken.None);
        publico.ShouldNotBeNull();
        publico.MetaTitulo.ShouldBe("Titulo SEO custom");
        publico.MetaDescricao.ShouldBe("Descricao SEO custom");
        publico.ImagemOgUrl.ShouldBe("/og/custom.png");
    }

    [Fact]
    public async Task Criar_semMetaSeo_retornaCamposNulos()
    {
        var client = await fixture.CriarClienteAutenticadoAsync();
        const string slug = "fatia-2c-sem-meta";

        var criacao = await client.PostAsJsonAsync(
            "/artigos",
            new CriarArtigoRequest("Sem SEO", slug, "Resumo.", "Conteudo."),
            CancellationToken.None);
        criacao.StatusCode.ShouldBe(HttpStatusCode.Created);
        var criado = await criacao.Content.ReadFromJsonAsync<ArtigoResponse>(CancellationToken.None);

        criado.ShouldNotBeNull();
        criado.MetaTitulo.ShouldBeNull();
        criado.MetaDescricao.ShouldBeNull();
        criado.ImagemOgUrl.ShouldBeNull();
    }

    [Fact]
    public async Task Criar_comSlugDuplicado_retorna409()
    {
        var client = await fixture.CriarClienteAutenticadoAsync();

        // "clean-architecture-na-pratica" e semeado em Development.
        var resposta = await client.PostAsJsonAsync(
            "/artigos",
            new CriarArtigoRequest("Duplicado", "clean-architecture-na-pratica", "Resumo.", "Conteudo."),
            CancellationToken.None);

        resposta.StatusCode.ShouldBe(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Publicar_inexistente_retorna404()
    {
        var client = await fixture.CriarClienteAutenticadoAsync();

        var resposta = await client.PostAsync(
            $"/artigos/{Guid.NewGuid()}/publicar", content: null, CancellationToken.None);

        resposta.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

}
