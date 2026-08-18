using System.Net;
using Levante.Api.Seguranca;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Shouldly;
using Xunit;

namespace Levante.Api.IntegrationTests;

/// <summary>
/// O rate limit por cliente depende de duas coisas do ForwardedHeaders: desfazer o hop do proxy da
/// borda E gravar o X-Original-For que <c>OrigemDoCliente.VeioDeCliente</c> le para distinguir
/// visitante de trafego interno. Estes testes rodam o middleware real com a configuracao de
/// producao (<see cref="ForwardedHeadersConfig.Configurar"/>) porque a ApiAppFixture sobe em
/// Development, ambiente em que o <c>UseForwardedHeaders</c> e justamente pulado — um teste
/// ponta-a-ponta pela fixture nao exercitaria nada disso.
/// <para>Sem Docker: nao leva Trait de Integration.</para>
/// </summary>
public sealed class OrigemDoClienteTests
{
    /// <summary>IP do container do Next na rede do Docker (quem realmente abre a conexao).</summary>
    private const string IpDoContainer = "10.0.0.5";

    private static async Task<HttpContext> AtravessarForwardedHeaders(string? xForwardedFor)
    {
        var options = new ForwardedHeadersOptions();
        ForwardedHeadersConfig.Configurar(options);

        var contexto = new DefaultHttpContext();
        contexto.Connection.RemoteIpAddress = IPAddress.Parse(IpDoContainer);
        if (xForwardedFor is not null)
        {
            contexto.Request.Headers["X-Forwarded-For"] = xForwardedFor;
        }

        var middleware = new ForwardedHeadersMiddleware(
            _ => Task.CompletedTask,
            NullLoggerFactory.Instance,
            Options.Create(options));
        await middleware.Invoke(contexto);

        return contexto;
    }

    [Fact]
    public async Task VeioDeCliente_verdadeiroQuandoOProxyEncaminhouOIp()
    {
        var contexto = await AtravessarForwardedHeaders("203.0.113.7");

        OrigemDoCliente.VeioDeCliente(contexto).ShouldBeTrue();
        OrigemDoCliente.Ip(contexto).ShouldBe("203.0.113.7");
    }

    [Fact]
    public async Task VeioDeCliente_falsoNaChamadaInternaSemForwardedFor()
    {
        // SSR do Next: renderiza pagina publica chamando a API direto, sem IP de cliente.
        var contexto = await AtravessarForwardedHeaders(null);

        OrigemDoCliente.VeioDeCliente(contexto).ShouldBeFalse();
        OrigemDoCliente.Ip(contexto).ShouldBe(IpDoContainer);
    }

    [Fact]
    public async Task Ip_usaOHopDaBordaEIgnoraOValorSpoofadoPeloCliente()
    {
        // O proxy da borda anexa o IP real a direita. ForwardLimit=1 desfaz so esse hop, entao o
        // valor que o cliente inventou (a esquerda) nao vira a identidade do balde.
        var contexto = await AtravessarForwardedHeaders("198.51.100.1, 203.0.113.7");

        OrigemDoCliente.VeioDeCliente(contexto).ShouldBeTrue();
        OrigemDoCliente.Ip(contexto).ShouldBe("203.0.113.7");
    }

    [Fact]
    public async Task ParticaoGlobal_separaVisitantesEntreSiEDoTrafegoInterno()
    {
        var primeiro = RateLimiting.ParticaoGlobal(await AtravessarForwardedHeaders("203.0.113.7"));
        var segundo = RateLimiting.ParticaoGlobal(await AtravessarForwardedHeaders("203.0.113.8"));
        var interno = RateLimiting.ParticaoGlobal(await AtravessarForwardedHeaders(null));

        // O bug do #93: sem a distincao, os tres caiam na mesma chave (o IP do container).
        primeiro.Chave.ShouldBe("203.0.113.7");
        segundo.Chave.ShouldBe("203.0.113.8");
        primeiro.Chave.ShouldNotBe(segundo.Chave);

        interno.Chave.ShouldNotBe(primeiro.Chave);
        interno.Chave.ShouldNotBe(IpDoContainer); // nao pode colidir com um visitante nesse IP
        interno.Teto.ShouldBeGreaterThan(primeiro.Teto);
    }
}
