using Levante.Engajamento.Application.Reacoes.ObterReacoesDoArtigo;
using Levante.Engajamento.Application.Reacoes.RemoverReacao;
using Levante.Engajamento.Domain.Reacoes;
using Levante.SharedKernel;
using Shouldly;
using Xunit;

namespace Levante.Engajamento.UnitTests.Reacoes;

[Trait("Category", "Unit")]
public sealed class ReacoesQueryERemocaoTests
{
    [Fact]
    public async Task Obter_contaPorTipoEListaMinhasDoVisitante()
    {
        var artigoId = Guid.NewGuid();
        var repo = new ReacaoRepositorioEmMemoria(
            Reacao.Registrar(artigoId, TipoReacao.Curtir, "v-1", "h"),
            Reacao.Registrar(artigoId, TipoReacao.Relevante, "v-1", "h"),
            Reacao.Registrar(artigoId, TipoReacao.Curtir, "v-2", "h"));
        var handler = new ObterReacoesDoArtigoQueryHandler(repo);

        var resultado = await handler.Handle(new ObterReacoesDoArtigoQuery(artigoId, "v-1"), CancellationToken.None);

        resultado.Sucesso.ShouldBeTrue();
        resultado.Valor!.Curtir.ShouldBe(2);
        resultado.Valor.Relevante.ShouldBe(1);
        resultado.Valor.Amei.ShouldBe(0);
        resultado.Valor.Minhas.ShouldBe([nameof(TipoReacao.Curtir), nameof(TipoReacao.Relevante)], ignoreOrder: true);
    }

    [Fact]
    public async Task Obter_semVisitante_naoTrazMinhas()
    {
        var artigoId = Guid.NewGuid();
        var repo = new ReacaoRepositorioEmMemoria(Reacao.Registrar(artigoId, TipoReacao.Curtir, "v-1", "h"));
        var handler = new ObterReacoesDoArtigoQueryHandler(repo);

        var resultado = await handler.Handle(new ObterReacoesDoArtigoQuery(artigoId, string.Empty), CancellationToken.None);

        resultado.Valor!.Curtir.ShouldBe(1);
        resultado.Valor.Minhas.ShouldBeEmpty();
    }

    [Fact]
    public async Task Remover_tiraReacaoEDevolveContagem()
    {
        var artigoId = Guid.NewGuid();
        var repo = new ReacaoRepositorioEmMemoria(Reacao.Registrar(artigoId, TipoReacao.Curtir, "v-1", "h"));
        var handler = new RemoverReacaoCommandHandler(repo);

        var resultado = await handler.Handle(
            new RemoverReacaoCommand(artigoId, TipoReacao.Curtir, "v-1"), CancellationToken.None);

        resultado.Sucesso.ShouldBeTrue();
        repo.Removidas.ShouldBe(1);
        resultado.Valor!.Curtir.ShouldBe(0);
        resultado.Valor.Minhas.ShouldBeEmpty();
    }

    [Fact]
    public async Task Remover_semVisitante_falhaValidacao()
    {
        var repo = new ReacaoRepositorioEmMemoria();
        var handler = new RemoverReacaoCommandHandler(repo);

        var resultado = await handler.Handle(
            new RemoverReacaoCommand(Guid.NewGuid(), TipoReacao.Curtir, "  "), CancellationToken.None);

        resultado.Falhou.ShouldBeTrue();
        resultado.Erro.Tipo.ShouldBe(TipoErro.Validacao);
    }
}
