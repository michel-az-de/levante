namespace Levante.Api.Seguranca;

/// <summary>
/// Extrai origem do cliente numa escrita publica. Como o browser passa pelo BFF
/// do Next (nunca fala com a API direto, desde a Fatia 2a/A5), o IP real chega via
/// <c>X-Forwarded-For</c> e e resolvido pelo framework (<c>UseForwardedHeaders</c>);
/// o id de visitante vem em <c>X-Visitante</c>.
/// </summary>
internal static class OrigemDoCliente
{
    public const string HeaderVisitante = "X-Visitante";

    /// <summary>
    /// Header que o <c>ForwardedHeadersMiddleware</c> grava com o valor original ao consumir um hop
    /// de <c>X-Forwarded-For</c>. Presenca dele = a request atravessou o proxy da borda.
    /// </summary>
    private const string HeaderOriginalFor = "X-Original-For";

    /// <summary>
    /// True quando a request veio de um cliente atraves do proxy da borda (browser -> Caddy -> BFF
    /// do Next -> API), caso em que <see cref="Ip"/> e o IP do visitante. False quando e chamada
    /// interna container-a-container: o SSR do Next renderiza pagina publica chamando a API direto,
    /// sem IP de cliente para repassar (propagar exigiria <c>headers()</c> no front, o que forcaria
    /// renderizacao dinamica e mataria o ISR).
    /// <para>
    /// Nao e spoofavel pelo publico: os BFFs montam um <c>Headers</c> novo e repassam apenas o que
    /// esta na lista deles, entao um <c>X-Original-For</c> vindo do browser nao chega aqui. Vale a
    /// premissa de que a API so e alcancavel pela rede interna — se a porta for publicada no host,
    /// o balde interno vira bypass.
    /// </para>
    /// </summary>
    public static bool VeioDeCliente(HttpContext contexto)
    {
        ArgumentNullException.ThrowIfNull(contexto);

        return contexto.Request.Headers.ContainsKey(HeaderOriginalFor);
    }

    /// <summary>
    /// IP do cliente: o <c>RemoteIpAddress</c> resolvido pelo framework. Em producao,
    /// <c>UseForwardedHeaders</c> (ForwardLimit=1) ja desfaz UM hop de proxy confiavel e
    /// popula esse IP a partir do X-Forwarded-For. Ler o header cru aqui confiaria no valor
    /// mais a esquerda — controlado pelo cliente e spoofavel —, furando o rate limit publico
    /// e envenenando o hash de origem (anti-abuso).
    /// </summary>
    public static string Ip(HttpContext contexto)
        => contexto.Connection.RemoteIpAddress?.ToString() ?? "desconhecido";

    public static string UserAgent(HttpContext contexto) => contexto.Request.Headers.UserAgent.ToString();

    /// <summary>Id opaco do visitante (cookie httpOnly first-party, repassado pelo BFF).</summary>
    public static string Visitante(HttpContext contexto) => contexto.Request.Headers[HeaderVisitante].ToString();
}
