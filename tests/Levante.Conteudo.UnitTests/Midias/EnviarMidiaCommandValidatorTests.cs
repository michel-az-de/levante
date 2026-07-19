using Levante.Conteudo.Application.Midias.EnviarMidia;
using Shouldly;
using Xunit;

namespace Levante.Conteudo.UnitTests.Midias;

[Trait("Category", "Unit")]
public sealed class EnviarMidiaCommandValidatorTests
{
    private static readonly byte[] BytesPng = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0, 0, 0, 0];

    private readonly EnviarMidiaCommandValidator _validador = new();

    [Fact]
    public async Task Validate_pngValido_passa()
    {
        using var stream = new MemoryStream(BytesPng);
        var comando = new EnviarMidiaCommand(stream, "image/png", "foto.png", BytesPng.Length);

        var resultado = await _validador.ValidateAsync(comando);

        resultado.IsValid.ShouldBeTrue();
    }

    [Fact]
    public async Task Validate_contentTypeNaoPermitido_falha()
    {
        using var stream = new MemoryStream(BytesPng);
        var comando = new EnviarMidiaCommand(stream, "image/svg+xml", "foto.svg", BytesPng.Length);

        var resultado = await _validador.ValidateAsync(comando);

        resultado.IsValid.ShouldBeFalse();
    }

    [Fact]
    public async Task Validate_tamanhoAcimaDoLimite_falha()
    {
        using var stream = new MemoryStream(BytesPng);
        var comando = new EnviarMidiaCommand(
            stream, "image/png", "foto.png", EnviarMidiaCommandValidator.TamanhoMaximoBytes + 1);

        var resultado = await _validador.ValidateAsync(comando);

        resultado.IsValid.ShouldBeFalse();
    }

    [Fact]
    public async Task Validate_tamanhoZero_falha()
    {
        using var stream = new MemoryStream(BytesPng);
        var comando = new EnviarMidiaCommand(stream, "image/png", "foto.png", 0);

        var resultado = await _validador.ValidateAsync(comando);

        resultado.IsValid.ShouldBeFalse();
    }

    [Fact]
    public async Task Validate_bytesNaoBatemComContentTypeDeclarado_falha()
    {
        var bytesJpeg = new byte[] { 0xFF, 0xD8, 0xFF, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        using var stream = new MemoryStream(bytesJpeg);
        var comando = new EnviarMidiaCommand(stream, "image/png", "foto.png", bytesJpeg.Length); // declara PNG, envia JPEG

        var resultado = await _validador.ValidateAsync(comando);

        resultado.IsValid.ShouldBeFalse();
    }
}
