using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Testcontainers.MongoDb;
using Testcontainers.RabbitMq;
using Xunit;

namespace Levante.Api.IntegrationTests.Fixtures;

/// <summary>
/// Sobe a API com o relay do Outbox LIGADO, sobre um Mongo em REPLICA SET (exigido
/// por transacoes + Change Streams) e um RabbitMQ efemero. Exercita o caminho
/// transacional + relay ponta a ponta. Isolada das demais fixtures (single-node)
/// para conter a instabilidade do combo replica set + Testcontainers.
/// </summary>
public sealed class OutboxRelayFixture : WebApplicationFactory<Program>, IAsyncLifetime
{
    private const string EmailAdmin = "admin@levante.dev";
    private const string SenhaAdmin = "Senha-de-teste-Forte-123";
    private const string SegredoJwtDeTeste = "chave-de-teste-jwt-nao-secreta-com-mais-de-32-caracteres";
    private const string SegredoOrigemDeTeste = "segredo-de-teste-origem-hash-com-mais-de-32-caracteres";

    // Replica set single-node: habilita transacoes e Change Streams.
    private readonly MongoDbContainer _mongo = new MongoDbBuilder(ImagensDeTeste.Mongo)
        .WithReplicaSet("rs0")
        .Build();

    private readonly RabbitMqContainer _rabbit = new RabbitMqBuilder(ImagensDeTeste.RabbitMq).Build();

    public const string RabbitExchange = "levante.eventos";

    public string RabbitConnectionString => _rabbit.GetConnectionString();

    async Task IAsyncLifetime.InitializeAsync()
    {
        await _mongo.StartAsync();
        await _rabbit.StartAsync();
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await _rabbit.DisposeAsync();
        await _mongo.DisposeAsync();
        await base.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        var uri = new Uri(_rabbit.GetConnectionString());

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
                ["RabbitMq:Hostname"] = uri.Host,
                ["RabbitMq:Port"] = uri.Port.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["RabbitMq:Username"] = uri.UserInfo.Split(':')[0],
                ["RabbitMq:Password"] = uri.UserInfo.Split(':')[1],
                ["RabbitMq:VirtualHost"] = "/",
                ["RabbitMq:Exchange"] = RabbitExchange,
            }));
    }
}
