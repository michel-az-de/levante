using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Testcontainers.MongoDb;
using WireMock.Logging;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using Xunit;

namespace Levante.Api.IntegrationTests.Fixtures;

/// <summary>
/// Sobe a API com o relay do Outbox LIGADO, sobre um Mongo efemero e um stub HTTP
/// in-process do Hiram (WireMock). Exercita a emissao ponta a ponta (evento -> outbox
/// -> relay -> POST /v1/events). Mongo single-node: o gravador cai para escrita
/// sequencial best-effort (a garantia transacional roda em producao, Atlas replica set).
/// </summary>
public sealed class OutboxRelayFixture : WebApplicationFactory<Program>, IAsyncLifetime
{
    private const string EmailAdmin = "admin@levante.dev";
    private const string SenhaAdmin = "Senha-de-teste-Forte-123";
    private const string SegredoJwtDeTeste = "chave-de-teste-jwt-nao-secreta-com-mais-de-32-caracteres";
    private const string SegredoOrigemDeTeste = "segredo-de-teste-origem-hash-com-mais-de-32-caracteres";

    public const string ApiKey = "hk_live_teste_levante";
    public const string SiteUrl = "https://levante.test";
    public const string AdminEmail = "moderacao@levante.test";

    private readonly MongoDbContainer _mongo = new MongoDbBuilder(ImagensDeTeste.Mongo).Build();
    private WireMockServer _hiram = null!;

    public string MongoConnectionString => _mongo.GetConnectionString();

    async Task IAsyncLifetime.InitializeAsync()
    {
        await _mongo.StartAsync();
        _hiram = WireMockServer.Start();
        StubAceito();
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        _hiram?.Stop();
        _hiram?.Dispose();
        await _mongo.DisposeAsync();
        await base.DisposeAsync();
    }

    /// <summary>Responde 202 (aceito) a POST /v1/events; opcionalmente marca replay (header).</summary>
    public void StubAceito(bool replay = false)
    {
        _hiram.Reset();
        var resposta = Response.Create()
            .WithStatusCode(202)
            .WithHeader("Content-Type", "application/json")
            .WithBody("{\"id\":\"00000000-0000-0000-0000-000000000000\",\"status\":\"accepted\"}");
        if (replay)
        {
            resposta = resposta.WithHeader("Idempotency-Replayed", "true");
        }

        _hiram.Given(Request.Create().WithPath("/v1/events").UsingPost()).RespondWith(resposta);
    }

    /// <summary>Responde com um status arbitrario (ex. 500, 400) a POST /v1/events, resetando o journal.</summary>
    public void StubStatus(int status)
    {
        _hiram.Reset();
        _hiram.Given(Request.Create().WithPath("/v1/events").UsingPost())
            .RespondWith(Response.Create().WithStatusCode(status));
    }

    /// <summary>Requisicoes POST /v1/events recebidas ate agora.</summary>
    public IReadOnlyList<ILogEntry> RequisicoesEmissao() =>
        _hiram.FindLogEntries(Request.Create().WithPath("/v1/events").UsingPost());

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment(Environments.Development);
        builder.ConfigureAppConfiguration((_, config) =>
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Mongo:ConnectionString"] = _mongo.GetConnectionString(),
                ["Mongo:DatabaseName"] = "levante",
                ["Jwt:SecretKey"] = SegredoJwtDeTeste,
                ["Admin:Email"] = EmailAdmin,
                ["Admin:SenhaInicial"] = SenhaAdmin,
                ["Engajamento:OrigemHashSecret"] = SegredoOrigemDeTeste,
                ["Outbox:RelayHabilitado"] = "true",
                ["Outbox:IntervaloSegundos"] = "1",
                ["Hiram:BaseUrl"] = _hiram.Url,
                ["Hiram:ApiKey"] = ApiKey,
                ["Levante:Notificacoes:AdminEmail"] = AdminEmail,
                ["Levante:Notificacoes:SiteUrl"] = SiteUrl,
            }));
    }
}
