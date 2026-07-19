using Levante.Conteudo.Application.Midias.EnviarMidia;
using Shouldly;
using Xunit;

namespace Levante.Conteudo.UnitTests.Midias;

[Trait("Category", "Unit")]
public sealed class AssinaturaDeMidiaTests
{
    private static readonly byte[] CabecalhoPng = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0, 0, 0, 0];
    private static readonly byte[] CabecalhoJpeg = [0xFF, 0xD8, 0xFF, 0, 0, 0, 0, 0, 0, 0, 0, 0];
    private static readonly byte[] CabecalhoGif = "GIF89a"u8.ToArray();
    private static readonly byte[] CabecalhoWebp = [.. "RIFF"u8.ToArray(), 0, 0, 0, 0, .. "WEBP"u8.ToArray()];

    [Theory]
    [MemberData(nameof(AssinaturasValidas))]
    public async Task ConfereAsync_assinaturaBateComTipo_retornaTrue(byte[] cabecalho, string contentType)
    {
        using var stream = new MemoryStream(cabecalho);

        var confere = await AssinaturaDeMidia.ConfereAsync(stream, contentType, CancellationToken.None);

        confere.ShouldBeTrue();
    }

    [Fact]
    public async Task ConfereAsync_conteudoNaoBateComTipoDeclarado_retornaFalse()
    {
        using var stream = new MemoryStream(CabecalhoJpeg); // bytes de JPEG...

        var confere = await AssinaturaDeMidia.ConfereAsync(stream, "image/png", CancellationToken.None); // ...declarado como PNG

        confere.ShouldBeFalse();
    }

    [Fact]
    public async Task ConfereAsync_tipoDesconhecido_retornaFalse()
    {
        using var stream = new MemoryStream(CabecalhoPng);

        var confere = await AssinaturaDeMidia.ConfereAsync(stream, "image/svg+xml", CancellationToken.None);

        confere.ShouldBeFalse();
    }

    [Fact]
    public async Task ConfereAsync_streamMuitoCurta_retornaFalse()
    {
        using var stream = new MemoryStream([0x89, 0x50]);

        var confere = await AssinaturaDeMidia.ConfereAsync(stream, "image/png", CancellationToken.None);

        confere.ShouldBeFalse();
    }

    [Fact]
    public async Task ConfereAsync_streamNaoPosicionavel_retornaFalse()
    {
        await using var naoPosicionavel = new StreamSemSeek(new MemoryStream(CabecalhoPng));

        var confere = await AssinaturaDeMidia.ConfereAsync(naoPosicionavel, "image/png", CancellationToken.None);

        confere.ShouldBeFalse();
    }

    [Fact]
    public async Task ConfereAsync_reposicionaOStreamNoInicioAposConferir()
    {
        using var stream = new MemoryStream(CabecalhoPng);
        stream.Position = 3; // simula um leitor anterior que ja avancou o cursor

        await AssinaturaDeMidia.ConfereAsync(stream, "image/png", CancellationToken.None);

        stream.Position.ShouldBe(3); // reposiciona para a posicao ORIGINAL, nao para 0
    }

    public static TheoryData<byte[], string> AssinaturasValidas() => new()
    {
        { CabecalhoPng, "image/png" },
        { CabecalhoJpeg, "image/jpeg" },
        { CabecalhoGif, "image/gif" },
        { CabecalhoWebp, "image/webp" },
    };

    /// <summary>Envolve um stream posicionavel escondendo CanSeek, para testar o guard de nao-seekable.</summary>
    private sealed class StreamSemSeek(Stream interno) : Stream
    {
        public override bool CanRead => interno.CanRead;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        public override long Length => interno.Length;

        public override long Position
        {
            get => interno.Position;
            set => throw new NotSupportedException();
        }

        public override void Flush() => interno.Flush();

        public override int Read(byte[] buffer, int offset, int count) => interno.Read(buffer, offset, count);

        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

        public override void SetLength(long value) => throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }
}
