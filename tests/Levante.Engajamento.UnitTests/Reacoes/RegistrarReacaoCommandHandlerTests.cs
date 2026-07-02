using Levante.Engajamento.Application.Reacoes.RegistrarReacao;
using Levante.Engajamento.Domain.Reacoes;
using Levante.SharedKernel;
using Shouldly;
using Xunit;

namespace Levante.Engajamento.UnitTests.Reacoes;

[Trait("Category", "Unit")]
public sealed class RegistrarReacaoCommandHandlerTests
{
    private static RegistrarReacaoCommandHandler Criar(ReacaoRepositorioEmMemoria repo) =>
        new(repo, new GeradorDeOrigemHashFake(), new RegistrarReacaoCommandValidator());

    private static RegistrarReacaoCommand Comando(Guid artigoId, TipoReacao tipo, string visitante) =>
        new(artigoId, tipo, visitante, "203.0.113.5", "Mozilla/5.0");

    [Fact]
    public async Task Handle_registraEContaContagens()
    {
        var artigoId = Guid.NewGuid();
        var repo = new ReacaoRepositorioEmMemoria();
        var handler = Criar(repo);

        var resultado = await handler.Handle(Comando(artigoId, TipoReacao.Curtir, "v-1"), CancellationToken.None);

        resultado.Sucesso.ShouldBeTrue();
        repo.Adicionadas.ShouldBe(1);
        resultado.Valor!.Curtir.ShouldBe(1);
        resultado.Valor.Minhas.ShouldContain(nameof(TipoReacao.Curtir));
    }

    [Fact]
    public async Task Handle_reagirMesmoTipoDeNovo_eIdempotente()
    {
        var artigoId = Guid.NewGuid();
        var existente = Reacao.Registrar(artigoId, TipoReacao.Curtir, "v-1", "hash:qualquer");
        var repo = new ReacaoRepositorioEmMemoria(existente);
        var handler = Criar(repo);

        var resultado = await handler.Handle(Comando(artigoId, TipoReacao.Curtir, "v-1"), CancellationToken.None);

        resultado.Sucesso.ShouldBeTrue();
        repo.Adicionadas.ShouldBe(0); // duplicata engolida
        resultado.Valor!.Curtir.ShouldBe(1);
    }

    [Fact]
    public async Task Handle_visitantesDistintos_somamContagem()
    {
        var artigoId = Guid.NewGuid();
        var repo = new ReacaoRepositorioEmMemoria();
        var handler = Criar(repo);

        await handler.Handle(Comando(artigoId, TipoReacao.Amei, "v-1"), CancellationToken.None);
        var resultado = await handler.Handle(Comando(artigoId, TipoReacao.Amei, "v-2"), CancellationToken.None);

        resultado.Valor!.Amei.ShouldBe(2);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Handle_falhaQuandoVisitanteVazio(string visitante)
    {
        var repo = new ReacaoRepositorioEmMemoria();
        var handler = Criar(repo);

        var resultado = await handler.Handle(
            Comando(Guid.NewGuid(), TipoReacao.Curtir, visitante), CancellationToken.None);

        resultado.Falhou.ShouldBeTrue();
        resultado.Erro.Tipo.ShouldBe(TipoErro.Validacao);
        repo.Adicionadas.ShouldBe(0);
    }
}
