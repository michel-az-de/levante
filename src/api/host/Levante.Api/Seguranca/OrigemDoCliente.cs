namespace Levante.Api.Seguranca;

/// <summary>
/// Extrai origem do cliente numa escrita publica. Como o browser passa pelo BFF
/// do Next (nunca fala com a API direto, desde a Fatia 2a/A5), o IP real vem no
/// header <c>X-Forwarded-For</c> e o id de visitante em <c>X-Visitante</c>.
/// </summary>
internal static class OrigemDoCliente
{
    public const string HeaderVisitante = "X-Visitante";

    /// <summary>IP do cliente: 1o valor de X-Forwarded-For (posto pelo BFF) ou o IP da conexao.</summary>
    public static string Ip(HttpContext contexto)
    {
        var encaminhado = contexto.Request.Headers["X-Forwarded-For"].ToString();
        if (!string.IsNullOrWhiteSpace(encaminhado))
        {
            return encaminhado.Split(',')[0].Trim();
        }

        return contexto.Connection.RemoteIpAddress?.ToString() ?? "desconhecido";
    }

    public static string UserAgent(HttpContext contexto) => contexto.Request.Headers.UserAgent.ToString();

    /// <summary>Id opaco do visitante (cookie httpOnly first-party, repassado pelo BFF).</summary>
    public static string Visitante(HttpContext contexto) => contexto.Request.Headers[HeaderVisitante].ToString();
}
