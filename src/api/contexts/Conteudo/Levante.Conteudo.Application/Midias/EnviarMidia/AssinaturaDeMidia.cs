namespace Levante.Conteudo.Application.Midias.EnviarMidia;

/// <summary>
/// Confere a assinatura binaria (magic bytes) do arquivo contra o content-type
/// declarado. Nao confia so no header HTTP: um client mal-intencionado pode
/// declarar "image/png" e enviar outra coisa.
/// </summary>
internal static class AssinaturaDeMidia
{
    private const int TamanhoCabecalho = 12;

    public static async Task<bool> ConfereAsync(Stream conteudo, string contentType, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(conteudo);

        if (!conteudo.CanSeek)
        {
            return false;
        }

        var posicaoOriginal = conteudo.Position;
        conteudo.Position = 0;
        try
        {
            var cabecalho = new byte[TamanhoCabecalho];
            var lidos = await conteudo.ReadAsync(cabecalho.AsMemory(0, TamanhoCabecalho), ct);
            return Bate(contentType, cabecalho, lidos);
        }
        finally
        {
            conteudo.Position = posicaoOriginal;
        }
    }

    private static bool Bate(string contentType, byte[] cabecalho, int lidos) => contentType switch
    {
        "image/png" => lidos >= 8
            && cabecalho[0] == 0x89 && cabecalho[1] == 0x50 && cabecalho[2] == 0x4E && cabecalho[3] == 0x47
            && cabecalho[4] == 0x0D && cabecalho[5] == 0x0A && cabecalho[6] == 0x1A && cabecalho[7] == 0x0A,
        "image/jpeg" => lidos >= 3 && cabecalho[0] == 0xFF && cabecalho[1] == 0xD8 && cabecalho[2] == 0xFF,
        "image/gif" => lidos >= 6
            && cabecalho[0] == (byte)'G' && cabecalho[1] == (byte)'I' && cabecalho[2] == (byte)'F',
        "image/webp" => lidos >= TamanhoCabecalho
            && cabecalho[0] == (byte)'R' && cabecalho[1] == (byte)'I' && cabecalho[2] == (byte)'F' && cabecalho[3] == (byte)'F'
            && cabecalho[8] == (byte)'W' && cabecalho[9] == (byte)'E' && cabecalho[10] == (byte)'B' && cabecalho[11] == (byte)'P',
        _ => false,
    };
}
