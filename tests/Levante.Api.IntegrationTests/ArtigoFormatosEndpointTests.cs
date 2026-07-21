using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Levante.Api.Endpoints;
using Levante.Api.IntegrationTests.Fixtures;
using Levante.Conteudo.Application.Artigos;
using Levante.Conteudo.Application.Midias;
using Shouldly;
using Xunit;

namespace Levante.Api.IntegrationTests;

/// <summary>
/// Gera postagem "em todos os formatos possiveis e de formas diferentes" ponta-a-ponta
/// (HTTP -> validacao -> dominio -> Mongo -> leitura publica) para provar integridade:
/// o corpo do artigo volta identico ao que entrou, inclusive markdown rico, imagem de
/// midia real, HTML cru, unicode e corpo grande. Cobre tambem as formas de criar/publicar
/// (ciclo de vida, tags, negativos de validacao) que o fluxo base nao exercita.
/// </summary>
[Trait("Category", "Integration")]
public sealed class ArtigoFormatosEndpointTests(ApiAppFixture fixture) : IClassFixture<ApiAppFixture>
{
    private static readonly byte[] BytesPng = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0, 0, 0, 0];

    /// <summary>Formatos de corpo que o artigo aceita; o nome tambem serve de slug unico no container.</summary>
    public static IEnumerable<object[]> FormatosDeConteudo()
    {
        // Chars unicode montados por code point para manter o .cs em ASCII (convencao do repo) sem
        // depender do encoding do arquivo; em runtime e uma string unicode de verdade (acentos, emoji, CJK).
        var unicode = "Cora" + (char)0x00E7 + (char)0x00E3 + "o " + (char)0x00E0 + " noite: "
            + (char)0x00E7 + ", " + (char)0x00E3 + ", " + (char)0x00E9 + " - emoji "
            + char.ConvertFromUtf32(0x1F600) + " - CJK " + (char)0x4E2D + (char)0x6587 + " - integridade.";

        return
        [
            ["texto-puro", "Um paragrafo simples, sem marcacao."],
            ["headings", "# H1\n\n## H2\n\n### H3\n\nCorpo."],
            ["listas", "- item a\n- item b\n\n1. um\n2. dois"],
            ["tabela-gfm", "| a | b |\n| - | - |\n| 1 | 2 |"],
            ["task-list", "- [ ] pendente\n- [x] feito"],
            ["code-fence", "```csharp\nvar x = 1;\n```\n\nInline `code` aqui."],
            ["blockquote", "> uma citacao\n> em duas linhas"],
            ["links", "Veja [o guia](https://exemplo.com) e https://autolink.dev"],
            ["imagem-url-absoluta", "![capa](https://cdn.exemplo.com/capa.png)"],
            ["html-cru-embutido", "Texto\n\n<div class=\"callout\">bloco cru</div>\n\n<script>alert(1)</script>"],
            ["unicode-acentos-emoji", unicode],
            ["markdown-rico", "# Titulo\n\n> nota\n\n- a\n- b\n\n```\ncodigo\n```\n\n| x | y |\n| - | - |\n| 1 | 2 |"],
            ["conteudo-grande", new string('a', 50_000)],
        ];
    }

    [Theory]
    [MemberData(nameof(FormatosDeConteudo))]
    public async Task Criar_ePublicar_conteudoEmVariosFormatos_roundtripIntegro(string nome, string conteudo)
    {
        var client = await fixture.CriarClienteAutenticadoAsync();
        var slug = $"fmt-{nome}";

        var criacao = await client.PostAsJsonAsync(
            "/artigos",
            new CriarArtigoRequest($"Formato {nome}", slug, "Resumo.", conteudo),
            CancellationToken.None);

        criacao.StatusCode.ShouldBe(HttpStatusCode.Created);
        var criado = await criacao.Content.ReadFromJsonAsync<ArtigoResponse>(CancellationToken.None);
        criado.ShouldNotBeNull();
        criado.Conteudo.ShouldBe(conteudo); // round-trip na criacao (echo do handler)

        var publicacao = await client.PostAsync($"/artigos/{criado.Id}/publicar", content: null, CancellationToken.None);
        publicacao.StatusCode.ShouldBe(HttpStatusCode.OK);

        // Round-trip pelo Mongo: o publico devolve exatamente o corpo gravado, sem reescrita.
        var publico = await client.GetFromJsonAsync<ArtigoResponse>($"/artigos/{slug}", CancellationToken.None);
        publico.ShouldNotBeNull();
        publico.Conteudo.ShouldBe(conteudo);
    }

    /// <summary>Postagem com imagem de midia REAL: sobe a midia, embute /midias/{id} no markdown e o publico serve a imagem.</summary>
    [Fact]
    public async Task Criar_comImagemDeMidiaReal_publicaEServeAImagem()
    {
        var client = await fixture.CriarClienteAutenticadoAsync();

        using var formulario = FormularioPng();
        var envio = await client.PostAsync("/admin/midias", formulario, CancellationToken.None);
        envio.StatusCode.ShouldBe(HttpStatusCode.Created);
        var midia = await envio.Content.ReadFromJsonAsync<MidiaResponse>(CancellationToken.None);
        midia.ShouldNotBeNull();

        const string slug = "fmt-artigo-com-midia";
        var conteudo = $"# Com imagem\n\n![diagrama]({midia.Url})\n";
        var criacao = await client.PostAsJsonAsync(
            "/artigos",
            new CriarArtigoRequest("Artigo com midia", slug, "Resumo.", conteudo),
            CancellationToken.None);
        criacao.StatusCode.ShouldBe(HttpStatusCode.Created);
        var criado = await criacao.Content.ReadFromJsonAsync<ArtigoResponse>(CancellationToken.None);
        criado.ShouldNotBeNull();
        criado.Conteudo.ShouldBe(conteudo);

        var publicacao = await client.PostAsync($"/artigos/{criado.Id}/publicar", content: null, CancellationToken.None);
        publicacao.StatusCode.ShouldBe(HttpStatusCode.OK);

        var publico = await client.GetFromJsonAsync<ArtigoResponse>($"/artigos/{slug}", CancellationToken.None);
        publico.ShouldNotBeNull();
        publico.Conteudo.ShouldContain(midia.Url);

        // A URL embutida serve a imagem de verdade (vinculo artigo <-> midia integro).
        var imagem = await fixture.CreateClient().GetAsync(midia.Url, CancellationToken.None);
        imagem.StatusCode.ShouldBe(HttpStatusCode.OK);
        imagem.Content.Headers.ContentType!.MediaType.ShouldBe("image/png");
        (await imagem.Content.ReadAsByteArrayAsync(CancellationToken.None)).ShouldBe(BytesPng);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Criar_conteudoVazioOuEmBranco_retorna400(string conteudo)
    {
        var client = await fixture.CriarClienteAutenticadoAsync();

        var resposta = await client.PostAsJsonAsync(
            "/artigos",
            new CriarArtigoRequest("Titulo", "fmt-conteudo-invalido", "Resumo.", conteudo),
            CancellationToken.None);

        resposta.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Criar_comTagsValidas_roundtripDasTags()
    {
        var client = await fixture.CriarClienteAutenticadoAsync();

        var criacao = await client.PostAsJsonAsync(
            "/artigos",
            new CriarArtigoRequest("Com tags", "fmt-com-tags", "Resumo.", "Conteudo.", Tags: ["clean-architecture", "ddd"]),
            CancellationToken.None);

        criacao.StatusCode.ShouldBe(HttpStatusCode.Created);
        var criado = await criacao.Content.ReadFromJsonAsync<ArtigoResponse>(CancellationToken.None);
        criado.ShouldNotBeNull();
        criado.Tags.ShouldBe(["clean-architecture", "ddd"]);
    }

    [Fact]
    public async Task Criar_comTagInvalida_retorna400()
    {
        var client = await fixture.CriarClienteAutenticadoAsync();

        var resposta = await client.PostAsJsonAsync(
            "/artigos",
            new CriarArtigoRequest("Tag ruim", "fmt-tag-ruim", "Resumo.", "Conteudo.", Tags: ["Tag Invalida"]),
            CancellationToken.None);

        resposta.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    private static MultipartFormDataContent FormularioPng()
    {
        var conteudo = new ByteArrayContent(BytesPng);
        conteudo.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        return new MultipartFormDataContent { { conteudo, MidiaAdminEndpoints.CampoArquivo, "foto.png" } };
    }
}
