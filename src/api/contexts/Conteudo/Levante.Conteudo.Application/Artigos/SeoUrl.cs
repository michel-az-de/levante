namespace Levante.Conteudo.Application.Artigos;

/// <summary>Validacao da URL da imagem Open Graph (compartilhada pelos validators de artigo).</summary>
internal static class SeoUrl
{
    /// <summary>
    /// Aceita vazio (campo opcional), URL absoluta http(s) ou caminho relativo
    /// comecando com '/'. Rejeita esquemas perigosos (javascript:, data:, etc.).
    /// </summary>
    public static bool EhImagemOgValida(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return true;
        }

        if (url.StartsWith('/'))
        {
            return true;
        }

        return Uri.TryCreate(url, UriKind.Absolute, out var uri)
            && (string.Equals(uri.Scheme, Uri.UriSchemeHttp, StringComparison.Ordinal)
                || string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.Ordinal));
    }
}
