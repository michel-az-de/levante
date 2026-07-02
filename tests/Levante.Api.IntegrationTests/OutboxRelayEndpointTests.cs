using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Levante.Api.Endpoints;
using Levante.Api.IntegrationTests.Fixtures;
using Levante.Conteudo.Application.Artigos;
using Levante.Identity.Application.Autenticacao;
using RabbitMQ.Client;
using Shouldly;
using Xunit;

namespace Levante.Api.IntegrationTests;

/// <summary>
/// Caminho do Outbox ponta a ponta: publicar um artigo grava o evento na mesma
/// transacao (Mongo replica set), o relay observa por Change Stream e publica no
/// RabbitMQ. Um consumidor de teste recebe o envelope. So roda no CI (Docker).
/// </summary>
[Trait("Category", "Integration")]
public sealed class OutboxRelayEndpointTests(OutboxRelayFixture fixture) : IClassFixture<OutboxRelayFixture>
{
    [Fact]
    public async Task PublicarArtigo_publicaEventoArtigoPublicadoNoRabbit()
    {
        // Consumidor: fila temporaria ligada ao exchange (declara o exchange de forma idempotente).
        await using var conexao = await new ConnectionFactory
        {
            Uri = new Uri(fixture.RabbitConnectionString),
        }.CreateConnectionAsync(CancellationToken.None);
        await using var canal = await conexao.CreateChannelAsync(cancellationToken: CancellationToken.None);
        await canal.ExchangeDeclareAsync(
            OutboxRelayFixture.RabbitExchange, ExchangeType.Topic, durable: true, autoDelete: false,
            cancellationToken: CancellationToken.None);
        var fila = await canal.QueueDeclareAsync(cancellationToken: CancellationToken.None);
        await canal.QueueBindAsync(
            fila.QueueName, OutboxRelayFixture.RabbitExchange, routingKey: "#",
            cancellationToken: CancellationToken.None);

        // Publica um artigo (dispara ArtigoPublicado -> outbox -> relay).
        var client = await ClienteAutenticadoAsync();
        var slug = "artigo-outbox-" + Guid.NewGuid().ToString("N")[..8];

        var criacao = await client.PostAsJsonAsync(
            "/artigos", new CriarArtigoRequest("Titulo", slug, "Resumo.", "Conteudo."), CancellationToken.None);
        criacao.EnsureSuccessStatusCode();
        var artigo = await criacao.Content.ReadFromJsonAsync<ArtigoResponse>(CancellationToken.None);
        artigo.ShouldNotBeNull();

        var publicacao = await client.PostAsync($"/artigos/{artigo.Id}/publicar", content: null, CancellationToken.None);
        publicacao.EnsureSuccessStatusCode();

        // Aguarda o evento chegar na fila (o relay publica de forma assincrona).
        var mensagem = await AguardarMensagemAsync(canal, fila.QueueName, TimeSpan.FromSeconds(25));
        mensagem.ShouldNotBeNull();

        using var envelope = JsonDocument.Parse(mensagem);
        envelope.RootElement.GetProperty("tipo").GetString().ShouldBe("ArtigoPublicado");
        envelope.RootElement.GetProperty("eventId").GetString().ShouldNotBeNullOrWhiteSpace();
        envelope.RootElement.GetProperty("dados").GetProperty("slug").GetString().ShouldBe(slug);
    }

    private static async Task<string?> AguardarMensagemAsync(IChannel canal, string fila, TimeSpan limite)
    {
        var prazo = DateTime.UtcNow + limite;
        while (DateTime.UtcNow < prazo)
        {
            var resultado = await canal.BasicGetAsync(fila, autoAck: true, CancellationToken.None);
            if (resultado is not null)
            {
                return Encoding.UTF8.GetString(resultado.Body.Span);
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
