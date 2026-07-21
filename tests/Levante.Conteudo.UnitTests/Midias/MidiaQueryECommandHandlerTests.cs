using Levante.Conteudo.Application.Midias.EnviarMidia;
using Levante.Conteudo.Application.Midias.ObterMidia;
using Levante.Conteudo.Application.Midias.RemoverMidia;
using Levante.SharedKernel;
using Shouldly;
using Xunit;

namespace Levante.Conteudo.UnitTests.Midias;

/// <summary>Cobre ObterMidiaQueryHandler e RemoverMidiaCommandHandler (leitura e exclusao).</summary>
[Trait("Category", "Unit")]
public sealed class MidiaQueryECommandHandlerTests
{
    private static readonly byte[] BytesPng = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0, 0, 0, 0];

    [Fact]
    public async Task ObterMidia_existente_devolveConteudoEContentType()
    {
        var storage = new MidiaStorageEmMemoria();
        var id = await SalvarPngAsync(storage);

        var resultado = await new ObterMidiaQueryHandler(storage)
            .Handle(new ObterMidiaQuery(id), CancellationToken.None);

        resultado.Sucesso.ShouldBeTrue();
        resultado.Valor!.ContentType.ShouldBe("image/png");
        resultado.Valor.Tamanho.ShouldBe(BytesPng.Length);

        using var memoria = new MemoryStream();
        await resultado.Valor.Conteudo.CopyToAsync(memoria, CancellationToken.None);
        memoria.ToArray().ShouldBe(BytesPng);
    }

    [Fact]
    public async Task ObterMidia_inexistente_falhaComNaoEncontrado()
    {
        var resultado = await new ObterMidiaQueryHandler(new MidiaStorageEmMemoria())
            .Handle(new ObterMidiaQuery(Guid.NewGuid()), CancellationToken.None);

        resultado.Falhou.ShouldBeTrue();
        resultado.Erro.Codigo.ShouldBe("midia_nao_encontrada");
        resultado.Erro.Tipo.ShouldBe(TipoErro.NaoEncontrado);
    }

    [Fact]
    public async Task RemoverMidia_existente_removeEDepoisNaoEncontra()
    {
        var storage = new MidiaStorageEmMemoria();
        var id = await SalvarPngAsync(storage);

        var remocao = await new RemoverMidiaCommandHandler(storage)
            .Handle(new RemoverMidiaCommand(id), CancellationToken.None);
        remocao.Sucesso.ShouldBeTrue();

        var leitura = await new ObterMidiaQueryHandler(storage)
            .Handle(new ObterMidiaQuery(id), CancellationToken.None);
        leitura.Falhou.ShouldBeTrue();
    }

    [Fact]
    public async Task RemoverMidia_inexistente_falhaComNaoEncontrado()
    {
        var resultado = await new RemoverMidiaCommandHandler(new MidiaStorageEmMemoria())
            .Handle(new RemoverMidiaCommand(Guid.NewGuid()), CancellationToken.None);

        resultado.Falhou.ShouldBeTrue();
        resultado.Erro.Tipo.ShouldBe(TipoErro.NaoEncontrado);
    }

    private static async Task<Guid> SalvarPngAsync(MidiaStorageEmMemoria storage)
    {
        using var stream = new MemoryStream(BytesPng);
        var envio = await new EnviarMidiaCommandHandler(storage, new EnviarMidiaCommandValidator())
            .Handle(new EnviarMidiaCommand(stream, "image/png", "foto.png", BytesPng.Length), CancellationToken.None);

        return envio.Valor!.Id;
    }
}
