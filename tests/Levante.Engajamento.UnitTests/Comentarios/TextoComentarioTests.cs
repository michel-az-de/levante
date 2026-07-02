using Levante.Engajamento.Domain.Comentarios;
using Shouldly;
using Xunit;

namespace Levante.Engajamento.UnitTests.Comentarios;

[Trait("Category", "Unit")]
public sealed class TextoComentarioTests
{
    [Fact]
    public void Criar_normalizaTrim()
    {
        var texto = new TextoComentario("  ola  ");
        texto.Valor.ShouldBe("ola");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Criar_rejeitaVazio(string valor)
    {
        Should.Throw<ArgumentException>(() => new TextoComentario(valor));
    }

    [Fact]
    public void Criar_rejeitaAcimaDoMaximo()
    {
        var longo = new string('a', TextoComentario.TamanhoMaximo + 1);
        Should.Throw<ArgumentException>(() => new TextoComentario(longo));
    }

    [Fact]
    public void TryParse_falhaAcimaDoMaximo()
    {
        var longo = new string('a', TextoComentario.TamanhoMaximo + 1);
        TextoComentario.TryParse(longo, out var texto).ShouldBeFalse();
        texto.ShouldBeNull();
    }

    [Fact]
    public void TryParse_okDentroDoLimite()
    {
        TextoComentario.TryParse("valido", out var texto).ShouldBeTrue();
        texto!.Valor.ShouldBe("valido");
    }
}
