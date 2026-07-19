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
    public async Task ConfereAsync_contentTypeComCaixaEParametros_aindaConfere()
    {
        using var stream = new MemoryStream(CabecalhoPng);

        var confere = await AssinaturaDeMidia.ConfereAsync(stream, "IMAGE/PNG; charset=binary", CancellationToken.None);

        confere.ShouldBeTrue();
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

    /// <summary>
    /// Regressao: o contrato de Stream.ReadAsync permite devolver MENOS bytes que o
    /// pedido. Com uma unica chamada de leitura, um WEBP valido (assinatura ate o
    /// offset 12) era rejeitado sempre que o buffering fragmentasse a primeira leitura.
    /// </summary>
    [Fact]
    public async Task ConfereAsync_leituraFragmentada_aindaConfereAAssinatura()
    {
        await using var fragmentado = new StreamQuePingaBytes(CabecalhoWebp, bytesPorLeitura: 3);

        var confere = await AssinaturaDeMidia.ConfereAsync(fragmentado, "image/webp", CancellationToken.None);

        confere.ShouldBeTrue();
    }

    [Fact]
    public async Task ConfereAsync_restauraAPosicaoOriginalDoStreamAposConferir()
    {
        using var stream = new MemoryStream(CabecalhoPng);
        stream.Position = 3; // simula um leitor anterior que ja avancou o cursor

        await AssinaturaDeMidia.ConfereAsync(stream, "image/png", CancellationToken.None);

        // Devolve o cursor para onde o recebeu: quem grava e responsavel por rebobinar
        // (EnviarMidiaCommandHandler faz isso), este helper nao muta estado do chamador.
        stream.Position.ShouldBe(3);
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

    /// <summary>
    /// Stream posicionavel que devolve no maximo N bytes por leitura, imitando o
    /// buffering real (FileBufferingReadStream) que o MemoryStream dos testes esconde.
    /// </summary>
    private sealed class StreamQuePingaBytes(byte[] dados, int bytesPorLeitura) : Stream
    {
        public override bool CanRead => true;

        public override bool CanSeek => true;

        public override bool CanWrite => false;

        public override long Length => dados.Length;

        public override long Position { get; set; }

        public override void Flush()
        {
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var disponivel = (int)Math.Min(dados.Length - Position, Math.Min(count, bytesPorLeitura));
            if (disponivel <= 0)
            {
                return 0;
            }

            Array.Copy(dados, Position, buffer, offset, disponivel);
            Position += disponivel;
            return disponivel;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            Position = origin switch
            {
                SeekOrigin.Begin => offset,
                SeekOrigin.Current => Position + offset,
                _ => dados.Length + offset,
            };
            return Position;
        }

        public override void SetLength(long value) => throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }
}
