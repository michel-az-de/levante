using MongoDB.Bson;
using MongoDB.Driver;
using Testcontainers.MongoDb;
using Xunit;

namespace Levante.Api.IntegrationTests.Fixtures;

/// <summary>
/// Mongo efemero COM autenticacao (single node) para validar o detector de
/// privilegio minimo contra usuarios reais: um root (admin) e um app
/// (readWrite na database levante).
/// </summary>
public sealed class MongoAuthFixture : IAsyncLifetime
{
    private const string Banco = "levante";

    private readonly MongoDbContainer _mongo = new MongoDbBuilder(ImagensDeTeste.Mongo)
        .WithUsername("root")
        .WithPassword("root-pwd")
        .Build();

    public IMongoClient RootClient { get; private set; } = null!;

    public IMongoClient AppClient { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await _mongo.StartAsync();

        RootClient = new MongoClient(_mongo.GetConnectionString());

        var criarUsuarioApp = new BsonDocument
        {
            { "createUser", "appuser" },
            { "pwd", "app-pwd" },
            { "roles", new BsonArray { new BsonDocument { { "role", "readWrite" }, { "db", Banco } } } },
        };
        await RootClient.GetDatabase(Banco).RunCommandAsync<BsonDocument>(criarUsuarioApp);

        var settings = MongoClientSettings.FromConnectionString(_mongo.GetConnectionString());
        settings.Credential = MongoCredential.CreateCredential(Banco, "appuser", "app-pwd");
        AppClient = new MongoClient(settings);
    }

    public async Task DisposeAsync() => await _mongo.DisposeAsync();
}
