using Levante.Conteudo.Application.Midias;
using Shouldly;
using Xunit;

namespace Levante.Conteudo.UnitTests.Midias;

[Trait("Category", "Unit")]
public sealed class TipoDeMidiaTests
{
    [Theory]
    [InlineData("image/png")]
    [InlineData("image/jpeg")]
    [InlineData("image/webp")]
    [InlineData("image/gif")]
    public void Resolver_tipoSuportado_devolveOTipo(string contentType)
    {
        TipoDeMidia.Resolver(contentType)!.ContentType.ShouldBe(contentType);
    }

    // Regressao: media type e case-insensitive por RFC; comparar com == rejeitava
    // um upload legitimo de qualquer client que nao normalizasse a caixa.
    [Theory]
    [InlineData("IMAGE/PNG")]
    [InlineData("Image/Png")]
    public void Resolver_ignoraCaixaDoContentType(string contentType)
    {
        TipoDeMidia.Resolver(contentType)!.ContentType.ShouldBe("image/png");
    }

    // Regressao: parametros sao legais no header e um servidor remoto (import da
    // fatia 3) manda "image/png; charset=binary" sem cerimonia.
    [Theory]
    [InlineData("image/png; charset=binary")]
    [InlineData("image/png;charset=utf-8")]
    [InlineData("image/jpeg; q=0.9")]
    public void Resolver_ignoraParametrosDoContentType(string contentType)
    {
        TipoDeMidia.Resolver(contentType).ShouldNotBeNull();
    }

    [Theory]
    [InlineData("image/svg+xml")]
    [InlineData("application/pdf")]
    [InlineData("text/html")]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("nao-e-um-media-type")]
    public void Resolver_tipoNaoSuportadoOuInvalido_devolveNulo(string? contentType)
    {
        TipoDeMidia.Resolver(contentType).ShouldBeNull();
    }

    [Fact]
    public void AssinaturaConfere_aceitaGif87aEGif89a()
    {
        var gif = TipoDeMidia.Resolver("image/gif")!;

        gif.AssinaturaConfere("GIF87a"u8).ShouldBeTrue();
        gif.AssinaturaConfere("GIF89a"u8).ShouldBeTrue();
    }

    [Fact]
    public void AssinaturaConfere_recusaGifComVersaoInvalida()
    {
        TipoDeMidia.Resolver("image/gif")!.AssinaturaConfere("GIF77a"u8).ShouldBeFalse();
    }

    // Regressao: WEBP e "RIFF" + tamanho + "WEBP"; conferir so o RIFF aceitaria
    // qualquer container RIFF (ex.: WAV) declarado como imagem.
    [Fact]
    public void AssinaturaConfere_recusaRiffQueNaoEWebp()
    {
        var webp = TipoDeMidia.Resolver("image/webp")!;
        byte[] wav = [.. "RIFF"u8.ToArray(), 0, 0, 0, 0, .. "WAVE"u8.ToArray()];

        webp.AssinaturaConfere(wav).ShouldBeFalse();
    }

    [Fact]
    public void AssinaturaConfere_aceitaWebpValido()
    {
        var webp = TipoDeMidia.Resolver("image/webp")!;
        byte[] valido = [.. "RIFF"u8.ToArray(), 0, 0, 0, 0, .. "WEBP"u8.ToArray()];

        webp.AssinaturaConfere(valido).ShouldBeTrue();
    }

    // Regressao: cabecalho truncado nao pode estourar IndexOutOfRange nem passar.
    [Fact]
    public void AssinaturaConfere_cabecalhoCurtoDemais_recusaSemEstourar()
    {
        TipoDeMidia.Resolver("image/webp")!.AssinaturaConfere("RIFF"u8).ShouldBeFalse();
        TipoDeMidia.Resolver("image/png")!.AssinaturaConfere([0x89, 0x50]).ShouldBeFalse();
        TipoDeMidia.Resolver("image/png")!.AssinaturaConfere([]).ShouldBeFalse();
    }

    [Fact]
    public void ContentTypesSuportados_listaTodosOsTiposSemDuplicar()
    {
        TipoDeMidia.ContentTypesSuportados.ShouldBe("image/png, image/jpeg, image/gif, image/webp");
    }
}
