using Levante.Conteudo.Application.Midias.EnviarMidia;
using Levante.SharedKernel;
using Shouldly;
using Xunit;

namespace Levante.Conteudo.UnitTests.Midias;

[Trait("Category", "Unit")]
public sealed class EnviarMidiaCommandHandlerTests
{
    private static readonly byte[] BytesPng = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0, 0, 0, 0];

    private static EnviarMidiaCommandHandler Criar(MidiaStorageEmMemoria storage) =>
        new(storage, new EnviarMidiaCommandValidator());

    [Fact]
    public async Task Handle_pngValido_salvaERetornaUrlRelativa()
    {
        var storage = new MidiaStorageEmMemoria();
        var handler = Criar(storage);
        using var stream = new MemoryStream(BytesPng);

        var resultado = await handler.Handle(
            new EnviarMidiaCommand(stream, "image/png", "foto.png", BytesPng.Length), CancellationToken.None);

        resultado.Sucesso.ShouldBeTrue();
        resultado.Valor!.Url.ShouldBe($"/midias/{resultado.Valor.Id}");
        resultado.Valor.ContentType.ShouldBe("image/png");
        storage.Salvas.ShouldBe(1);
    }

    /// <summary>
    /// Regressao do cenario que a fatia 3 (importacao de HTML/URL) ia disparar: baixar
    /// a imagem para um MemoryStream via CopyToAsync deixa o cursor no fim. Sem o
    /// rebobinar do handler, o GridFS gravava 0 byte e ainda assim respondia 201 com o
    /// tamanho cheio — imagem quebrada em producao, sem erro em log nenhum.
    /// </summary>
    [Fact]
    public async Task Handle_streamPosicionadoNoFim_gravaOArquivoInteiro()
    {
        var storage = new MidiaStorageEmMemoria();
        var handler = Criar(storage);
        using var stream = new MemoryStream(BytesPng);
        stream.Position = stream.Length; // como ficaria apos um CopyToAsync

        var resultado = await handler.Handle(
            new EnviarMidiaCommand(stream, "image/png", "foto.png", BytesPng.Length), CancellationToken.None);

        resultado.Sucesso.ShouldBeTrue();
        storage.UltimoConteudo.ShouldBe(BytesPng);
        resultado.Valor!.Tamanho.ShouldBe(BytesPng.Length);
    }

    /// <summary>O que e servido depois precisa ser o content-type canonico, nao o header cru.</summary>
    [Fact]
    public async Task Handle_contentTypeComCaixaEParametros_normalizaAoSalvar()
    {
        var storage = new MidiaStorageEmMemoria();
        var handler = Criar(storage);
        using var stream = new MemoryStream(BytesPng);

        var resultado = await handler.Handle(
            new EnviarMidiaCommand(stream, "IMAGE/PNG; charset=binary", "foto.png", BytesPng.Length),
            CancellationToken.None);

        resultado.Sucesso.ShouldBeTrue();
        resultado.Valor!.ContentType.ShouldBe("image/png");
        storage.UltimoContentType.ShouldBe("image/png");
    }

    [Fact]
    public async Task Handle_tipoNaoPermitido_falhaSemSalvar()
    {
        var storage = new MidiaStorageEmMemoria();
        var handler = Criar(storage);
        using var stream = new MemoryStream(BytesPng);

        var resultado = await handler.Handle(
            new EnviarMidiaCommand(stream, "application/pdf", "arquivo.pdf", BytesPng.Length), CancellationToken.None);

        resultado.Falhou.ShouldBeTrue();
        resultado.Erro.Codigo.ShouldBe("validacao");
        resultado.Erro.Tipo.ShouldBe(TipoErro.Validacao);
        storage.Salvas.ShouldBe(0);
    }

    [Fact]
    public async Task Handle_cadaEnvioGeraIdNovo()
    {
        var storage = new MidiaStorageEmMemoria();
        var handler = Criar(storage);

        using var stream1 = new MemoryStream(BytesPng);
        var r1 = await handler.Handle(new EnviarMidiaCommand(stream1, "image/png", "a.png", BytesPng.Length), CancellationToken.None);

        using var stream2 = new MemoryStream(BytesPng);
        var r2 = await handler.Handle(new EnviarMidiaCommand(stream2, "image/png", "b.png", BytesPng.Length), CancellationToken.None);

        r1.Valor!.Id.ShouldNotBe(r2.Valor!.Id);
    }
}
