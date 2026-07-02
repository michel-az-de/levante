using Levante.Engajamento.Application.Comentarios.CriarComentario;
using Levante.Engajamento.UnitTests.Reacoes;
using Levante.SharedKernel;
using Shouldly;
using Xunit;

namespace Levante.Engajamento.UnitTests.Comentarios;

[Trait("Category", "Unit")]
public sealed class CriarComentarioCommandHandlerTests
{
    private static CriarComentarioCommandHandler Criar(ComentarioRepositorioEmMemoria repo) =>
        new(repo, new GeradorDeOrigemHashFake(), new CriarComentarioCommandValidator());

    private static CriarComentarioCommand Comando(string autor = "Ana", string texto = "Bom artigo.", string? armadilha = null) =>
        new(Guid.NewGuid(), "meu-artigo", autor, texto, "v-1", "203.0.113.5", "Mozilla/5.0", armadilha);

    [Fact]
    public async Task Handle_criaComentarioPendente()
    {
        var repo = new ComentarioRepositorioEmMemoria();
        var handler = Criar(repo);

        var resultado = await handler.Handle(Comando(), CancellationToken.None);

        resultado.Sucesso.ShouldBeTrue();
        repo.Adicionados.ShouldBe(1);
    }

    [Fact]
    public async Task Handle_honeypotPreenchido_descartaEmSilencio()
    {
        var repo = new ComentarioRepositorioEmMemoria();
        var handler = Criar(repo);

        var resultado = await handler.Handle(Comando(armadilha: "sou-um-bot"), CancellationToken.None);

        // Aceito (nao revela a armadilha), mas nada e persistido.
        resultado.Sucesso.ShouldBeTrue();
        repo.Adicionados.ShouldBe(0);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Handle_falhaQuandoAutorVazio(string autor)
    {
        var repo = new ComentarioRepositorioEmMemoria();
        var handler = Criar(repo);

        var resultado = await handler.Handle(Comando(autor: autor), CancellationToken.None);

        resultado.Falhou.ShouldBeTrue();
        resultado.Erro.Tipo.ShouldBe(TipoErro.Validacao);
        repo.Adicionados.ShouldBe(0);
    }

    [Fact]
    public async Task Handle_falhaQuandoTextoVazio()
    {
        var repo = new ComentarioRepositorioEmMemoria();
        var handler = Criar(repo);

        var resultado = await handler.Handle(Comando(texto: "  "), CancellationToken.None);

        resultado.Falhou.ShouldBeTrue();
        resultado.Erro.Tipo.ShouldBe(TipoErro.Validacao);
    }
}
