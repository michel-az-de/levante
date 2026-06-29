using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Testcontainers.MongoDb;
using Xunit;

namespace Levante.Api.IntegrationTests.Fixtures;

/// <summary>
/// Sobe a API real (WebApplicationFactory) sobre um Mongo efemero em replica
/// set (sem auth) — exercita transacoes/Change Streams (Fatia 0 lock #3) e o
/// boot completo (indices, seed em Development, self-check de privilegio).
/// </summary>
public sealed class ApiAppFixture : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly MongoDbContainer _mongo = new MongoDbBuilder()
        .WithUsername(string.Empty)
        .WithPassword(string.Empty)
        .WithReplicaSet("rs0")
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
