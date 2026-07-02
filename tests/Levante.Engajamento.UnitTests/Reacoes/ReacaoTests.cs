using Levante.Engajamento.Domain.Reacoes;
using Shouldly;
using Xunit;

namespace Levante.Engajamento.UnitTests.Reacoes;

[Trait("Category", "Unit")]
public sealed class ReacaoTests
{
    [Fact]
    public void Registrar_preencheCampos()
    {
        var artigoId = Guid.NewGuid();

        var reacao = Reacao.Registrar(artigoId, TipoReacao.Curtir, "visitante-1", "hash-abc");

        reacao.Id.ShouldNotBe(Guid.Empty);
        reacao.ArtigoId.ShouldBe(artigoId);
        reacao.Tipo.ShouldBe(TipoReacao.Curtir);
        reacao.Visitante.ShouldBe("visitante-1");
        reacao.OrigemHash.ShouldBe("hash-abc");
        reacao.DataCriacao.Kind.ShouldBe(DateTimeKind.Utc);
    }

    [Fact]
    public void Registrar_rejeitaArtigoVazio()
    {
        Should.Throw<ArgumentException>(() =>
            Reacao.Registrar(Guid.Empty, TipoReacao.Amei, "visitante-1", "hash"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Registrar_rejeitaVisitanteVazio(string visitante)
    {
        Should.Throw<ArgumentException>(() =>
            Reacao.Registrar(Guid.NewGuid(), TipoReacao.Curtir, visitante, "hash"));
    }

    [Fact]
    public void Reconstituir_preservaCampos()
    {
        var id = Guid.NewGuid();
        var artigoId = Guid.NewGuid();
        var data = new DateTime(2026, 1, 2, 3, 4, 5, DateTimeKind.Utc);

        var reacao = Reacao.Reconstituir(id, artigoId, TipoReacao.Relevante, "v-9", "h-9", data);

        reacao.Id.ShouldBe(id);
        reacao.ArtigoId.ShouldBe(artigoId);
        reacao.Tipo.ShouldBe(TipoReacao.Relevante);
        reacao.DataCriacao.ShouldBe(data);
    }
}
