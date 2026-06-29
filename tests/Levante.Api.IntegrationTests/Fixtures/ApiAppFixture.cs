using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Testcontainers.MongoDb;
using Xunit;

namespace Levante.Api.IntegrationTests.Fixtures;

/// <summary>
/// Sobe a API real (WebApplicationFactory) sobre um Mongo efemero autenticado
/// e exercita o boot completo (indices, seed em Development, self-check de
/// privilegio) + endpoints.
///
/// TODO (Fatia 4): habilitar replica set quando o Outbox/Change Streams entrar.
/// O combo auth + replica set + keyfile do Testcontainers e instavel e a Fatia 0
/// nao usa transacoes/Change Streams, entao roda single node por ora.
/// </summary>
public sealed class ApiAppFixture : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly MongoDbContainer _mongo = new MongoDbBuilder()
        .WithUsername("root")
        .WithPassword("root-pwd")
        .Build();

    Task IAsyncLifetime.InitializeAsync() => _mongo.StartAsync();

    async Task IAsyncLifetime.DisposeAsync()
    {
        await _mongo.DisposeAsync();
        await base.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment(Environments.Development);
        builder.ConfigureAppConfiguration((_, config) =>
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Mongo:ConnectionString"] = _mongo.GetConnectionString(),
                ["Mongo:DatabaseName"] = "levante",
            }));
    }
}
