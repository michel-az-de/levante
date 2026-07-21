using System.Net.Http.Headers;
using Levante.Identity.Application.Ports;
using Levante.Identity.Domain.Administradores;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace Levante.Api.IntegrationTests.Fixtures;

/// <summary>
/// Bearer de admin para os testes que so precisam estar autenticados.
///
/// O token e assinado direto pelo IGeradorDeToken do container, sem passar por
/// POST /auth/login: aquele endpoint tem PolicyAuth (5 req/min por IP) e gastar
/// esse balde em teste que nao testa login derrubou o CI de forma intermitente.
/// Quem exercita o fluxo de login de verdade e o AuthEndpointTests, batendo no
/// endpoint — como deve ser.
/// </summary>
internal static class AutenticacaoDeTeste
{
    /// <summary>E-mail do admin semeado em Development pelas fixtures de teste.</summary>
    public const string EmailAdminSemeado = "admin@levante.dev";

    public static async Task<HttpClient> CriarClienteAutenticadoAsync(this WebApplicationFactory<Program> fabrica)
    {
        ArgumentNullException.ThrowIfNull(fabrica);

        var client = fabrica.CreateClient(); // tambem materializa o host antes de resolver servicos
        using var escopo = fabrica.Services.CreateScope();

        var administradores = escopo.ServiceProvider.GetRequiredService<IAdministradorRepository>();
        var admin = await administradores.GetByEmailAsync(EmailAdminSemeado, CancellationToken.None)
            ?? throw new InvalidOperationException(
                $"Admin '{EmailAdminSemeado}' nao foi semeado; o seed so roda em Development.");

        var token = escopo.ServiceProvider.GetRequiredService<IGeradorDeToken>().Gerar(admin).AccessToken;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return client;
    }
}
