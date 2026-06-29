using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Levante.Api.Endpoints;
using Levante.Api.IntegrationTests.Fixtures;
using Levante.Identity.Application.Autenticacao;
using Shouldly;
using Xunit;

namespace Levante.Api.IntegrationTests;

[Trait("Category", "Integration")]
public sealed class AuthEndpointTests(ApiAppFixture fixture) : IClassFixture<ApiAppFixture>
{
    [Fact]
    public async Task Login_comCredenciaisValidas_retornaToken()
    {
        var client = fixture.CreateClient();

        var resposta = await client.PostAsJsonAsync(
            "/auth/login",
            new AutenticarRequest(ApiAppFixture.EmailAdmin, ApiAppFixture.SenhaAdmin),
            CancellationToken.None);

        resposta.StatusCode.ShouldBe(HttpStatusCode.OK);
        var token = await resposta.Content.ReadFromJsonAsync<TokenDeAcessoResponse>(CancellationToken.None);
        token.ShouldNotBeNull();
        token.AccessToken.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Login_comSenhaErrada_retorna401()
    {
        var client = fixture.CreateClient();

        var resposta = await client.PostAsJsonAsync(
            "/auth/login",
            new AutenticarRequest(ApiAppFixture.EmailAdmin, "senha-errada"),
            CancellationToken.None);

        resposta.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Eu_semToken_retorna401()
    {
        var client = fixture.CreateClient();

        var resposta = await client.GetAsync("/auth/eu", CancellationToken.None);

        resposta.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Eu_comToken_retornaEmailDoAdmin()
    {
        var client = fixture.CreateClient();
        var login = await client.PostAsJsonAsync(
            "/auth/login",
            new AutenticarRequest(ApiAppFixture.EmailAdmin, ApiAppFixture.SenhaAdmin),
            CancellationToken.None);
        var token = await login.Content.ReadFromJsonAsync<TokenDeAcessoResponse>(CancellationToken.None);
        token.ShouldNotBeNull();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);
        var resposta = await client.GetAsync("/auth/eu", CancellationToken.None);

        resposta.StatusCode.ShouldBe(HttpStatusCode.OK);
        var eu = await resposta.Content.ReadFromJsonAsync<AdministradorAtualResponse>(CancellationToken.None);
        eu.ShouldNotBeNull();
        eu.Email.ShouldBe(ApiAppFixture.EmailAdmin);
    }
}
