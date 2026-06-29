using System.Net;
using Levante.Api.IntegrationTests.Fixtures;
using Shouldly;
using Xunit;

namespace Levante.Api.IntegrationTests;

[Trait("Category", "Integration")]
public sealed class HealthEndpointTests(ApiAppFixture fixture) : IClassFixture<ApiAppFixture>
{
    [Fact]
    public async Task Live_retornaSaudavel()
    {
        var client = fixture.CreateClient();

        var resposta = await client.GetAsync("/health/live", CancellationToken.None);

        resposta.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Ready_retornaSaudavelComMongoNoAr()
    {
        var client = fixture.CreateClient();

        var resposta = await client.GetAsync("/health/ready", CancellationToken.None);

        resposta.StatusCode.ShouldBe(HttpStatusCode.OK);
    }
}
