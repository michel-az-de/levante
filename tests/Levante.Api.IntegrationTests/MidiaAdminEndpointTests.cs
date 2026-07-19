using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Levante.Api.Endpoints;
using Levante.Api.IntegrationTests.Fixtures;
using Levante.Conteudo.Application.Midias;
using Levante.Identity.Application.Autenticacao;
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
        var client = await ClienteAutenticadoAsync();
        using var formulario = FormularioComArquivo(BytesPng, "image/png", "foto.png");

        var envio = await client.PostAsync("/admin/midias", formulario, CancellationToken.None);

        envio.StatusCode.ShouldBe(HttpStatusCode.Created);
        var midia = await envio.Content.ReadFromJsonAsync<MidiaResponse>(CancellationToken.None);
        midia.ShouldNotBeNull();
        midia.Url.ShouldBe($"/midias/{midia.Id}");
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
    }

    [Fact]
    public async Task Obter_comIfNoneMatchDoETagCorreto_retorna304()
    {
        var client = await ClienteAutenticadoAsync();
        using var formulario = FormularioComArquivo(BytesPng, "image/png", "foto.png");
        var envio = await client.PostAsync("/admin/midias", formulario, CancellationToken.None);
        var midia = await envio.Content.ReadFromJsonAsync<MidiaResponse>(CancellationToken.None);

        var publico = fixture.CreateClient();
        var primeiraLeitura = await publico.GetAsync(midia!.Url, CancellationToken.None);
        var etag = primeiraLeitura.Headers.ETag!;

        using var segundaRequisicao = new HttpRequestMessage(HttpMethod.Get, midia.Url);
        segundaRequisicao.Headers.IfNoneMatch.Add(etag);
        var segundaLeitura = await publico.SendAsync(segundaRequisicao, CancellationToken.None);

        segundaLeitura.StatusCode.ShouldBe(HttpStatusCode.NotModified);
    }

    [Fact]
    public async Task Obter_inexistente_retorna404()
    {
        var publico = fixture.CreateClient();

        var resposta = await publico.GetAsync($"/midias/{Guid.NewGuid()}", CancellationToken.None);

        resposta.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Enviar_semArquivoNoFormulario_retorna400()
    {
        var client = await ClienteAutenticadoAsync();
        using var formulario = new MultipartFormDataContent();

        var resposta = await client.PostAsync("/admin/midias", formulario, CancellationToken.None);

        resposta.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Enviar_tipoNaoPermitido_retorna400()
    {
        var client = await ClienteAutenticadoAsync();
        using var formulario = FormularioComArquivo(BytesPng, "application/pdf", "arquivo.pdf");

        var resposta = await client.PostAsync("/admin/midias", formulario, CancellationToken.None);

        resposta.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Enviar_conteudoNaoBateComTipoDeclarado_retorna400()
    {
        var client = await ClienteAutenticadoAsync();
        var bytesJpeg = new byte[] { 0xFF, 0xD8, 0xFF, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        using var formulario = FormularioComArquivo(bytesJpeg, "image/png", "foto.png"); // JPEG disfarcado de PNG

        var resposta = await client.PostAsync("/admin/midias", formulario, CancellationToken.None);

        resposta.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Enviar_acimaDoLimiteDeTamanho_naoCriaAMidia()
    {
        var client = await ClienteAutenticadoAsync();
        var conteudoGrande = new byte[6 * 1024 * 1024]; // acima do limite de negocio (5MB)
        BytesPng.CopyTo(conteudoGrande, 0);
        using var formulario = FormularioComArquivo(conteudoGrande, "image/png", "grande.png");

        var resposta = await client.PostAsync("/admin/midias", formulario, CancellationToken.None);

        // O corte pode acontecer no Kestrel (413, corpo grande demais, antes de
        // ler o form) ou no validador de negocio (400, apos ler) - depende de
        // onde o limite bate primeiro. Em qualquer caso, nunca deve criar a midia.
        resposta.IsSuccessStatusCode.ShouldBeFalse();
    }

    private static MultipartFormDataContent FormularioComArquivo(byte[] bytes, string contentType, string nomeArquivo)
    {
        var conteudo = new ByteArrayContent(bytes);
        conteudo.Headers.ContentType = new MediaTypeHeaderValue(contentType);

        return new MultipartFormDataContent
        {
            { conteudo, MidiaAdminEndpoints.CampoArquivo, nomeArquivo },
        };
    }

    // Cacheado por classe: PolicyAuth limita /auth/login a 5 req/min por IP, e
    // esta classe tem mais de 5 testes autenticados - logar a cada teste
    // estourava o limite (429) de forma intermitente. Um token so, reusado.
    private string? _tokenCache;

    private async Task<HttpClient> ClienteAutenticadoAsync()
    {
        var client = fixture.CreateClient();
        _tokenCache ??= await ObterTokenAsync();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _tokenCache);
        return client;
    }

    private async Task<string> ObterTokenAsync()
    {
        var client = fixture.CreateClient();
        var login = await client.PostAsJsonAsync(
            "/auth/login",
            new AutenticarRequest(ApiAppFixture.EmailAdmin, ApiAppFixture.SenhaAdmin),
            CancellationToken.None);
        var token = await login.Content.ReadFromJsonAsync<TokenDeAcessoResponse>(CancellationToken.None);
        token.ShouldNotBeNull();

        return token.AccessToken;
    }
}
