using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Levante.Api.Endpoints;
using Levante.Api.IntegrationTests.Fixtures;
using Levante.Conteudo.Application.Midias;
using Shouldly;
using Xunit;

namespace Levante.Api.IntegrationTests;

[Trait("Category", "Integration")]
public sealed class MidiaAdminEndpointTests(ApiAppFixture fixture) : IClassFixture<ApiAppFixture>
{
    private static readonly byte[] BytesPng = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0, 0, 0, 0];

    [Fact]
    public async Task Enviar_semToken_retorna401()
    {
        var client = fixture.CreateClient();
        using var formulario = FormularioComArquivo(BytesPng, "image/png", "foto.png");

        var resposta = await client.PostAsync("/admin/midias", formulario, CancellationToken.None);

        resposta.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Enviar_pngValido_retorna201EDepoisServeComCacheImutavel()
    {
        var client = await fixture.CriarClienteAutenticadoAsync();
        using var formulario = FormularioComArquivo(BytesPng, "image/png", "foto.png");

        var envio = await client.PostAsync("/admin/midias", formulario, CancellationToken.None);

        envio.StatusCode.ShouldBe(HttpStatusCode.Created);
        var midia = await envio.Content.ReadFromJsonAsync<MidiaResponse>(CancellationToken.None);
        midia.ShouldNotBeNull();
        midia.Url.ShouldBe($"/midias/{midia.Id}");
        midia.Tamanho.ShouldBe(BytesPng.Length);
        envio.Headers.Location!.ToString().ShouldBe(midia.Url);

        // GET publico (sem token) serve o mesmo conteudo com cache imutavel.
        var publico = fixture.CreateClient();
        var download = await publico.GetAsync(midia.Url, CancellationToken.None);

        download.StatusCode.ShouldBe(HttpStatusCode.OK);
        download.Content.Headers.ContentType!.MediaType.ShouldBe("image/png");
        (await download.Content.ReadAsByteArrayAsync(CancellationToken.None)).ShouldBe(BytesPng);
        download.Headers.CacheControl!.Public.ShouldBeTrue();
        download.Headers.CacheControl.Extensions.ShouldContain(x => x.Name == "immutable");
        download.Headers.ETag.ShouldNotBeNull();

        // Stream seekable no GridFS: sem isso a resposta sai chunked, sem Content-Length.
        download.Content.Headers.ContentLength.ShouldBe(BytesPng.Length);
    }

    /// <summary>Content-type com caixa e parametros e legal por RFC e precisa ser aceito e normalizado.</summary>
    [Fact]
    public async Task Enviar_contentTypeComCaixaEParametros_aceitaENormaliza()
    {
        var client = await fixture.CriarClienteAutenticadoAsync();
        using var formulario = FormularioComArquivo(BytesPng, "IMAGE/PNG; charset=binary", "foto.png");

        var envio = await client.PostAsync("/admin/midias", formulario, CancellationToken.None);

        envio.StatusCode.ShouldBe(HttpStatusCode.Created);
        var midia = await envio.Content.ReadFromJsonAsync<MidiaResponse>(CancellationToken.None);
        midia!.ContentType.ShouldBe("image/png");

        var download = await fixture.CreateClient().GetAsync(midia.Url, CancellationToken.None);
        download.Content.Headers.ContentType!.MediaType.ShouldBe("image/png");
    }

    [Fact]
    public async Task Obter_comIfNoneMatchDoETagCorreto_retorna304()
    {
        var midia = await EnviarPngAsync();

        var publico = fixture.CreateClient();
        var primeiraLeitura = await publico.GetAsync(midia.Url, CancellationToken.None);
        var etag = primeiraLeitura.Headers.ETag!;

        using var segundaRequisicao = new HttpRequestMessage(HttpMethod.Get, midia.Url);
        segundaRequisicao.Headers.IfNoneMatch.Add(etag);
        var segundaLeitura = await publico.SendAsync(segundaRequisicao, CancellationToken.None);

        segundaLeitura.StatusCode.ShouldBe(HttpStatusCode.NotModified);
    }

    /// <summary>O negativo do 304: ETag que nao corresponde tem de devolver o corpo, nao 304.</summary>
    [Fact]
    public async Task Obter_comIfNoneMatchDeOutroETag_retorna200ComOCorpo()
    {
        var midia = await EnviarPngAsync();

        using var requisicao = new HttpRequestMessage(HttpMethod.Get, midia.Url);
        requisicao.Headers.IfNoneMatch.Add(new EntityTagHeaderValue($"\"{Guid.NewGuid()}\""));
        var leitura = await fixture.CreateClient().SendAsync(requisicao, CancellationToken.None);

        leitura.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await leitura.Content.ReadAsByteArrayAsync(CancellationToken.None)).ShouldBe(BytesPng);
    }

    [Fact]
    public async Task Obter_inexistente_retorna404()
    {
        var publico = fixture.CreateClient();

        var resposta = await publico.GetAsync($"/midias/{Guid.NewGuid()}", CancellationToken.None);

        resposta.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Remover_midiaExistente_retorna204EDepoisNaoServeMais()
    {
        var client = await fixture.CriarClienteAutenticadoAsync();
        var midia = await EnviarPngAsync();

        var remocao = await client.DeleteAsync($"/admin/midias/{midia.Id}", CancellationToken.None);
        remocao.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var leitura = await fixture.CreateClient().GetAsync(midia.Url, CancellationToken.None);
        leitura.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Remover_semToken_retorna401()
    {
        var resposta = await fixture.CreateClient()
            .DeleteAsync($"/admin/midias/{Guid.NewGuid()}", CancellationToken.None);

        resposta.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Remover_inexistente_retorna404()
    {
        var client = await fixture.CriarClienteAutenticadoAsync();

        var resposta = await client.DeleteAsync($"/admin/midias/{Guid.NewGuid()}", CancellationToken.None);

        resposta.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Enviar_semArquivoNoFormulario_retorna400()
    {
        var client = await fixture.CriarClienteAutenticadoAsync();
        using var formulario = new MultipartFormDataContent();

        var resposta = await client.PostAsync("/admin/midias", formulario, CancellationToken.None);

        resposta.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Enviar_tipoNaoPermitido_retorna400()
    {
        var client = await fixture.CriarClienteAutenticadoAsync();
        using var formulario = FormularioComArquivo(BytesPng, "application/pdf", "arquivo.pdf");

        var resposta = await client.PostAsync("/admin/midias", formulario, CancellationToken.None);

        resposta.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Enviar_conteudoNaoBateComTipoDeclarado_retorna400()
    {
        var client = await fixture.CriarClienteAutenticadoAsync();
        var bytesJpeg = new byte[] { 0xFF, 0xD8, 0xFF, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        using var formulario = FormularioComArquivo(bytesJpeg, "image/png", "foto.png"); // JPEG disfarcado de PNG

        var resposta = await client.PostAsync("/admin/midias", formulario, CancellationToken.None);

        resposta.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Enviar_acimaDoLimiteDeTamanho_recusaENaoCriaAMidia()
    {
        var client = await fixture.CriarClienteAutenticadoAsync();
        var conteudoGrande = new byte[6 * 1024 * 1024]; // acima do limite de negocio (5MB)
        BytesPng.CopyTo(conteudoGrande, 0);
        using var formulario = FormularioComArquivo(conteudoGrande, "image/png", "grande.png");

        var resposta = await client.PostAsync("/admin/midias", formulario, CancellationToken.None);

        // Status especifico, e nao so "nao teve sucesso": afirmar apenas o negativo
        // deixava o teste passar com 401/429/500, ou seja, verde por motivo errado.
        // O corte pode vir do Kestrel (413, antes de ler o form) ou do validador de
        // negocio (400, depois) - depende de onde o limite bate primeiro.
        resposta.StatusCode.ShouldBeOneOf(HttpStatusCode.RequestEntityTooLarge, HttpStatusCode.BadRequest);

        // E o que o nome do teste promete: nenhuma midia foi criada.
        var corpo = await resposta.Content.ReadAsStringAsync(CancellationToken.None);
        corpo.ShouldNotContain("/midias/");
    }

    private async Task<MidiaResponse> EnviarPngAsync()
    {
        var client = await fixture.CriarClienteAutenticadoAsync();
        using var formulario = FormularioComArquivo(BytesPng, "image/png", "foto.png");

        var envio = await client.PostAsync("/admin/midias", formulario, CancellationToken.None);
        envio.StatusCode.ShouldBe(HttpStatusCode.Created);

        var midia = await envio.Content.ReadFromJsonAsync<MidiaResponse>(CancellationToken.None);
        midia.ShouldNotBeNull();
        return midia;
    }

    private static MultipartFormDataContent FormularioComArquivo(byte[] bytes, string contentType, string nomeArquivo)
    {
        var conteudo = new ByteArrayContent(bytes);
        // TryParse: content-type com parametros ("image/png; charset=binary") nao
        // passa pelo construtor de MediaTypeHeaderValue.
        conteudo.Headers.ContentType = MediaTypeHeaderValue.Parse(contentType);

        return new MultipartFormDataContent
        {
            { conteudo, MidiaAdminEndpoints.CampoArquivo, nomeArquivo },
        };
    }
}
