using System.Net;
using System.Net.Http.Json;
using Levante.Api.Endpoints;
using Levante.Api.IntegrationTests.Fixtures;
using Shouldly;
using Xunit;

namespace Levante.Api.IntegrationTests;

/// <summary>
/// Endpoints de comentario (contexto Engajamento). Criacao publica nasce Pendente
/// (invisivel ate moderar); moderacao exige JWT. Honeypot descarta bots.
/// </summary>
[Trait("Category", "Integration")]
public sealed class ComentarioEndpointTests(ApiAppFixture fixture) : IClassFixture<ApiAppFixture>
{
    private sealed record ComentarioDto(
        Guid Id, Guid ArtigoId, string ArtigoSlug, string Autor, string Texto, string Status, DateTime DataCriacao);

    [Fact]
    public async Task Fluxo_criaPendente_invisivel_adminAprova_apareceNoPublico()
    {
        var client = fixture.CreateClient();
        var artigoId = Guid.NewGuid();

        // Publico cria -> 202 (aguardando moderacao).
        var criacao = await CriarAsync(client, artigoId, "Ana", "Otimo artigo!");
        criacao.StatusCode.ShouldBe(HttpStatusCode.Accepted);

        // Ainda nao aparece no publico (esta pendente).
        var publicoAntes = await ListarPublicoAsync(client, artigoId);
        publicoAntes.ShouldBeEmpty();

        // Admin ve na fila, pega o id e aprova.
        var adminClient = await fixture.CriarClienteAutenticadoAsync();
        var pendentes = await ListarPendentesAsync(adminClient);
        var meu = pendentes.ShouldHaveSingleItem();
        meu.Autor.ShouldBe("Ana");
        meu.Status.ShouldBe("Pendente");

        var aprovacao = await adminClient.PostAsync(
            $"/admin/comentarios/{meu.Id}/aprovar", content: null, CancellationToken.None);
        aprovacao.StatusCode.ShouldBe(HttpStatusCode.OK);

        // Agora aparece no publico.
        var publicoDepois = await ListarPublicoAsync(client, artigoId);
        publicoDepois.ShouldContain(c => c.Autor == "Ana" && c.Status == "Aprovado");
    }

    [Fact]
    public async Task Criar_comHoneypot_ehDescartado()
    {
        var client = fixture.CreateClient();
        var artigoId = Guid.NewGuid();

        using var requisicao = new HttpRequestMessage(HttpMethod.Post, $"/artigos/{artigoId}/comentarios")
        {
            Content = JsonContent.Create(new CriarComentarioRequest("meu-artigo", "Bot", "spam", Armadilha: "preenchido")),
        };
        requisicao.Headers.Add("X-Visitante", "v-bot");

        var resposta = await client.SendAsync(requisicao, CancellationToken.None);
        resposta.StatusCode.ShouldBe(HttpStatusCode.Accepted); // aceito em silencio

        // Nada entrou na fila de moderacao por causa desse artigo.
        var adminClient = await fixture.CriarClienteAutenticadoAsync();
        var pendentes = await ListarPendentesAsync(adminClient);
        pendentes.ShouldNotContain(c => c.ArtigoId == artigoId);
    }

    [Fact]
    public async Task Moderacao_semToken_retorna401()
    {
        var client = fixture.CreateClient();

        var lista = await client.GetAsync("/admin/comentarios", CancellationToken.None);
        lista.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);

        var aprovar = await client.PostAsync(
            $"/admin/comentarios/{Guid.NewGuid()}/aprovar", content: null, CancellationToken.None);
        aprovar.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    private static async Task<HttpResponseMessage> CriarAsync(HttpClient client, Guid artigoId, string autor, string texto)
    {
        using var requisicao = new HttpRequestMessage(HttpMethod.Post, $"/artigos/{artigoId}/comentarios")
        {
            Content = JsonContent.Create(new CriarComentarioRequest("meu-artigo", autor, texto)),
        };
        requisicao.Headers.Add("X-Visitante", "v-1");
        return await client.SendAsync(requisicao, CancellationToken.None);
    }

    private static async Task<List<ComentarioDto>> ListarPublicoAsync(HttpClient client, Guid artigoId)
    {
        var lista = await client.GetFromJsonAsync<List<ComentarioDto>>(
            $"/artigos/{artigoId}/comentarios", CancellationToken.None);
        return lista ?? [];
    }

    private static async Task<List<ComentarioDto>> ListarPendentesAsync(HttpClient client)
    {
        var lista = await client.GetFromJsonAsync<List<ComentarioDto>>("/admin/comentarios", CancellationToken.None);
        return lista ?? [];
    }

}
