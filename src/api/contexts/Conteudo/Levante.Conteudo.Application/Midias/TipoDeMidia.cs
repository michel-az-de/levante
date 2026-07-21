using System.Net.Http.Headers;

namespace Levante.Conteudo.Application.Midias;

/// <summary>
/// Tipo de imagem aceito no upload: content-type canonico + a assinatura binaria
/// (magic bytes) que o arquivo precisa ter. Fonte UNICA de verdade — antes a lista
/// de tipos vivia no validador e as assinaturas num switch separado, entao aceitar
/// um formato novo exigia lembrar de mexer nos dois lugares.
/// </summary>
internal sealed record TipoDeMidia(string ContentType, IReadOnlyList<SegmentoDeAssinatura> Assinatura)
{
    /// <summary>Bytes lidos do inicio do arquivo; cobre a maior assinatura (WEBP, ate o offset 12).</summary>
    public const int TamanhoCabecalho = 12;

    private static readonly TipoDeMidia[] Suportados =
    [
        new("image/png", [new(0, [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A])]),
        new("image/jpeg", [new(0, [0xFF, 0xD8, 0xFF])]),
        // "GIF8" cobre 87a e 89a (as duas unicas versoes) sem precisar de alternativas.
        new("image/gif", [new(0, "GIF8"u8.ToArray())]),
        // WEBP e RIFF com o marcador "WEBP" depois do tamanho: dois segmentos separados.
        new("image/webp", [new(0, "RIFF"u8.ToArray()), new(8, "WEBP"u8.ToArray())]),
    ];

    /// <summary>Content-types aceitos, para compor mensagem de erro sem duplicar a lista.</summary>
    public static string ContentTypesSuportados { get; } = string.Join(", ", Suportados.Select(t => t.ContentType));

    /// <summary>
    /// Resolve o tipo a partir do header, ignorando caixa e parametros: media type e
    /// case-insensitive por RFC e "image/png; charset=binary" e um header legal.
    /// Devolve <c>null</c> quando o tipo nao e suportado ou o header e invalido.
    /// </summary>
    public static TipoDeMidia? Resolver(string? contentType)
    {
        if (!MediaTypeHeaderValue.TryParse(contentType, out var cabecalho) || cabecalho.MediaType is null)
        {
            return null;
        }

        return Array.Find(
            Suportados,
            tipo => string.Equals(tipo.ContentType, cabecalho.MediaType, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>Confere se o cabecalho lido do arquivo bate com a assinatura deste tipo.</summary>
    public bool AssinaturaConfere(ReadOnlySpan<byte> cabecalho)
    {
        foreach (var segmento in Assinatura)
        {
            if (cabecalho.Length < segmento.Deslocamento + segmento.Bytes.Length)
            {
                return false;
            }

            if (!cabecalho.Slice(segmento.Deslocamento, segmento.Bytes.Length).SequenceEqual(segmento.Bytes))
            {
                return false;
            }
        }

        return true;
    }
}

/// <summary>Trecho de bytes que deve aparecer numa posicao especifica do inicio do arquivo.</summary>
internal sealed record SegmentoDeAssinatura(int Deslocamento, byte[] Bytes);
