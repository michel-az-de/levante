using System.Net;
using System.Net.Http.Json;
using Levante.Api.Endpoints;
using Levante.Api.IntegrationTests.Fixtures;
using Shouldly;
using Xunit;

namespace Levante.Api.IntegrationTests;

/// <summary>
/// Endpoints publicos de reacao (contexto Engajamento). A identidade do visitante
/// vem do header X-Visitante (posto pelo BFF do Next); a unicidade e por visitante.
/// </summary>
[Trait("Category", "Integration")]
public sealed class ReacaoEndpointTests(ApiAppFixture fixture) : IClassFixture<ApiAppFixture>
{
    private sealed record ReacoesDto(int Curtir, int Amei, int Relevante, string[] Minhas);

    [Fact]
    public async Task Reagir_conta_dedupPorVisitante_eRemover()
    {
        var client = fixture.CreateClient();
        var artigoId = Guid.NewGuid();

        // v-1 curte -> 1, e aparece em "minhas".
        var apos1 = await ReagirAsync(client, artigoId, "v-1", "Curtir");
        apos1.Curtir.ShouldBe(1);
        apos1.Minhas.ShouldContain("Curtir");

        // v-1 curte de novo o mesmo tipo -> idempotente (continua 1).
        var apos1DeNovo = await ReagirAsync(client, artigoId, "v-1", "Curtir");
        apos1DeNovo.Curtir.ShouldBe(1);

        // v-2 curte -> 2 (visitante distinto soma).
        var apos2 = await ReagirAsync(client, artigoId, "v-2", "Curtir");
        apos2.Curtir.ShouldBe(2);

        // GET como v-1: conta 2, "minhas" = [Curtir].
        var lido = await ObterAsync(client, artigoId, "v-1");
        lido.Curtir.ShouldBe(2);
        lido.Minhas.ShouldBe(["Curtir"]);

        // v-1 remove -> 1, sem "minhas".
        var aposRemover = await RemoverAsync(client, artigoId, "v-1", "Curtir");
        aposRemover.Curtir.ShouldBe(1);
        aposRemover.Minhas.ShouldBeEmpty();
    }

    [Fact]
    public async Task Reagir_tipoInvalido_retorna400()
    {
        var client = fixture.CreateClient();
        using var requisicao = new HttpRequestMessage(
            HttpMethod.Post, $"/artigos/{Guid.NewGuid()}/reacoes")
        {
            Content = JsonContent.Create(new RegistrarReacaoRequest("Detestei")),
        };
        requisicao.Headers.Add("X-Visitante", "v-1");

        var resposta = await client.SendAsync(requisicao, CancellationToken.None);

        resposta.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    private static async Task<ReacoesDto> ReagirAsync(HttpClient client, Guid artigoId, string visitante, string tipo)
    {
        using var requisicao = new HttpRequestMessage(HttpMethod.Post, $"/artigos/{artigoId}/reacoes")
        {
            Content = JsonContent.Create(new RegistrarReacaoRequest(tipo)),
        };
        requisicao.Headers.Add("X-Visitante", visitante);

        var resposta = await client.SendAsync(requisicao, CancellationToken.None);
        resposta.StatusCode.ShouldBe(HttpStatusCode.OK);
        return await LerAsync(resposta);
    }

    private static async Task<ReacoesDto> RemoverAsync(HttpClient client, Guid artigoId, string visitante, string tipo)
    {
        using var requisicao = new HttpRequestMessage(HttpMethod.Delete, $"/artigos/{artigoId}/reacoes/{tipo}");
        requisicao.Headers.Add("X-Visitante", visitante);

        var resposta = await client.SendAsync(requisicao, CancellationToken.None);
        resposta.StatusCode.ShouldBe(HttpStatusCode.OK);
        return await LerAsync(resposta);
    }

    private static async Task<ReacoesDto> ObterAsync(HttpClient client, Guid artigoId, string visitante)
    {
        using var requisicao = new HttpRequestMessage(HttpMethod.Get, $"/artigos/{artigoId}/reacoes");
        requisicao.Headers.Add("X-Visitante", visitante);

        var resposta = await client.SendAsync(requisicao, CancellationToken.None);
        resposta.StatusCode.ShouldBe(HttpStatusCode.OK);
        return await LerAsync(resposta);
    }

    private static async Task<ReacoesDto> LerAsync(HttpResponseMessage resposta)
    {
        var dto = await resposta.Content.ReadFromJsonAsync<ReacoesDto>(CancellationToken.None);
        dto.ShouldNotBeNull();
        return dto;
    }
}
