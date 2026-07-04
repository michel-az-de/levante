using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Levante.Api.Endpoints;
using Levante.Api.IntegrationTests.Fixtures;
using Levante.Conteudo.Application.Artigos;
using Levante.Identity.Application.Autenticacao;
using MongoDB.Bson;
using MongoDB.Driver;
using Shouldly;
using Xunit;

namespace Levante.Api.IntegrationTests;

/// <summary>
/// Emissao do Outbox ponta a ponta (ADR 0002): um evento de dominio grava a emissao na
/// mesma escrita do agregado; o relay (flag-based) le pendente por emissionSeq, faz
/// POST /v1/events no stub do Hiram e MARCA o resultado. So roda no CI (Docker: Mongo).
/// </summary>
[Trait("Category", "Integration")]
public sealed class OutboxRelayEndpointTests(OutboxRelayFixture fixture) : IClassFixture<OutboxRelayFixture>
{
    private const int Pendente = 0;
    private const int Enviada = 1;
    private const int Falhada = 2;
    private const int Ignorada = 3;

    private IMongoCollection<BsonDocument> Outbox() =>
        new MongoClient(fixture.MongoConnectionString).GetDatabase("levante").GetCollection<BsonDocument>("outbox");

    [Fact]
    public async Task AssinaturaSolicitada_emitida_marcaEnviada_comContratoCorreto()
    {
        fixture.StubAceito();
        var client = fixture.CreateClient();
        var email = $"pessoa-{Guid.NewGuid():N}@exemplo.com";

        var resposta = await client.PostAsJsonAsync(
            "/newsletter", new SolicitarNewsletterRequest(email), CancellationToken.None);
        resposta.StatusCode.ShouldBe(HttpStatusCode.Accepted);

        var doc = await AguardarAsync(PorEmail(email), d => Status(d) == Enviada, TimeSpan.FromSeconds(30));
        doc.ShouldNotBeNull();
        doc["enviadoEm"].IsBsonNull.ShouldBeFalse();
        doc["emissionSeq"].ToInt64().ShouldBeGreaterThan(0);

        var requisicao = fixture.RequisicoesEmissao()
            .FirstOrDefault(e => e.RequestMessage?.Body?.Contains(email, StringComparison.Ordinal) == true);
        var mensagem = requisicao.ShouldNotBeNull().RequestMessage.ShouldNotBeNull();
        var headers = mensagem.Headers.ShouldNotBeNull();
        headers["X-Api-Key"].ShouldContain(OutboxRelayFixture.ApiKey);
        headers.ContainsKey("Idempotency-Key").ShouldBeTrue();

        using var corpo = JsonDocument.Parse(mensagem.Body.ShouldNotBeNull());
        var raiz = corpo.RootElement;
        raiz.GetProperty("eventType").GetString().ShouldBe("assinatura_solicitada");
        raiz.GetProperty("recipient").GetProperty("email").GetString().ShouldBe(email);
        raiz.GetProperty("data").GetProperty("token").GetString().ShouldNotBeNullOrWhiteSpace();
        raiz.GetProperty("data").GetProperty("confirmUrlBase").GetString()
            .ShouldBe($"{OutboxRelayFixture.SiteUrl}/newsletter/confirmar");
        raiz.GetProperty("emissionSeq").GetInt64().ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task Emissao_com5xx_naoPerde_ficaPendente_eReprograma()
    {
        fixture.StubStatus(500);
        var client = fixture.CreateClient();
        var email = $"erro5xx-{Guid.NewGuid():N}@exemplo.com";

        await client.PostAsJsonAsync("/newsletter", new SolicitarNewsletterRequest(email), CancellationToken.None);

        var doc = await AguardarAsync(
            PorEmail(email), d => Status(d) == Pendente && d["tentativas"].ToInt32() > 0, TimeSpan.FromSeconds(20));
        doc.ShouldNotBeNull();
        doc["proximaTentativaEm"].IsBsonNull.ShouldBeFalse();
    }

    [Fact]
    public async Task Emissao_com400_marcaFalhada()
    {
        fixture.StubStatus(400);
        var client = fixture.CreateClient();
        var email = $"erro400-{Guid.NewGuid():N}@exemplo.com";

        await client.PostAsJsonAsync("/newsletter", new SolicitarNewsletterRequest(email), CancellationToken.None);

        var doc = await AguardarAsync(PorEmail(email), d => Status(d) == Falhada, TimeSpan.FromSeconds(20));
        doc.ShouldNotBeNull();
        doc["erroUltimaTentativa"].AsString.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Replay_202ComHeader_ehTratadoComoSucesso()
    {
        fixture.StubAceito(replay: true);
        var client = fixture.CreateClient();
        var email = $"replay-{Guid.NewGuid():N}@exemplo.com";

        await client.PostAsJsonAsync("/newsletter", new SolicitarNewsletterRequest(email), CancellationToken.None);

        var doc = await AguardarAsync(PorEmail(email), d => Status(d) == Enviada, TimeSpan.FromSeconds(30));
        doc.ShouldNotBeNull();
    }

    [Fact]
    public async Task ArtigoPublicado_semMapeador_marcaIgnorada_semPostAoHiram()
    {
        fixture.StubAceito();
        var client = await ClienteAutenticadoAsync();
        var slug = "artigo-" + Guid.NewGuid().ToString("N")[..8];

        var criacao = await client.PostAsJsonAsync(
            "/artigos", new CriarArtigoRequest("Titulo", slug, "Resumo.", "Conteudo."), CancellationToken.None);
        criacao.EnsureSuccessStatusCode();
        var artigo = await criacao.Content.ReadFromJsonAsync<ArtigoResponse>(CancellationToken.None);
        artigo.ShouldNotBeNull();
        (await client.PostAsync($"/artigos/{artigo.Id}/publicar", content: null, CancellationToken.None))
            .EnsureSuccessStatusCode();

        var filtro = Builders<BsonDocument>.Filter.And(
            Builders<BsonDocument>.Filter.Eq("tipo", "ArtigoPublicado"),
            Builders<BsonDocument>.Filter.Eq("dados.slug", slug));
        var doc = await AguardarAsync(filtro, d => Status(d) == Ignorada, TimeSpan.FromSeconds(20));
        doc.ShouldNotBeNull();

        fixture.RequisicoesEmissao()
            .Any(e => e.RequestMessage?.Body is string corpo && corpo.Contains(slug, StringComparison.Ordinal))
            .ShouldBeFalse();
    }

    private static FilterDefinition<BsonDocument> PorEmail(string email) =>
        Builders<BsonDocument>.Filter.Eq("dados.email", email);

    private static int Status(BsonDocument doc) => doc["status"].ToInt32();

    private async Task<BsonDocument?> AguardarAsync(
        FilterDefinition<BsonDocument> filtro, Func<BsonDocument, bool> condicao, TimeSpan limite)
    {
        var prazo = DateTime.UtcNow + limite;
        while (DateTime.UtcNow < prazo)
        {
            var doc = await Outbox().Find(filtro).FirstOrDefaultAsync(CancellationToken.None);
            if (doc is not null && condicao(doc))
            {
                return doc;
            }

            await Task.Delay(300, CancellationToken.None);
        }

        return null;
    }

    private async Task<HttpClient> ClienteAutenticadoAsync()
    {
        var client = fixture.CreateClient();
        var login = await client.PostAsJsonAsync(
            "/auth/login",
            new AutenticarRequest("admin@levante.dev", "Senha-de-teste-Forte-123"),
            CancellationToken.None);
        var token = await login.Content.ReadFromJsonAsync<TokenDeAcessoResponse>(CancellationToken.None);
        token.ShouldNotBeNull();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);
        return client;
    }
}
