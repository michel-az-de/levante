using System.Net.Http.Json;
using Levante.Api.IntegrationTests.Fixtures;
using Levante.Conteudo.Application.Artigos;
using Shouldly;
using Xunit;

namespace Levante.Api.IntegrationTests;

[Trait("Category", "Integration")]
public sealed class ArtigosEndpointTests(ApiAppFixture fixture) : IClassFixture<ApiAppFixture>
{
    [Fact]
    public async Task Listar_devolveOSeedLidoDoMongo()
    {
        var client = fixture.CreateClient();

        var artigos = await client.GetFromJsonAsync<List<ArtigoResponse>>("/artigos", CancellationToken.None);

        artigos.ShouldNotBeNull();
        artigos.Count.ShouldBeGreaterThanOrEqualTo(2);
        artigos.ShouldContain(a => a.Slug == "clean-architecture-na-pratica");
    }
}
