using System.Net;
using System.Net.Http.Json;
using Levante.Api.Endpoints;
using Levante.Api.IntegrationTests.Fixtures;
using MongoDB.Bson;
using MongoDB.Driver;
using Shouldly;
using Xunit;

namespace Levante.Api.IntegrationTests;

/// <summary>
/// Endpoints publicos de newsletter (contexto Audiencia). Fluxo double opt-in:
/// solicitar nasce Pendente + evento no Outbox; confirmar por token -> Confirmado.
/// Honeypot descarta bots; token invalido -> 404. Nada revela se um e-mail existe.
/// </summary>
[Trait("Category", "Integration")]
public sealed class NewsletterEndpointTests(ApiAppFixture fixture) : IClassFixture<ApiAppFixture>
{
    private IMongoDatabase Banco() =>
        new MongoClient(fixture.MongoConnectionString).GetDatabase("levante");

    [Fact]
    public async Task Fluxo_solicita_gravaPendenteEEvento_confirmaPorToken()
    {
        var client = fixture.CreateClient();
        var email = $"pessoa-{Guid.NewGuid():N}@exemplo.com";

        // Solicita -> 202 (aguardando confirmacao).
        var solicitacao = await client.PostAsJsonAsync(
            "/newsletter", new SolicitarNewsletterRequest(email), CancellationToken.None);
        solicitacao.StatusCode.ShouldBe(HttpStatusCode.Accepted);

        // Gravou o assinante Pendente com um token.
        var assinantes = Banco().GetCollection<BsonDocument>("assinantes");
        var doc = await assinantes.Find(Builders<BsonDocument>.Filter.Eq("email", email))
            .FirstOrDefaultAsync(CancellationToken.None);
        doc.ShouldNotBeNull();
        doc["status"].AsString.ShouldBe("Pendente");
        var token = doc["token"].AsString;
        token.ShouldNotBeNullOrWhiteSpace();

        // Gravou o evento AssinaturaSolicitada no Outbox (mesma escrita).
        var outbox = Banco().GetCollection<BsonDocument>("outbox");
        var evento = await outbox
            .Find(Builders<BsonDocument>.Filter.Eq("tipo", "AssinaturaSolicitada"))
            .ToListAsync(CancellationToken.None);
        evento.ShouldContain(e => e["dados"]["email"].AsString == email);

        // Confirma pelo token -> 200 e status Confirmado.
        var confirmacao = await client.PostAsJsonAsync(
            "/newsletter/confirmar", new ConfirmarNewsletterRequest(token), CancellationToken.None);
        confirmacao.StatusCode.ShouldBe(HttpStatusCode.OK);

        var confirmado = await assinantes.Find(Builders<BsonDocument>.Filter.Eq("email", email))
            .FirstOrDefaultAsync(CancellationToken.None);
        confirmado["status"].AsString.ShouldBe("Confirmado");
    }

    [Fact]
    public async Task Solicitar_comHoneypot_ehDescartado()
    {
        var client = fixture.CreateClient();
        var email = $"bot-{Guid.NewGuid():N}@exemplo.com";

        var resposta = await client.PostAsJsonAsync(
            "/newsletter", new SolicitarNewsletterRequest(email, Armadilha: "preenchido"), CancellationToken.None);
        resposta.StatusCode.ShouldBe(HttpStatusCode.Accepted); // aceito em silencio

        var assinantes = Banco().GetCollection<BsonDocument>("assinantes");
        var doc = await assinantes.Find(Builders<BsonDocument>.Filter.Eq("email", email))
            .FirstOrDefaultAsync(CancellationToken.None);
        doc.ShouldBeNull(); // nada persistido
    }

    [Fact]
    public async Task Solicitar_emailInvalido_retorna400()
    {
        var client = fixture.CreateClient();

        var resposta = await client.PostAsJsonAsync(
            "/newsletter", new SolicitarNewsletterRequest("sem-arroba"), CancellationToken.None);

        resposta.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Confirmar_tokenInvalido_retorna404()
    {
        var client = fixture.CreateClient();

        var resposta = await client.PostAsJsonAsync(
            "/newsletter/confirmar", new ConfirmarNewsletterRequest(new string('a', 40)), CancellationToken.None);

        resposta.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}
