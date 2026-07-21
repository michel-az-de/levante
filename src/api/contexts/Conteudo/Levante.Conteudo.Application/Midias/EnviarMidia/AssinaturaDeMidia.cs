namespace Levante.Conteudo.Application.Midias.EnviarMidia;

/// <summary>
/// Confere a assinatura binaria (magic bytes) do arquivo contra o content-type
/// declarado. Nao confia so no header HTTP: um client mal-intencionado pode
/// declarar "image/png" e enviar outra coisa.
/// </summary>
internal static class AssinaturaDeMidia
{
    /// <summary>
    /// Le o cabecalho e compara com a assinatura do tipo. Devolve o cursor para onde
    /// o recebeu (nao rebobina para o inicio) — quem entrega o stream ao armazenamento
    /// e responsavel por posiciona-lo, ver <see cref="EnviarMidiaCommandHandler"/>.
    /// </summary>
    public static async Task<bool> ConfereAsync(Stream conteudo, string contentType, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(conteudo);

        var tipo = TipoDeMidia.Resolver(contentType);
        if (tipo is null || !conteudo.CanSeek)
        {
            return false;
        }

        var posicaoOriginal = conteudo.Position;
        conteudo.Position = 0;
        try
        {
            var cabecalho = new byte[TipoDeMidia.TamanhoCabecalho];

            // ReadAtLeastAsync, e nao ReadAsync: o contrato de Stream permite devolver
            // MENOS bytes que o pedido, e um WEBP valido (offset 8) seria rejeitado por
            // acaso quando o buffering fragmentasse a primeira leitura.
            var lidos = await conteudo.ReadAtLeastAsync(
                cabecalho, cabecalho.Length, throwOnEndOfStream: false, ct);

            return tipo.AssinaturaConfere(cabecalho.AsSpan(0, lidos));
        }
        finally
        {
            conteudo.Position = posicaoOriginal;
        }
    }
}
