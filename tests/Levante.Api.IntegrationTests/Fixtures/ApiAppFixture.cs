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
    // Credenciais de teste (NAO sao segredos): o seeder cria este admin em Development.
    public const string EmailAdmin = "admin@levante.dev";
    public const string SenhaAdmin = "Senha-de-teste-Forte-123";

    // Chave JWT apenas de teste (baixa entropia, descritiva): nao e segredo real.
    private const string SegredoJwtDeTeste = "chave-de-teste-jwt-nao-secreta-com-mais-de-32-caracteres";

    // Segredo do HMAC de origem (dedup de reacao). Apenas de teste, nao e segredo real.
    private const string SegredoOrigemDeTeste = "segredo-de-teste-origem-hash-com-mais-de-32-caracteres";

    private readonly MongoDbContainer _mongo = new MongoDbBuilder(ImagensDeTeste.Mongo)
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
                ["Jwt:SecretKey"] = SegredoJwtDeTeste,
                ["Admin:Email"] = EmailAdmin,
                ["Admin:SenhaInicial"] = SenhaAdmin,
                ["Engajamento:OrigemHashSecret"] = SegredoOrigemDeTeste,
            }));
    }
}
