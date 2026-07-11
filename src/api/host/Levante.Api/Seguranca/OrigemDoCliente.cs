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
