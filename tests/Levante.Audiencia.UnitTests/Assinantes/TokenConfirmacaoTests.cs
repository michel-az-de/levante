using Levante.Audiencia.Domain.Assinantes;
using Shouldly;
using Xunit;

namespace Levante.Audiencia.UnitTests.Assinantes;

[Trait("Category", "Unit")]
public sealed class TokenConfirmacaoTests
{
    [Fact]
    public void Gerar_produzHexMinusculoDe64Caracteres()
    {
        var token = TokenConfirmacao.Gerar();

        token.Valor.Length.ShouldBe(64);
        token.Valor.ShouldBe(token.Valor.ToLowerInvariant());
        token.ToString().ShouldBe(token.Valor);
    }

    [Fact]
    public void Gerar_produzTokensDistintos() =>
        TokenConfirmacao.Gerar().Valor.ShouldNotBe(TokenConfirmacao.Gerar().Valor);

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("curto")]
    public void Ctor_rejeitaInvalido(string entrada) =>
        Should.Throw<ArgumentException>(() => new TokenConfirmacao(entrada));

    [Fact]
    public void Ctor_aceitaTamanhoMinimo()
    {
        var valor = new string('a', TokenConfirmacao.TamanhoMinimo);

        new TokenConfirmacao(valor).Valor.ShouldBe(valor);
    }
}
