using Levante.Api.IntegrationTests.Fixtures;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace Levante.Api.IntegrationTests;

/// <summary>
/// Guardrail de seguranca: todo endpoint nasce com autorizacao explicita
/// (AllowAnonymous OU RequireAuthorization). Nada implicito (CLAUDE.md).
/// </summary>
[Trait("Category", "Integration")]
public sealed class AutorizacaoExplicitaTests(ApiAppFixture fixture) : IClassFixture<ApiAppFixture>
{
    [Fact]
    public void TodoEndpointDeclaraAutorizacaoExplicita()
    {
        _ = fixture.CreateClient(); // inicia o host e materializa os endpoints

        var fonte = fixture.Services.GetRequiredService<EndpointDataSource>();
        var rotas = fonte.Endpoints.OfType<RouteEndpoint>().ToList();

        rotas.ShouldNotBeEmpty();

        var semAutorizacao = rotas
            .Where(e =>
                e.Metadata.GetMetadata<IAllowAnonymous>() is null &&
                e.Metadata.GetMetadata<IAuthorizeData>() is null)
            .Select(e => e.RoutePattern.RawText ?? e.DisplayName ?? "(desconhecido)")
            .ToList();

        semAutorizacao.ShouldBeEmpty(
            $"Endpoints sem autorizacao explicita: {string.Join(", ", semAutorizacao)}");
    }
}
