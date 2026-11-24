using Microsoft.AspNetCore.HttpOverrides;

namespace Levante.Api.Seguranca;

/// <summary>
/// Configuracao do <c>UseForwardedHeaders</c>. Em producao a app fica atras de um proxy
/// (Caddy/ingress) cujo IP nao e estatico, entao confiamos so no ultimo hop.
/// <para>
/// Vive num tipo proprio (em vez de inline no <c>Program.cs</c>) porque o rate limit depende
/// deste comportamento: <c>OrigemDoCliente.VeioDeCliente</c> le o header que este middleware
/// grava. Assim o teste exercita exatamente a configuracao que roda em producao.
/// </para>
/// </summary>
public static class ForwardedHeadersConfig
{
    public static IServiceCollection AddLevanteForwardedHeaders(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        return services.Configure<ForwardedHeadersOptions>(Configurar);
    }

    /// <summary>
    /// Limpar <c>KnownIPNetworks</c>/<c>KnownProxies</c> nao desliga o processamento: com as duas
    /// listas vazias o middleware para de exigir proxy conhecido e passa a confiar no hop. E o que
    /// queremos aqui, porque o IP do proxy muda. <c>ForwardLimit = 1</c> garante que apenas UM hop
    /// e desfeito, ou seja, vale a entrada mais a direita do <c>X-Forwarded-For</c> — a que o proxy
    /// da borda escreveu, nao a que o cliente mandou. Sem isso o rate limit por IP colapsaria no IP
    /// do proxy (ou seria spoofavel).
    /// </summary>
    internal static void Configurar(ForwardedHeadersOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
        options.ForwardLimit = 1;
        options.KnownIPNetworks.Clear();
        options.KnownProxies.Clear();
    }
}
